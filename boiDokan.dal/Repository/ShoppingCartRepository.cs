using boiDokan.dal.Data;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities;
using boiDokan.entities.Models;

namespace boiDokan.dal.Repository;

public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public ShoppingCartRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }


    public int IncrementCount(ShoppingCart shoppingCart, int count)
    {
        shoppingCart.Count += count;
        return shoppingCart.Count;
    }

    public int DecrementCount(ShoppingCart shoppingCart, int count)
    {
        shoppingCart.Count -= count;
        return shoppingCart.Count;
    }
}