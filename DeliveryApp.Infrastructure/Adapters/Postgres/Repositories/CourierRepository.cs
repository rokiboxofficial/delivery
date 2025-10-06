using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public sealed class CourierRepository(ApplicationDbContext dbContext) : ICourierRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task AddAsync(Courier courier)
        => await _dbContext.AddAsync(courier);
    
    public void Update(Courier courier)
        => _dbContext.Couriers.Update(courier);
    
    public async Task<Maybe<Courier>> GetAsync(Guid courierId)
    {
        var courier = await _dbContext
            .Couriers
            .Include(c => c.StoragePlaces)
            .SingleOrDefaultAsync(courier => courier.Id == courierId);
        
        return courier;
    }

    public async Task<Courier[]> GetAllFreeCouriersAsync()
    {
        var freeCouriers = await _dbContext
            .Couriers
            .Where(courier => courier.StoragePlaces.All(storagePlace => storagePlace.OrderId == null))
            .Include(c => c.StoragePlaces)
            .ToArrayAsync();

        return freeCouriers;
    }
}