namespace boiDokan.dal.Repository.IRepository;

public interface IUnitOfWork
{
    ICategoryRepository Category { get; }

    void Save();

}