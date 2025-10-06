using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;

namespace DeliveryApp.TestsCommon;

public static class Setup
{
    public static Order AssignedOrder()
    {
        var assigned = Create.Order();
        assigned.Assign(Create.Courier());
        return assigned;
    }

    public static Order CompletedOrder()
    {
        var completed = Create.Order();
        completed.Assign(Create.Courier());
        completed.Complete();
        return completed;
    }

    public static Courier BusyCourier(Location location = null)
    {
        location ??= Location.MinLocation;
        
        var busyCourier1 = Create.Courier(location: location);
        busyCourier1.TakeOrder(Create.Order());
        return busyCourier1;
    }
}