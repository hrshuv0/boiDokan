using boiDokan.entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace boiDokan.dal.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category>? Categories { get; set; }
    public DbSet<CoverType>? CoverTypes { get; set; }
    public DbSet<Product>? Products { get; set; }
    public DbSet<ApplicationUser>? ApplicationUsers { get; set; }
    public DbSet<Company>? Companies { get; set; }
}