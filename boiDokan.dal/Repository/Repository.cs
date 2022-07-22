using System.Linq.Expressions;
using boiDokan.dal.Data;
using boiDokan.dal.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace boiDokan.dal.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        // _dbContext.Products!.Include(u => u.Category).Include(u => u.CoverType);
        // _dbContext.ShoppingCarts!.Include(u => u.Product);
        _dbSet = _dbContext.Set<T>();
    }

    // includeProperties - "Category, CoverType"
    public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties=null, bool tracked=true)
    {
        IQueryable<T> query;
        
        if (tracked)
        {
            query = _dbSet;
        }
        else
        {
            query = _dbSet.AsNoTracking();
        }

        query = query.Where(filter);
        
        if (includeProperties is not null)
        {
            foreach (var includeProp in includeProperties.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }

        return query.FirstOrDefault()!;
    }

    public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties=null)
    {
        IQueryable<T> query = _dbSet;
        
        if(filter is not null)
            query = query.Where(filter);

        if (includeProperties is not null)
        {
            foreach (var includeProp in includeProperties.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }

        return query.ToList();
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entity)
    {
        _dbSet.RemoveRange(entity);
    }
}