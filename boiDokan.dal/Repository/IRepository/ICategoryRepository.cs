using boiDokan.entities.Models;

namespace boiDokan.dal.Repository.IRepository;

public interface ICategoryRepository : IRepository<Category>
{
    void Update(Category category);
    

}