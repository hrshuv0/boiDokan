using System.Security.Claims;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities;
using boiDokan.entities.ViewModels;
using boiDokan.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                .GetAll(u => u.ApplicationUserId == claim!.Value, includeProperties:"Product"),
            OrderHeader = new OrderHeader()
        };

        foreach (var cart in ShoppingCartVm.CartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product!.Price, cart.Product.Price50, cart.Product.Price100);
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
            _unitOfWork.ShoppingCart.Remove(cart);
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

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVm = new ShoppingCartVm()
        {
            CartList = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == claim!.Value, includeProperties:"Product"),
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
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product!.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVm.OrderHeader!.OrderTotal += (cart.Price * cart.Count);
        }
        
        return View(ShoppingCartVm);
    }
    
    [HttpPost]
    [ActionName("Summary")]
    [ValidateAntiForgeryToken]
    public IActionResult SummaryPost(ShoppingCartVm shoppingCartVm)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        shoppingCartVm.CartList = _unitOfWork.ShoppingCart
            .GetAll(u => u.ApplicationUserId == claim!.Value, includeProperties: "Product");

        shoppingCartVm.OrderHeader!.PaymentStatus = CustomStatus.PaymentStatusPending;
        shoppingCartVm.OrderHeader.OrderStatus = CustomStatus.StatusPending;
        shoppingCartVm.OrderHeader.OrderDate = DateTime.Now;
        shoppingCartVm.OrderHeader.ApplicationUserId = claim!.Value;

        var shoppingCarts = shoppingCartVm.CartList.ToList();
        foreach (var cart in shoppingCarts)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product!.Price, cart.Product.Price50, cart.Product.Price100);
            shoppingCartVm.OrderHeader!.OrderTotal += (cart.Price * cart.Count);
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
        _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
        _unitOfWork.Save();
        
        return RedirectToAction(nameof(Index), "Home");
    }
}