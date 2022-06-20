using boiDokan.dal.Data;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;

namespace boiDokan.dal.Repository;

public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public CoverTypeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Update(CoverType obj)
    {
        _dbContext.CoverTypes!.Update(obj);
    }

    
}