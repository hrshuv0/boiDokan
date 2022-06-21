using boiDokan.entities.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace boiDokan.entities.ViewModels;

public class ProductVm
{
    public Product Product { get; set; }
    
    [ValidateNever]
    public IEnumerable<SelectListItem> CategoryList { get; set; }
    
    [ValidateNever]
    public IEnumerable<SelectListItem> CoverTypeList { get; set; }
}