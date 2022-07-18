using boiDokan.dal.Data;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities.Models;

namespace boiDokan.dal.Repository;

public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public ApplicationUserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    

    
}