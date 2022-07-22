using System.Security.Claims;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities;
using boiDokan.entities.ViewModels;
using boiDokan.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace boiDokan.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    [BindProperty] public OrderVm OrderVm { get; set; }

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Details(int orderId)
    {
        OrderVm = new OrderVm()
        {
            OrderHeader =
                _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product")
        };

        return View(OrderVm);
    }

    [HttpPost]
    [Authorize(Roles = SD.RoleAdmin+","+SD.RoleEmployee)]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateOrderDetail()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVm.OrderHeader.Id,tracked: false);

        orderHeaderFromDb.Name = OrderVm.OrderHeader.Name;
        orderHeaderFromDb.PhoneNumber = OrderVm.OrderHeader.PhoneNumber;
        orderHeaderFromDb.StreetAddress = OrderVm.OrderHeader.StreetAddress;
        orderHeaderFromDb.City = OrderVm.OrderHeader.City;
        orderHeaderFromDb.State = OrderVm.OrderHeader.State;
        orderHeaderFromDb.PostalCode = OrderVm.OrderHeader.PostalCode;

        if (OrderVm.OrderHeader.Carrier is not null)
        {
            orderHeaderFromDb.Carrier = OrderVm.OrderHeader.Carrier;
        }

        if (OrderVm.OrderHeader.TrackingNumber is not null)
        {
            orderHeaderFromDb.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
        }

        // _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();

        TempData["Success"] = "Order details updated successfully";
        return RedirectToAction(nameof(Details), "Order", new { orderId = orderHeaderFromDb.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.RoleAdmin+","+SD.RoleEmployee)]
    [ValidateAntiForgeryToken]
    public IActionResult StartProcessing()
    {
        // var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVm.OrderHeader.Id,tracked: false);
        _unitOfWork.OrderHeader.UpdateStatus(OrderVm.OrderHeader.Id, CustomStatus.StatusInProcess);

        // _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();

        TempData["Success"] = "Order status updated successfully";
        return RedirectToAction(nameof(Details), "Order", new { orderId = OrderVm.OrderHeader.Id });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.RoleAdmin+","+SD.RoleEmployee)]
    public IActionResult ShipOrder()
    {
        var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVm.OrderHeader.Id,tracked: false);

        orderHeader.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
        orderHeader.Carrier = OrderVm.OrderHeader.Carrier;
        orderHeader.OrderStatus = CustomStatus.StatusShipped;
        orderHeader.ShippingDate = DateTime.Now;

        if (orderHeader.PaymentStatus == CustomStatus.PaymentStatusDelayedPayment)
        {
            orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
        }
        _unitOfWork.OrderHeader.Update(orderHeader);
        _unitOfWork.Save();

        TempData["Success"] = "Order shipped successfully";
        return RedirectToAction(nameof(Details), "Order", new { orderId = OrderVm.OrderHeader.Id });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.RoleAdmin+","+SD.RoleEmployee)]
    public IActionResult CancelOrder()
    {
        var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVm.OrderHeader.Id,tracked: false);

        if (orderHeader.PaymentStatus == CustomStatus.PaymentStatusApproved)
        {
            var options = new RefundCreateOptions()
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderHeader.PaymentIntentId
            };
            var service = new RefundService();
            Refund refund = service.Create(options);
            
            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, CustomStatus.StatusCancelled, CustomStatus.StatusRefunded);
        }
        else
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, CustomStatus.StatusCancelled, CustomStatus.StatusCancelled);
        }
        
        _unitOfWork.Save();

        TempData["Success"] = "Order cancelled successfully";
        return RedirectToAction(nameof(Details), "Order", new { orderId = OrderVm.OrderHeader.Id });
    }


    #region API CALLS

    public IActionResult GetAll(string status)
    {
        IEnumerable<OrderHeader> orderHeaders;

        if (User.IsInRole(SD.RoleAdmin) || User.IsInRole(SD.RoleEmployee))
        {
            orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
        }
        else
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim,
                includeProperties: "ApplicationUser");
        }

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