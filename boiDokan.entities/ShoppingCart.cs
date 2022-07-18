using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using boiDokan.entities.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace boiDokan.entities;

public class ShoppingCart
{
    [Key]
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    [ValidateNever]
    public Product? Product { get; set; }
    
    [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
    public int Count { get; set; }

    public string? ApplicationUserId { get; set; }
    [ForeignKey("ApplicationUserId")]
    [ValidateNever]
    public ApplicationUser? ApplicationUser { get; set; }
}