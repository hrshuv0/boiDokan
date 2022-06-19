using boiDokan.web.Models;
using Microsoft.EntityFrameworkCore;

namespace boiDokan.web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category>? Categories { get; set; }
}