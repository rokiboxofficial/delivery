using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services;

public sealed class DispatchService : IDispatchService
{
    public Result<Courier, Error> Dispatch(Order order, IList<Courier> couriers)
    {
        if (order is null) return GeneralErrors.ValueIsRequired(nameof(order));
        if (order.Status != OrderStatus.Created) return Errors.OnlyOrderWithStatusCreatedCanBeDispatched(order);
        
        if (couriers is null) return GeneralErrors.ValueIsRequired(nameof(couriers));
        if (couriers.Count is 0) return GeneralErrors.CollectionIsTooSmall(1, 0);
        
        var mostSuitableCourier = couriers
            .Where(courier => courier.CanTakeOrder(order).Value)
            .MinBy(courier => courier.GetRemainingMovesCount(order.Location).Value);

        if (mostSuitableCourier is null) return Errors.SuitableCourierNotFound(order);

        order.Assign(mostSuitableCourier);
        mostSuitableCourier.TakeOrder(order);
        
        return mostSuitableCourier;
    }

    public static class Errors
    {
        public static Error OnlyOrderWithStatusCreatedCanBeDispatched(Order order)
        {
            return new Error(
                "only.order.with.status.created.can.be.dispatched",
                $"Only order with status \"Created\" can be dispatched. Actual order is {{id: {order?.Id}, volume: {order?.CourierId}, courierId: {order?.CourierId}, location: {order.Location}}}"
                );
        }

        public static Error SuitableCourierNotFound(Order order)
        {
            return new Error(
                "suitable.courier.not.found",
                $"Suitable courier for order {{id: {order?.Id}, volume: {order?.CourierId}, courierId: {order?.CourierId}, location: {order?.Location}}} not found"
                );
        }
    }
}