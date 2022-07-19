using boiDokan.entities;

namespace boiDokan.dal.Repository.IRepository;

public interface IOrderDetailRepository : IRepository<OrderDetail>
{
    void Update(OrderDetail obj);
    

}