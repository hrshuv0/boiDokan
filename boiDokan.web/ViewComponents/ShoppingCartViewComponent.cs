using System.Security.Claims;
using boiDokan.dal.Repository.IRepository;
using boiDokan.utility;
using Microsoft.AspNetCore.Mvc;

namespace boiDokan.web.ViewComponents;

public class ShoppingCartViewComponent : ViewComponent
{
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var claimIdentity = (ClaimsIdentity)User.Identity!;
        var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

        if (claim is not null)
        {
            if (HttpContext.Session.GetInt32(CustomStatus.SessionCart) is not null)
            {
                return View((int)HttpContext.Session.GetInt32(CustomStatus.SessionCart));
            }
            else
            {
                HttpContext.Session.SetInt32(CustomStatus.SessionCart, _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==claim.Value).ToList().Count);
                return View((int)HttpContext.Session.GetInt32(CustomStatus.SessionCart));
            }
        }
        HttpContext.Session.Clear();
        return View(0);
    }
}