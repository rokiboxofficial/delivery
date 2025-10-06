using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public sealed class OrderRepository(ApplicationDbContext dbContext) : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task AddAsync(Order order)
        => await _dbContext.Orders.AddAsync(order);

    public void Update(Order order)
        => _dbContext.Orders.Update(order);

    public async Task<Maybe<Order>> GetAsync(Guid orderId)
    {
        var order = await _dbContext
            .Orders
            .SingleOrDefaultAsync(order => order.Id == orderId);
        
        return order;
    }

    public async Task<Maybe<Order>> GetFirstInCreatedStatusAsync()
    {
        var order = await _dbContext
            .Orders
            .FirstOrDefaultAsync(order => order.Status.Name == OrderStatus.Created.Name);
        
        return order;
    }

    public async Task<Order[]> GetAllInAssignedStatusAsync()
    {
        var orders = await _dbContext
            .Orders
            .Where(order => order.Status.Name == OrderStatus.Assigned.Name)
            .ToArrayAsync();
        
        return orders;
    }
}