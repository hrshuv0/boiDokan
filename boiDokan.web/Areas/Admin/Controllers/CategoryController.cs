using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;
using boiDokan.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boiDokan.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.RoleAdmin)]
public class CategoryController : Controller
{
    // private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    // GET
    public IActionResult Index()
    {
        IEnumerable<Category> categoryList = _unitOfWork.Category.GetAll();
        
        return View(categoryList);
    }
    
    
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category model)
    {
        if (model.Name == model.DisplayOrder.ToString())
        {
            ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name");
        }
        if (!ModelState.IsValid) return View(model);

        _unitOfWork.Category.Add(model);
        _unitOfWork.Save();
        
        TempData["success"] = "Category created successfully";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id is null or 0) return NotFound();

        // var category = _dbContext.Categories!.Find(id);
        var category = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
        if (category is null) return NotFound();
        
        return View(category);
    }
    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category model)
    {
        if (model.Name == model.DisplayOrder.ToString())
        {
            ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name");
        }
        if (!ModelState.IsValid) return View(model);

        _unitOfWork.Category.Update(model);
        _unitOfWork.Save();
        
        TempData["success"] = "Category updated successfully";
        return RedirectToAction(nameof(Index));
    }
    
    
    [HttpGet]
    public IActionResult Delete(int? id)
    {
        if (id is null or 0) return NotFound();

        // var category = _dbContext.Categories!.Find(id);
        var category = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
        if (category is null) return NotFound();
        
        return View(category);
    }
    
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var category = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);

        if (category is null) return NotFound();

        _unitOfWork.Category.Remove(category);
        _unitOfWork.Save();
        
        TempData["delete"] = "Category deleted successfully";
        return RedirectToAction(nameof(Index));
    }
    
    
}