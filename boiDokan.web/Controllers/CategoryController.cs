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
    
    // GET
    public IActionResult Create()
    {
        return View();
    }
    
    // POST
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
}