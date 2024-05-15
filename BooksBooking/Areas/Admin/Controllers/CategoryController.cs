using BooksBooking.DataAccess.Data;
using BooksBooking.DataAccess.Repository.IRepository;
//using BooksBooking.DataAccess.Data;
using BooksBooking.Models;
using BooksBooking.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksBooking.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display Order Can't be exactly match the name.");
            }

            //if (obj.Name !=null && obj.Name.ToLower() == "Text")
            //{
            //    ModelState.AddModelError("", "Test is Invalid value");
            //}

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Category created successfully !!";
                return RedirectToAction("Index", "Category");
            }
            return View();
        }


        //================================================= EDIT CODE ==========================================================
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Edit(int? CategoryId)
        {
            if (CategoryId == null || CategoryId == 0)
            {
                return NotFound();
            }

            Category? CategoryFromDb = _unitOfWork.Category.Get(u => u.CategoryId == CategoryId);
            //Category? CategoryFromDb1 = _db.Categories.FirstOrDefault(u=>u.CategoryId==CategoryId);
            //Category? CategoryFromDb2 = _db.Categories.Where(u => u.CategoryId==CategoryId).FirstOrDefault();

            if (CategoryFromDb == null)
            {
                return NotFound();
            }
            return View(CategoryFromDb);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Edit(Category obj)
        {
            //if (obj.Name == obj.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("Name", "The Display Order Can't be exactly match the name.");
            //}

            //if (obj.Name !=null && obj.Name.ToLower() == "Text")
            //{
            //    ModelState.AddModelError("", "Test is Invalid value");
            //}

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Category updated successfully !!";
                return RedirectToAction("Index", "Category");
            }
            return View();
        }

        //================================================== DELETE CODE ========================================================
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Delete(int? CategoryId)
        {
            if (CategoryId == null || CategoryId == 0)
            {
                return NotFound();
            }

            Category? CategoryFromDb = _unitOfWork.Category.Get(u => u.CategoryId == CategoryId);


            if (CategoryFromDb == null)
            {
                return NotFound();
            }
            return View(CategoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult DeletePOST(int? CategoryId)
        {

            Category? obj = _unitOfWork.Category.Get(u => u.CategoryId == CategoryId);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["Success"] = "Category Deleted successfully !!";
            return RedirectToAction("Index", "Category");
        }
    }
}
