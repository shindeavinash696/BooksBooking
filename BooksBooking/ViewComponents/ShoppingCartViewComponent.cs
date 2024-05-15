using BooksBooking.DataAccess.Repository.IRepository;
using BooksBooking.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {

        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {

                if (HttpContext.Session.GetInt32(SD.SessionCard) == null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCard,
                    _unitOfWork.ShoppingCard.GetAll(u => u.Name == claim.Value).Count());
                }

                return View(HttpContext.Session.GetInt32(SD.SessionCard));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }

    }
}