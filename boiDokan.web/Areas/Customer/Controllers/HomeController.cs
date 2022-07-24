using System.Diagnostics;
using System.Security.Claims;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities;
using boiDokan.entities.Models;
using boiDokan.entities.ViewModels;
using boiDokan.models.Models;
using boiDokan.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boiDokan.web.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
        return View(productList);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Details(int productId)
    {
        Product product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties:"Category,CoverType");

        ShoppingCart cartObj = new()
        {
            Count = 1,
            ProductId = productId,
            Product = product
        };
        return View(cartObj);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details(ShoppingCart cart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        cart.ApplicationUserId = claim!.Value;

        ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(
            u => u.ApplicationUserId == claim.Value && 
                 u.ProductId == cart.ProductId);

        if (shoppingCart is null)
        {
            _unitOfWork.ShoppingCart.Add(cart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(CustomStatus.SessionCart, 
                _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId == claim.Value).ToList().Count);
        }
        else
        {
            _unitOfWork.ShoppingCart.IncrementCount(shoppingCart, cart.Count);
            _unitOfWork.Save();
        }
        

        return RedirectToAction(nameof(Index));
    }
}