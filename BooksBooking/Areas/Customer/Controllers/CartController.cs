using BooksBooking.DataAccess.Repository.IRepository;
using BooksBooking.Models;
using BooksBooking.Models.ViewModels;
using BooksBooking.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BooksBooking.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCard.GetAll(u => u.Name == userId,
                includeProperty: "Product"),
                OrderHeader = new() 
            };
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBaseOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary() 
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCard.GetAll(u => u.Name == userId,
                includeProperty: "Product"),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostCode;


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBaseOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
            
        }

        //summary post method
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCard.GetAll(u => u.Name == userId,
			   includeProperty: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBaseOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is regular customer account and we need to capture payment
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
            else
            {
				//it is company account
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.ApprovedForDelayPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetails = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.OrderHeaderId,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetails);
                _unitOfWork.Save();
            }
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				//it is a regular customer account and we need to capture payment
				//stripe logic
				var domain = "https://localhost:44330/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.OrderHeaderId}",
					CancelUrl = domain + "customer/cart/index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in ShoppingCartVM.ShoppingCartList)
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
				_unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);

			}

			return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.OrderHeaderId });
		}
		public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == id, includeProperty: "ApplicationUser");
            if(orderHeader.PaymentStatus != SD.ApprovedForDelayPayment)
            {
                //This is order by an customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if(session.PaymentStatus.ToLower() == "paid")
                {
					_unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id,SD.StatusApproved,SD.PaymentStatusApproved);
                    _unitOfWork.Save();
				}
                HttpContext.Session.Clear();
            }
            List<ShoppingCart> shoppingCard = _unitOfWork.ShoppingCard.
                GetAll(u => u.ApplicationUser == orderHeader.ApplicationUser).ToList();

            _unitOfWork.ShoppingCard.RemoveRange(shoppingCard);
            _unitOfWork.Save();
            return View(id);
        }

		public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCard.Get(u => u.ProductId == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCard.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCard.Get(u => u.ProductId == cartId, tracked: true);
            if (cartFromDb.Count <= 1)
            {
                // remove from cart
                HttpContext.Session.SetInt32(SD.SessionCard, _unitOfWork.ShoppingCard.GetAll(
                u => u.Name == cartFromDb.Name).Count() - 1);
                _unitOfWork.ShoppingCard.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCard.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCard.Get(u => u.ProductId == cartId, tracked:true);
            HttpContext.Session.SetInt32(SD.SessionCard, _unitOfWork.ShoppingCard.GetAll(
                u => u.Name == cartFromDb.Name).Count() - 1);
            _unitOfWork.ShoppingCard.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBaseOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
