using System.Diagnostics;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities;
using boiDokan.entities.Models;
using boiDokan.entities.ViewModels;
using boiDokan.models.Models;
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

    public IActionResult Details(int id)
    {
        Product product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties:"Category,CoverType");

        ShoppingCart cartObj = new()
        {
            Count = 1,
            Product = product
        };
        return View(cartObj);
    }
}