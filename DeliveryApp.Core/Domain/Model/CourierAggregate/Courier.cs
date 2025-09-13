using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public sealed class Courier : Aggregate<Guid>
{
    private const int DefaultBagTotalVolume = 10;
    
    [ExcludeFromCodeCoverage]
    private Courier()
    {
        
    }

    private Courier(string name, int speed, Location location, List<StoragePlace> storagePlaces)
    {
        Id = Guid.NewGuid();
        Name = name;
        Speed = speed;
        Location = location;
        StoragePlaces = storagePlaces;
    }

    public string Name { get; }
    public int Speed { get; }
    public Location Location { get; private set; }
    public List<StoragePlace> StoragePlaces { get; }

    public static Result<Courier, Error> Create(string name, int speed, Location location)
    {
        if(string.IsNullOrEmpty(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if(speed <= 0) return GeneralErrors.ValueIsInvalid(nameof(speed));
        if(location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        var bag = StoragePlace.Create("Bag", DefaultBagTotalVolume).Value;

        return new Courier(name, speed, location, [bag]);
    }

    public UnitResult<Error> AddStoragePlace(string name, int volume)
    {
        if (StoragePlace.Create(name, volume) is var creatingResult && creatingResult.IsFailure)
            return Errors.StoragePlaceCannotBeAddedToCourier(creatingResult.Error);

        StoragePlaces.Add(creatingResult.Value);
        return UnitResult.Success<Error>();
    }

    public Result<bool, Error> CanTakeOrder(Order order)
    {
        return order is null 
            ? GeneralErrors.ValueIsRequired(nameof(order))
            : StoragePlaces.Any(storagePlace => storagePlace.CanStore(order.Volume).Value);
    }

    public UnitResult<Error> TakeOrder(Order order)
    {
        if (order is null) return GeneralErrors.ValueIsRequired(nameof(order));
        if (!CanTakeOrder(order).Value) return Errors.OrderCannotBeTakenByCourier();
        
        StoragePlaces
            .Where(storagePlace => storagePlace.CanStore(order.Volume).Value)
            .MinBy(storagePlace => storagePlace.TotalVolume)
            .Store(order.Id, order.Volume);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> CompleteOrder(Order order)
    {
        if (order is null) return GeneralErrors.ValueIsRequired(nameof(order));

        var orderStoragePlace = StoragePlaces.SingleOrDefault(storagePlace => storagePlace.OrderId == order.Id);
        if (orderStoragePlace == null) return Errors.CourierHasNoSpecifiedOrder(order);
        
        orderStoragePlace.Clear(order.Id);

        return UnitResult.Success<Error>();
    }

    public Result<int, Error> GetRemainingMovesCount(Location target)
    {
        if (target is null) return GeneralErrors.ValueIsRequired(nameof(target));
        
        var distance = target.DistanceTo(Location).Value;
        var remainingMovesCount = (int) Math.Ceiling(distance / (double) Speed);

        return remainingMovesCount;
    }
    
    public UnitResult<Error> Move(Location target)
    {
        if (target == null) return GeneralErrors.ValueIsRequired(nameof(target));

        var xDifference = target.X - Location.X;
        var yDifference = target.Y - Location.Y;
        var leftMoves = Speed;

        var moveX = Math.Clamp(xDifference, -leftMoves, leftMoves);
        leftMoves -= Math.Abs(moveX);
        var moveY = Math.Clamp(yDifference, -leftMoves, leftMoves);

        Location = Location.Create(Location.X + moveX, Location.Y + moveY).Value;
        
        return UnitResult.Success<Error>();
    }
    
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error CourierHasNoSpecifiedOrder(Order order)
        {
            return new Error(
                "courier.has.no.specified.order",
                $"Courier has no order with id: {order.Id}"
            );
        }
        
        public static Error StoragePlaceCannotBeAddedToCourier(Error innerError = null)
        {
            return new Error(
                "storage.place.cannot.be.added.to.courier",
                "Storage place cannot be added to courier",
                innerError);
        }
        
        public static Error OrderCannotBeTakenByCourier(Error innerError = null)
        {
            return new Error(
                "order.cannot.be.taken.by.courier",
                "Order cannot be taken by courier",
                innerError);
        }
    }
}