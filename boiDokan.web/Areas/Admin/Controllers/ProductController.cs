using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;
using boiDokan.entities.ViewModels;
using boiDokan.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace boiDokan.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.RoleAdmin)]
public class ProductController : Controller
{
    // private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _hostEnvironment = hostEnvironment;
    }


    // GET
    public IActionResult Index()
    {
        // IEnumerable<CoverType> coverTypeList = _unitOfWork.CoverType.GetAll();
        return View();
    }


    [HttpGet]
    public IActionResult UpSert(int? id)
    {
        ProductVm productVm = new ProductVm()
        {
            Product = new Product(),
            CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem()
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }),
            CoverTypeList = _unitOfWork.CoverType.GetAll().Select(c => new SelectListItem()
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }),
        };

        if (id is null or 0)
        {
            // ViewBag.CategoryList = categoryList;
            // ViewData["CoverTypeList"] = coverTypeList;
            return View(productVm);
        }
        else
        {
            productVm.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            return View(productVm);
            // update product
        }

        return View(productVm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpSert(ProductVm model, IFormFile? file)
    {
        if (!ModelState.IsValid) return View(model);

        string wwwRootPath = _hostEnvironment.WebRootPath;
        if (file is not null)
        {
            string fileName = Guid.NewGuid().ToString();
            var uploads = Path.Combine(wwwRootPath, @"images/products");
            var extension = Path.GetExtension(file.FileName);

            if (model.Product.ImageUrl is not null)
            {
                var oldImagePath = Path.Combine(wwwRootPath, model.Product.ImageUrl.Trim('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
            {
                file.CopyTo(fileStreams);
            }

            model.Product.ImageUrl = @"/images/products/" + fileName + extension;
        }

        if (model.Product.Id == 0)
        {
            _unitOfWork.Product.Add(model.Product);
        }
        else
        {
            _unitOfWork.Product.Update(model.Product);
        }

        _unitOfWork.Save();

        TempData["success"] = "Product updated successfully";
        return RedirectToAction(nameof(Index));
    }


    // [HttpGet]
    // public IActionResult Delete(int? id)
    // {
    //     if (id is null or 0) return NotFound();
    //
    //     // var category = _dbContext.Categories!.Find(id);
    //     var coverType = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
    //     // if (coverType is null) return NotFound();
    //
    //     return View(coverType);
    // }


    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
        return Json(new { data = productList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var product = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);

        if (product is null)
            return Json(new
            {
                success = false,
                message = "Error while deleting"
            });

        var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl!.Trim('/'));
        if (System.IO.File.Exists(oldImagePath))
        {
            System.IO.File.Delete(oldImagePath);
        }
        
        _unitOfWork.Product.Remove(product);
        _unitOfWork.Save();

        return Json(new
        {
            success = true,
            message = "Delete Successful"
        });
    }

    #endregion
}