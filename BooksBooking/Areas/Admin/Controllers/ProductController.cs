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
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;     //using this we can access wwwroot folder
        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;  //dependency injection
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperty: "Category").ToList();
            
            return View(objProductList);
        }
        public IActionResult Upsert(int? ProductId)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.CategoryId.ToString(),
                }),
            Product = new Product()
            };
            if (ProductId == null || ProductId == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.ProductId == ProductId);
                return View(productVM);
            }

        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM,IFormFile? file)
        {
            

            //if (obj.Name !=null && obj.Name.ToLower() == "Text")
            //{
            //    ModelState.AddModelError("", "Test is Invalid value");
            //}

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);             //giving random name to the file        //image name take
                    string productPath = Path.Combine(wwwRootPath, @"images\Product");                        //save file location

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //delete old img
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"images\Product"+ fileName;
                }
                if (productVM.Product.ProductId == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }
                
                _unitOfWork.Save();
                TempData["Success"] = "Product created successfully !!";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.CategoryId.ToString(),
                });

                return View(productVM);
            }

        }


        //================================================= EDIT CODE ==========================================================
        //public IActionResult Edit(int? ProductId)
        //{
        //    if (ProductId == null || ProductId == 0)
        //    {
        //        return NotFound();
        //    }

        //    Product? productFromDb = _unitOfWork.Product.Get(u => u.ProductId == ProductId);
        //    //Product? ProductFromDb1 = _db.Categories.FirstOrDefault(u=>u.ProductId==ProductId);
        //    //Product? ProductFromDb2 = _db.Categories.Where(u => u.ProductId==ProductId).FirstOrDefault();

        //    if (productFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productFromDb);
        //}
        //[HttpPost]
        //public IActionResult Edit(Product obj)
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
        //        _unitOfWork.Product.Update(obj);
        //        _unitOfWork.Save();
        //        TempData["Success"] = "Product updated successfully !!";
        //        return RedirectToAction("Index", "Product");
        //    }
        //    return View();
        //}

        //================================================== DELETE CODE ========================================================
        //public IActionResult Delete(int? ProductId)
        //{
        //    if (ProductId == null || ProductId == 0)
        //    {
        //        return NotFound();
        //    }

        //    Product? ProductFromDb = _unitOfWork.Product.Get(u => u.ProductId == ProductId);


        //    if (ProductFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(ProductFromDb);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? ProductId)
        //{

        //    Product? obj = _unitOfWork.Product.Get(u => u.ProductId == ProductId);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Product.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["Success"] = "Product Deleted successfully !!";
        //    return RedirectToAction("Index", "Product");
        //}


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperty: "Category").ToList();
            return Json(new { data = objProductList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.ProductId == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
