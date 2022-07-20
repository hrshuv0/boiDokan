using boiDokan.dal.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace boiDokan.web.Areas.Admin.Controllers;

[Area("Admin")]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }


    #region API CALLS

    public IActionResult GetAll()
    {
        var orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties:"ApplicationUser");

        return Json(new { data = orderHeaders });
    }

    #endregion
}