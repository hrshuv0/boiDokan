using boiDokan.dal.Data;
using boiDokan.dal.Repository.IRepository;
using boiDokan.entities;
using boiDokan.entities.Models;

namespace boiDokan.dal.Repository;

public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public OrderHeaderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Update(OrderHeader category)
    {
        _dbContext.OrderHeaders!.Update(category);
    }

    public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
    {
        var order = _dbContext.OrderHeaders!.FirstOrDefault(u => u.Id == id);
        if (order is not null)
        {
            order.OrderStatus = orderStatus;
            if (paymentStatus is not null)
            {
                order.PaymentStatus = paymentStatus;
            }
        }
    }
    
    public void UpdateStripePaymentId(int id, string sessionId, string? paymentIntentId = null)
    {
        var order = _dbContext.OrderHeaders!.FirstOrDefault(u => u.Id == id);
        order!.SessionId = sessionId;
        order.PaymentIntentId = paymentIntentId;
        order.PaymentDate = DateTime.Now;
    }
}