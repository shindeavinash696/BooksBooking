using BooksBooking.DataAccess.Repository.IRepository;
using BooksBooking.Models;
using BooksBooking.Models.ViewModels;
using BooksBooking.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BooksBooking.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			return View();
		}
        public IActionResult Details(int orderId)
        {
             orderVM = new()
            {
                orderHeader = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == orderId, includeProperty: "ApplicationUser"),
                orderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderDetailId == orderId, includeProperty: "Product")
            };

            return View(orderVM);
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == orderVM.orderHeader.OrderHeaderId);
            orderHeaderFromDb.Name = orderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.orderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.orderHeader.City;
            orderHeaderFromDb.State = orderVM.orderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.orderHeader.PostalCode;

            if (!String.IsNullOrEmpty(orderVM.orderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.orderHeader.Carrier;
            }
            if (!String.IsNullOrEmpty(orderVM.orderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order details updated successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.OrderHeaderId});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.orderHeader.OrderHeaderId, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order details updated successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeaders = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == orderVM.orderHeader.OrderHeaderId);
            orderHeaders.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderHeaders.Carrier = orderVM.orderHeader.Carrier;
            orderHeaders.OrderStatus = SD.StatusShipped;
            orderHeaders.ShoppingDate = DateTime.Now;
            if(orderHeaders.PaymentStatus == SD.ApprovedForDelayPayment)
            {
                orderHeaders.PaymentDueDate = DateOnly.FromDateTime( DateTime.Now.AddDays(30));

            }
            _unitOfWork.OrderHeader.Update(orderHeaders);
            _unitOfWork.Save();
            
            TempData["Success"] = "Order Shipped successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeaders = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == orderVM.orderHeader.OrderHeaderId);
            if(orderHeaders.PaymentStatus == SD.ApprovedForDelayPayment)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaders.PaymentIntenId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeaders.OrderHeaderId, SD.StatusCancelled, SD.StatusRefund);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaders.OrderHeaderId, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.OrderHeaderId });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            orderVM.orderHeader = _unitOfWork.OrderHeader.
                Get(u => u.OrderHeaderId == orderVM.orderHeader.OrderHeaderId, includeProperty: "ApplicationUser");

            orderVM.orderDetail = _unitOfWork.OrderDetail.
                GetAll(u => u.OrderDetailId == orderVM.orderHeader.OrderHeaderId, includeProperty: "Product");
            //stripe logic
            var domain = "https://localhost:44330/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?OrderHeaderId={orderVM.orderHeader.OrderHeaderId}",
                CancelUrl = domain + $"admin/order/details?{orderVM.orderHeader.OrderHeaderId}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderVM.orderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(orderVM.orderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int OrderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == OrderHeaderId);
            if (orderHeader.PaymentStatus == SD.ApprovedForDelayPayment)
            {
                //This is order by an company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(OrderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
           
            return View(OrderHeaderId);
        }

        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status)
		{
            IEnumerable<OrderHeader> objOrderHeaders;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperty: "ApplicationUser").ToList();
            }
            else
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperty: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.ApprovedForDelayPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = objOrderHeaders });
		}
		#endregion
	}
}
