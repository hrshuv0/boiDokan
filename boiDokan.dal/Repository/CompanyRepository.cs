using boiDokan.dal.Data;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;

namespace boiDokan.dal.Repository;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public CompanyRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Update(Company obj)
    {
        _dbContext.Companies!.Update(obj);
    }

    
}