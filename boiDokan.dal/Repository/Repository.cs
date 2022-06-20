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
        _dbSet = _dbContext.Set<T>();
    }

    public T GetFirstOrDefault(Expression<Func<T, bool>> filter)
    {
        IQueryable<T> query = _dbSet;

        query = query.Where(filter);

        return query.FirstOrDefault()!;
    }

    public IEnumerable<T> GetAll()
    {
        IQueryable<T> query = _dbSet;

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