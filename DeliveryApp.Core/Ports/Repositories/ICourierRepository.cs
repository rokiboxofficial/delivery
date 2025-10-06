using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Primitives;

namespace DeliveryApp.Core.Ports.Repositories;

public interface ICourierRepository : IRepository<Courier>
{
    public Task AddAsync(Courier courier);
    public void Update(Courier courier);
    public Task<Maybe<Courier>> GetAsync(Guid courierId);
    public Task<Courier[]> GetAllFreeCouriersAsync();
}