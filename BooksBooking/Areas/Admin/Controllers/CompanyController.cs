using BooksBooking.DataAccess.Data;
using BooksBooking.DataAccess.Repository.IRepository;
//using BooksBooking.DataAccess.Data;
using BooksBooking.Models;
using BooksBooking.Models.ViewModels;
using BooksBooking.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace BooksBooking.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
       // private readonly IWebHostEnvironment _webHostEnvironment;     //using this we can access wwwroot folder
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
             //dependency injection
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            
            return View(objCompanyList);
        }
        public IActionResult Upsert(int? CompanyId)
        {
            if (CompanyId == null || CompanyId == 0)
            {
                return View(new Company());
            }
            else
            {
                Company CompanyObj = _unitOfWork.Company.Get(u => u.CompanyId == CompanyId);
                return View(CompanyObj);
            }

        }
        [HttpPost]
        public IActionResult Upsert(Company CompanyObj)
        {
            if (ModelState.IsValid) { 
                if (CompanyObj.CompanyId == 0)
                {
                    _unitOfWork.Company.Add(CompanyObj);
                }
                else
                {
                    _unitOfWork.Company.Update(CompanyObj);
                }
                
                _unitOfWork.Save();
                TempData["Success"] = "Company created successfully !!";
                return RedirectToAction("Index", "Company");
            }
            else
            {
                return View(CompanyObj);
            }

        }


        //================================================= EDIT CODE ==========================================================
        //public IActionResult Edit(int? CompanyId)
        //{
        //    if (CompanyId == null || CompanyId == 0)
        //    {
        //        return NotFound();
        //    }

        //    Company? CompanyFromDb = _unitOfWork.Company.Get(u => u.CompanyId == CompanyId);
        //    //Company? CompanyFromDb1 = _db.Categories.FirstOrDefault(u=>u.CompanyId==CompanyId);
        //    //Company? CompanyFromDb2 = _db.Categories.Where(u => u.CompanyId==CompanyId).FirstOrDefault();

        //    if (CompanyFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(CompanyFromDb);
        //}
        //[HttpPost]
        //public IActionResult Edit(Company obj)
        //{
        //    //if (obj.Name == obj.DisplayOrder.ToString())
        //    //{
        //    //    ModelState.AddModelError("Name", "The Display Order Can't be exactly match the name.");
        //    //}

        //    //if (obj.Name !=null && obj.Name.ToLower() == "Text")
        //    //{
        //    //    ModelState.AddModelError("", "Test is Invalid value");
        //    //}

        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Company.Update(obj);
        //        _unitOfWork.Save();
        //        TempData["Success"] = "Company updated successfully !!";
        //        return RedirectToAction("Index", "Company");
        //    }
        //    return View();
        //}

        //================================================== DELETE CODE ========================================================
        //public IActionResult Delete(int? CompanyId)
        //{
        //    if (CompanyId == null || CompanyId == 0)
        //    {
        //        return NotFound();
        //    }

        //    Company? CompanyFromDb = _unitOfWork.Company.Get(u => u.CompanyId == CompanyId);


        //    if (CompanyFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(CompanyFromDb);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? CompanyId)
        //{

        //    Company? obj = _unitOfWork.Company.Get(u => u.CompanyId == CompanyId);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Company.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["Success"] = "Company Deleted successfully !!";
        //    return RedirectToAction("Index", "Company");
        //}





        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new {data = objCompanyList});
        }

        [HttpDelete]
        public IActionResult Delete(int? CompanyId)
        {
            var CompanyToBeDeleted = _unitOfWork.Company.Get(u => u.CompanyId == CompanyId);

            if (CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Company.Remove(CompanyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "deleted successfully" });
        }

        #endregion
    }
}
