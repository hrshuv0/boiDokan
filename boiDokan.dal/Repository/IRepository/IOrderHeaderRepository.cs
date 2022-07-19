using boiDokan.entities;

namespace boiDokan.dal.Repository.IRepository;

public interface IOrderHeaderRepository : IRepository<OrderHeader>
{
    void Update(OrderHeader obj);

    void UpdateStatus(int id, string orderStatus, string? paymentStatus=null);


}