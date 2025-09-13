using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public sealed class StoragePlace : Entity<Guid>
{
    [ExcludeFromCodeCoverage]
    private StoragePlace()
    {
        
    }

    private StoragePlace(string name, int totalVolume)
    {
        Id = Guid.NewGuid();
        Name = name;
        TotalVolume = totalVolume;
    }
    
    public string Name { get; }
    public int TotalVolume { get; }
    public Guid? OrderId { get; private set; }

    public static Result<StoragePlace, Error> Create(string name, int totalVolume)
    {
        if (string.IsNullOrEmpty(name)) return GeneralErrors.ValueIsRequired(nameof(name)); 
        if (totalVolume <= 0) return GeneralErrors.ValueIsInvalid(nameof(totalVolume));

        return new StoragePlace(name, totalVolume);
    }

    public Result<bool, Error> CanStore(Order order)
    {
        if(order.Volume <= 0) return GeneralErrors.ValueIsInvalid(nameof(order.Volume));
        
        return order.Volume <= TotalVolume && OrderId is null;
    }

    public UnitResult<Error> Store(Order order)
    {
        if (CanStore(order) is var canStore && (canStore.IsFailure || !canStore.Value))
            return Errors.OrderCannotBeStored(innerError: canStore.IsFailure ? canStore.Error : null);

        OrderId = order.Id;
        
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Clear(Order order)
    {
        if (OrderId is null || OrderId != order.Id)
            return Errors.OrderCannotBeCleared($"OrderId({OrderId}) is not set or not equal to {order.Id}");

        OrderId = null;
        return UnitResult.Success<Error>();
    }

    public static class Errors
    {
        public static Error OrderCannotBeStored(string message = "Order cannot be stored", Error innerError = null)
            => new Error("order.cannot.be.stored", message, innerError);
        
        public static Error OrderCannotBeCleared(string message = "Order cannot be cleared", Error innerError = null)
            => new Error("order.cannot.be.cleared", message, innerError);
    }
}