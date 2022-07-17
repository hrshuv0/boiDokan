using boiDokan.entities.Models;

namespace boiDokan.dal.Repository.IRepository;

public interface ICompanyRepository : IRepository<Company>
{
    void Update(Company obj);
    

}