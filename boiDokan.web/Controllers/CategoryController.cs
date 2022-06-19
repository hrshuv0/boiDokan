using boiDokan.web.Data;
using boiDokan.web.Models;
using Microsoft.AspNetCore.Mvc;

namespace boiDokan.web.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET
    public IActionResult Index()
    {
        IEnumerable<Category> objCategoryList = _dbContext.Categories!.ToList();
        
        return View(objCategoryList);
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

        _dbContext.Categories!.Add(model);
        _dbContext.SaveChanges();
        
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id is null or 0) return NotFound();

        var category = _dbContext.Categories!.Find(id);
        // var categoryFirst = _dbContext.Categories!.FirstOrDefault(c => c.Id == id);
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

        _dbContext.Categories!.Update(model);
        _dbContext.SaveChanges();
        
        return RedirectToAction(nameof(Index));
    }
    
    
    [HttpGet]
    public IActionResult Delete(int? id)
    {
        if (id is null or 0) return NotFound();

        var category = _dbContext.Categories!.Find(id);
        // var categoryFirst = _dbContext.Categories!.FirstOrDefault(c => c.Id == id);
        if (category is null) return NotFound();
        
        return View(category);
    }
    
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var category = _dbContext.Categories!.Find(id);

        if (category is null) return NotFound();

        _dbContext.Categories.Remove(category);
        _dbContext.SaveChanges();
        
        return RedirectToAction(nameof(Index));
    }
    
    
}