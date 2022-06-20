using boiDokan.dal.Data;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;

namespace boiDokan.dal.Repository;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Update(Category category)
    {
        _dbContext.Categories!.Update(category);
    }

    
}