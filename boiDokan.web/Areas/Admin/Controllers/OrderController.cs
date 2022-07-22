using System.Security.Claims;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities;
using boiDokan.entities.ViewModels;
using boiDokan.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

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
    [ActionName("Details")]
    [ValidateAntiForgeryToken]
    public IActionResult DetailsPayNow()
    {
        OrderVm.OrderHeader =
            _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVm.OrderHeader.Id,
                includeProperties: "ApplicationUser");
        OrderVm.OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVm.OrderHeader.Id,
            includeProperties: "Product");

        //stripe settings
        var domain = "https://localhost:7257/";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string>()
            {
                "card"
            },
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVm.OrderHeader.Id}",
            CancelUrl = domain + $"admin/order/details?orderId={OrderVm.OrderHeader.Id}",
        };

        foreach (var item in OrderVm.OrderDetails)
        {
            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product!.Title,
                    },
                },
                Quantity = item.Count,
            };

            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);

        _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVm.OrderHeader.Id, session.Id,
            session.PaymentIntentId);
        _unitOfWork.Save();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }
    
    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderId);

        if (orderHeader.PaymentStatus == CustomStatus.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            //check the stripe status

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus!,
                    CustomStatus.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }

        return View(orderHeaderId);
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

        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
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