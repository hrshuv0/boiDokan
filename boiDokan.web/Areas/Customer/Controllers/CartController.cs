using System.Security.Claims;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities;
using boiDokan.entities.Models;
using boiDokan.entities.ViewModels;
using boiDokan.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace boiDokan.web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private ShoppingCartVm ShoppingCartVm { get; set; }
    public int OrderTotal { get; set; }

    public CartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVm = new ShoppingCartVm()
        {
            CartList = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == claim!.Value, includeProperties: "Product"),
            OrderHeader = new OrderHeader()
        };

        foreach (var cart in ShoppingCartVm.CartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product!.Price, cart.Product.Price50,
                cart.Product.Price100);
            ShoppingCartVm.OrderHeader!.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVm);
    }

    private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
    {
        switch (quantity)
        {
            case <= 50:
                return price;
            case <= 100:
                return price50;
            default:
                return price100;
        }
    }

    public IActionResult Plus(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
        _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Minus(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);

        if (cart.Count <= 1)
        {
            _unitOfWork.ShoppingCart.Remove(cart);
            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList()
                .Count;
            HttpContext.Session.SetInt32(CustomStatus.SessionCart, count);
        }
        else
            _unitOfWork.ShoppingCart.DecrementCount(cart, 1);

        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Remove(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
        _unitOfWork.ShoppingCart.Remove(cart);
        _unitOfWork.Save();

        var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
        HttpContext.Session.SetInt32(CustomStatus.SessionCart, count);
            
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVm = new ShoppingCartVm()
        {
            CartList = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == claim!.Value, includeProperties: "Product"),
            OrderHeader = new OrderHeader()
        };

        ShoppingCartVm.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
            .GetFirstOrDefault(u => u.Id == claim!.Value);

        ShoppingCartVm.OrderHeader.Name = ShoppingCartVm.OrderHeader.ApplicationUser.Name;
        ShoppingCartVm.OrderHeader.PhoneNumber = ShoppingCartVm.OrderHeader.ApplicationUser.PhoneNumber;
        ShoppingCartVm.OrderHeader.StreetAddress = ShoppingCartVm.OrderHeader.ApplicationUser.StreetAddress;
        ShoppingCartVm.OrderHeader.City = ShoppingCartVm.OrderHeader.ApplicationUser.City;
        ShoppingCartVm.OrderHeader.State = ShoppingCartVm.OrderHeader.ApplicationUser.State;
        ShoppingCartVm.OrderHeader.PostalCode = ShoppingCartVm.OrderHeader.ApplicationUser.PostalCode;

        foreach (var cart in ShoppingCartVm.CartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product!.Price, cart.Product.Price50,
                cart.Product.Price100);
            ShoppingCartVm.OrderHeader!.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVm);
    }

    [HttpPost]
    [ActionName("Summary")]
    [ValidateAntiForgeryToken]
    public IActionResult SummaryPost(ShoppingCartVm shoppingCartVm)
    {
        if (shoppingCartVm.OrderHeader!.PhoneNumber is null ||
            shoppingCartVm.OrderHeader.StreetAddress is null ||
            shoppingCartVm.OrderHeader.City is null ||
            shoppingCartVm.OrderHeader.State is null ||
            shoppingCartVm.OrderHeader.PostalCode is null)
        {
            return View(shoppingCartVm);
        }


        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        shoppingCartVm.CartList = _unitOfWork.ShoppingCart
            .GetAll(u => u.ApplicationUserId == claim!.Value, includeProperties: "Product");

        shoppingCartVm.OrderHeader.OrderDate = DateTime.Now;
        shoppingCartVm.OrderHeader.ApplicationUserId = claim!.Value;

        var shoppingCarts = shoppingCartVm.CartList.ToList();
        foreach (var cart in shoppingCarts)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product!.Price, cart.Product.Price50,
                cart.Product.Price100);
            shoppingCartVm.OrderHeader!.OrderTotal += (cart.Price * cart.Count);
        }

        ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            shoppingCartVm.OrderHeader!.PaymentStatus = CustomStatus.PaymentStatusPending;
            shoppingCartVm.OrderHeader.OrderStatus = CustomStatus.StatusPending;
        }
        else
        {
            shoppingCartVm.OrderHeader!.PaymentStatus = CustomStatus.PaymentStatusDelayedPayment;
            shoppingCartVm.OrderHeader.OrderStatus = CustomStatus.StatusApproved;
        }

        _unitOfWork.OrderHeader.Add(shoppingCartVm.OrderHeader);
        _unitOfWork.Save();

        foreach (var cart in shoppingCarts)
        {
            OrderDetail orderDetail = new()
            {
                ProductId = cart.ProductId,
                OrderId = shoppingCartVm.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count
            };
            _unitOfWork.OrderDetail.Add(orderDetail);
            _unitOfWork.Save();
        }

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            //stripe settings
            // var domain = "https://localhost:7257/";
            var domain = "https://boidokan.azurewebsites.net/";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>()
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVm.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/index",
            };

            foreach (var item in shoppingCarts)
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

            _unitOfWork.OrderHeader.UpdateStripePaymentId(shoppingCartVm.OrderHeader.Id, session.Id,
                session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        return RedirectToAction(nameof(OrderConfirmation), "Cart", new { id = shoppingCartVm.OrderHeader.Id });
    }

    public IActionResult OrderConfirmation(int id)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

        if (orderHeader.PaymentStatus != CustomStatus.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            //check the stripe status

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(id, CustomStatus.StatusApproved,
                    CustomStatus.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }

        List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
            .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
        _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
        _unitOfWork.Save();

        return View(id);
    }
}