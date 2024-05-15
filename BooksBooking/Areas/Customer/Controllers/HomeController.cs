using BooksBooking.DataAccess.Repository;
using BooksBooking.DataAccess.Repository.IRepository;
using BooksBooking.Models;
using BooksBooking.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BooksBooking.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork; 
        }

        public IActionResult Index()
        {
            
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperty:"Category");
            return View(productList);
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.ProductId == productId, includeProperty: "Category"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.Name = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCard.Get(u => u.Name == userId &&
                u.ProductId == shoppingCart.ProductId);

            if(cartFromDb != null)
            {
                //shopping cart exist
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCard.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                //add cart record
                _unitOfWork.ShoppingCard.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCard,
                    _unitOfWork.ShoppingCard.GetAll(u => u.Name == userId).Count());
            }
           
            TempData["Success"] = "Cart Updated successfully !!";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}