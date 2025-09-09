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

    public Result<bool, Error> CanStore(int orderVolume)
    {
        if(orderVolume <= 0) return GeneralErrors.ValueIsInvalid(nameof(orderVolume));
        
        return orderVolume <= TotalVolume && OrderId is null;
    }

    public UnitResult<Error> Store(Guid orderId, int orderVolume)
    {
        if (CanStore(orderVolume) is var canStore && (canStore.IsFailure || !canStore.Value))
            return Errors.OrderCannotBeStored(innerError: canStore.IsFailure ? canStore.Error : null);

        OrderId = orderId;
        
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Clear(Guid orderId)
    {
        if (OrderId is null || OrderId != orderId)
            return Errors.OrderCannotBeCleared($"OrderId({OrderId}) is not set or not equal to {orderId}");

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