using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;
using boiDokan.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boiDokan.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.RoleAdmin)]
public class CoverTypeController : Controller
{
    // private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CoverTypeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    // GET
    public IActionResult Index()
    {
        IEnumerable<CoverType> coverTypeList = _unitOfWork.CoverType.GetAll();
        
        return View(coverTypeList);
    }
    
    
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CoverType model)
    {
        if (!ModelState.IsValid) return View(model);

        _unitOfWork.CoverType.Add(model);
        _unitOfWork.Save();
        
        TempData["success"] = "Category created successfully";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id is null or 0) return NotFound();

        var coverType = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
        if (coverType is null) return NotFound();
        
        return View(coverType);
    }
    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(CoverType model)
    {
        if (!ModelState.IsValid) return View(model);

        _unitOfWork.CoverType.Update(model);
        _unitOfWork.Save();
        
        TempData["success"] = "Category updated successfully";
        return RedirectToAction(nameof(Index));
    }
    
    
    [HttpGet]
    public IActionResult Delete(int? id)
    {
        if (id is null or 0) return NotFound();

        // var category = _dbContext.Categories!.Find(id);
        var coverType = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
        if (coverType is null) return NotFound();
        
        return View(coverType);
    }
    
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var coverType = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);

        if (coverType is null) return NotFound();

        _unitOfWork.CoverType.Remove(coverType);
        _unitOfWork.Save();
        
        TempData["delete"] = "CoverType deleted successfully";
        return RedirectToAction(nameof(Index));
    }
    
    
}