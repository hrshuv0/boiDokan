using boiDokan.entities.Models;
using Microsoft.EntityFrameworkCore;

namespace boiDokan.dal.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category>? Categories { get; set; }
    public DbSet<CoverType>? CoverTypes { get; set; }
    public DbSet<Product>? Products { get; set; }
}