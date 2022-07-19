using boiDokan.dal.Data;
using boiDokan.dal.Repository.IRepository;

namespace boiDokan.dal.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public ICategoryRepository Category { get; }
    public ICoverTypeRepository CoverType { get; }
    public IProductRepository Product { get; }
    public ICompanyRepository Company { get; }
    public IShoppingCartRepository ShoppingCart { get; }
    public IApplicationUserRepository ApplicationUser { get; }
    public IOrderDetailRepository OrderDetail { get; }
    public IOrderHeaderRepository OrderHeader { get; }

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        CoverType = new CoverTypeRepository(_dbContext);
        Category = new CategoryRepository(_dbContext);
        Product = new ProductRepository(_dbContext);
        Company = new CompanyRepository(_dbContext);
        ShoppingCart = new ShoppingCartRepository(_dbContext);
        ApplicationUser = new ApplicationUserRepository(_dbContext);
        OrderDetail = new OrderDetailRepository(_dbContext);
        OrderHeader = new OrderHeaderRepository(_dbContext);
    }

    public void Save()
    {
        _dbContext.SaveChanges();
    }
}