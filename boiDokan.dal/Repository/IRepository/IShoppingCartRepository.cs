using boiDokan.entities;

namespace boiDokan.dal.Repository.IRepository;

public interface IShoppingCartRepository : IRepository<ShoppingCart>
{
    int IncrementCount(ShoppingCart shoppingCart, int count);
    int DecrementCount(ShoppingCart shoppingCart, int count);
}