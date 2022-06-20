using System.ComponentModel.DataAnnotations;

namespace boiDokan.entities.Models;

public class Category
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string? Name { get; set; }
    
    [Display(Name = "Display Order")]
    [Range(1,100, ErrorMessage = "Display Order must be between 1 and 100 only!!")]
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedDateTime { get; set; } = DateTime.Now;
}