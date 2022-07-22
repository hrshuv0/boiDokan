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
public class CompanyController : Controller
{
    // private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
        Company company = new ();

        if (id is null or 0)
        {
            return View(company);
        }
        else
        {
            company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
            return View(company);
        }

        return View(company);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpSert(Company model)
    {
        if (!ModelState.IsValid) return View(model);
        
        if (model.Id == 0)
        {
            _unitOfWork.Company.Add(model);
            TempData["success"] = "Company created successfully";
        }
        else
        {
            _unitOfWork.Company.Update(model);
            TempData["success"] = "Company updated successfully";
        }

        _unitOfWork.Save();

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
        var companyList = _unitOfWork.Company.GetAll();
        return Json(new { data = companyList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var company = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);

        if (company is null)
            return Json(new
            {
                success = false,
                message = "Error while deleting"
            });

        _unitOfWork.Company.Remove(company);
        _unitOfWork.Save();

        return Json(new
        {
            success = true,
            message = "Delete Successful"
        });
    }

    #endregion
}