using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;
using Primitives.Exceptions;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

public sealed class Order : Aggregate<Guid>
{
    [ExcludeFromCodeCoverage]
    private Order()
    {
        
    }
    
    private Order(Guid orderId, Location location, int volume, OrderStatus status)
    {
        Id = orderId;
        Location = location;
        Volume = volume;
        Status = status;
        
        RaiseDomainEvent(new OrderCreatedDomainEvent(orderId));
    }
    
    public Location Location { get; }
    
    public int Volume { get; }
    
    public OrderStatus Status { get; private set; }
    
    public Guid? CourierId { get; private set; }
    
    public static Result<Order, Error> Create(Guid orderId, Location location, int volume)
    {
        if (orderId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(orderId));
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));
        if (volume <= 0) return GeneralErrors.ValueIsInvalid(nameof(volume));
        
        return new Order(orderId, location, volume, OrderStatus.Created);
    }
    
    public UnitResult<Error> Assign(Courier courier)
    {
        if (courier == null) return GeneralErrors.ValueIsRequired(nameof(courier));
        if (Status != OrderStatus.Created) return Errors.OnlyOrderWithStatusCreatedCanBeAssigned();

        CourierId = courier.Id;
        Status = OrderStatus.Assigned;

        return UnitResult.Success<Error>();
    }
    
    public UnitResult<Error> Complete()
    {
        if (Status != OrderStatus.Assigned)
            return Errors.NotAssignedOrderCannotBeCompleted();

        Status = OrderStatus.Completed;

        if (!CourierId.HasValue) throw new DomainInvariantException("Order should has CourierId when completing");
        RaiseDomainEvent(new OrderCompletedDomainEvent(Id, CourierId!.Value));
        
        return UnitResult.Success<Error>();
    }
    
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error NotAssignedOrderCannotBeCompleted()
        {
            return new Error("not.assigned.order.cannot.be.completed",
                "Not assigned order cannot be completed");
        }

        public static Error OnlyOrderWithStatusCreatedCanBeAssigned()
        {
            return new Error("only.order.with.status.created.can.be.assigned",
                "Only order with status \"Created\" can be assigned");
        }
    }
}