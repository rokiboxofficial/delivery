using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
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

    public bool CanStore(int orderVolume)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(orderVolume);
        
        return orderVolume <= TotalVolume && OrderId is null;
    }

    public UnitResult<Error> Store(Guid orderId, int orderVolume)
    {
        if (!CanStore(orderVolume)) return Errors.OrderCannotBeStored();
        OrderId = orderId;
        
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Clear(Guid orderId)
    {
        if (OrderId is null || OrderId != orderId) return Errors.OrderCannotBeCleared();

        OrderId = null;
        return UnitResult.Success<Error>();
    }

    public static class Errors
    {
        public static Error OrderCannotBeStored()
            => new Error("order.cannot.be.stored", "Order cannot be stored");
        
        public static Error OrderCannotBeCleared()
            => new Error("order.cannot.be.cleared", "Order cannot be cleared");
    }
}