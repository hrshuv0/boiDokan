using boiDokan.dal.Repository.IRepository;
using boiDokan.utility;
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

    public IActionResult GetAll(string status)
    {
        var orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties:"ApplicationUser");

        switch (status)
        {
            case "pending":
                orderHeaders = orderHeaders.Where(u => u.PaymentStatus == CustomStatus.PaymentStatusDelayedPayment);
                break;
            case "inprocess":
                orderHeaders = orderHeaders.Where(u => u.OrderStatus == CustomStatus.StatusInProcess);
                break;
            case "completed":
                orderHeaders = orderHeaders.Where(u => u.OrderStatus == CustomStatus.StatusShipped);
                break;
            case "approved":
                orderHeaders = orderHeaders.Where(u => u.OrderStatus == CustomStatus.StatusApproved);
                break;
            default:
                break;
        }
        return Json(new { data = orderHeaders });
    }

    #endregion
}