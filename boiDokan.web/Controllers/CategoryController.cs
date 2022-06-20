using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace boiDokan.web.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryRepository _repository;

    public CategoryController(ICategoryRepository repository)
    {
        _repository = repository;
    }


    // GET
    public IActionResult Index()
    {
        IEnumerable<Category> categoryList = _repository.GetAll();
        
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

        _repository.Add(model);
        _repository.Save();
        
        TempData["success"] = "Category created successfully";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id is null or 0) return NotFound();

        // var category = _dbContext.Categories!.Find(id);
        var category = _repository.GetFirstOrDefault(c => c.Id == id);
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

        _repository.Update(model);
        _repository.Save();
        
        TempData["success"] = "Category updated successfully";
        return RedirectToAction(nameof(Index));
    }
    
    
    [HttpGet]
    public IActionResult Delete(int? id)
    {
        if (id is null or 0) return NotFound();

        // var category = _dbContext.Categories!.Find(id);
        var category = _repository.GetFirstOrDefault(c => c.Id == id);
        if (category is null) return NotFound();
        
        return View(category);
    }
    
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var category = _repository.GetFirstOrDefault(c => c.Id == id);

        if (category is null) return NotFound();

        _repository.Remove(category);
        _repository.Save();
        
        TempData["delete"] = "Category deleted successfully";
        return RedirectToAction(nameof(Index));
    }
    
    
}