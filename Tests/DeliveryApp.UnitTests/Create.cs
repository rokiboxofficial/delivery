using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;

namespace DeliveryApp.UnitTests;

public static class Create
{
    public static Location MiddleLocation()
    {
        var middle = Location.Create(
            Location.MinLocation.X + (Location.MaxLocation.X - Location.MinLocation.X) / 2,
            Location.MinLocation.Y + (Location.MaxLocation.Y - Location.MinLocation.Y) / 2).Value;
        
        return middle;
    }
    
    public static StoragePlace StoragePlace(string name = "test", int totalVolume = 10)
    {
        return DeliveryApp.Core.Domain.Model.CourierAggregate.StoragePlace.Create(name, totalVolume).Value;
    }
    
    public static Order Order(Guid? orderId = null, int volume = 1, Location location = null)
    {
        orderId ??= Guid.NewGuid();
        location ??= Location.MinLocation;
        return DeliveryApp.Core.Domain.Model.OrderAggregate.Order.Create(orderId.Value, location, volume).Value;
    }

    public static Courier Courier(string name = "test", int speed = 1, Location location = null)
    {
        location ??= Location.MinLocation;
        return DeliveryApp.Core.Domain.Model.CourierAggregate.Courier.Create(name, speed, location).Value;
    }
}