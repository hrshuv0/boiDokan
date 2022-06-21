using boiDokan.entities.Models;

namespace boiDokan.dal.Repository.IRepository;

public interface IProductRepository : IRepository<Product>
{
    void Update(Product obj);
    

}