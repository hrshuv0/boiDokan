using boiDokan.dal.Data;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;
using Microsoft.VisualBasic.CompilerServices;

namespace boiDokan.dal.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Update(Product obj)
    {
        var product = _dbContext.Products!.FirstOrDefault(p => p.Id == obj.Id);

        if (product is null) return;
        
        product.Title = obj.Title;
        product.ISBN = obj.ISBN;
        product.Price = obj.Price;
        product.Price50 = obj.Price50;
        product.Price100 = obj.Price100;
        product.ListPrice = obj.ListPrice;
        product.Description = obj.Description;
        product.Author = obj.Author;
        product.CategoryId = obj.CategoryId;
        product.CoverTypeId = obj.CoverTypeId;

        if (obj.ImageUrl is not null)
        {
            product.ImageUrl = obj.ImageUrl;
        }
    }

    
}