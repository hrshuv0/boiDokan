using boiDokan.entities.Models;
using Microsoft.EntityFrameworkCore;

namespace boiDokan.dal.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category>? Categories { get; set; }
}