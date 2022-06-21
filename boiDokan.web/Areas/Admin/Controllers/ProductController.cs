using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;
using boiDokan.entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace boiDokan.web.Areas.Admin.Controllers;

[Area("Admin")]
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

            using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
            {
                file.CopyTo(fileStreams);
            }

            model.Product.ImageUrl = @"/images/products/" + fileName + extension;
        }
        _unitOfWork.Product.Add(model.Product);
        
        _unitOfWork.Save();

        TempData["success"] = "Product updated successfully";
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public IActionResult Delete(int? id)
    {
        if (id is null or 0) return NotFound();

        // var category = _dbContext.Categories!.Find(id);
        var coverType = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
        // if (coverType is null) return NotFound();

        return View(coverType);
    }


    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var product = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);

        // if (product is null) return NotFound();

        _unitOfWork.Product.Remove(product);
        _unitOfWork.Save();

        TempData["delete"] = "Product deleted successfully";
        return RedirectToAction(nameof(Index));
    }


    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        var productList = _unitOfWork.Product.GetAll();
        return Json(new { data = productList });
    }

    

    #endregion
    
}