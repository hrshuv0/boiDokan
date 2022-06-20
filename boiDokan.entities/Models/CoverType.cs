using System.ComponentModel.DataAnnotations;

namespace boiDokan.entities.Models;

public class CoverType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [Display(Name = "Cover Type")]
    [MaxLength(50)]
    public string? Name { get; set; }
}