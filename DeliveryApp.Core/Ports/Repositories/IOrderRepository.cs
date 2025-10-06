using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Ports.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    public Task AddAsync(Order order);
    public void Update(Order order);
    public Task<Maybe<Order>> GetAsync(Guid orderId);
    public Task<Maybe<Order>> GetFirstInCreatedStatusAsync();
    public Task<Order[]> GetAllInAssignedStatusAsync();
}