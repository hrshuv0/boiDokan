using boiDokan.entities.Models;

namespace boiDokan.dal.Repository.IRepository;

public interface ICoverTypeRepository : IRepository<CoverType>
{
    void Update(CoverType obj);
    

}