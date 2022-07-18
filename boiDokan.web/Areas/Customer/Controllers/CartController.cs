using System.Security.Claims;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.ViewModels;
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
                .GetAll(u => u.ApplicationUserId == claim.Value, includeProperties:"Product")
        };

        foreach (var cart in ShoppingCartVm.CartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product!.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVm.CartTotal += (cart.Price * cart.Count);
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
        return View();
    }
}