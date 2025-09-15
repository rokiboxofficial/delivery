using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Domain.Services;
using static DeliveryApp.Core.Domain.Model.SharedKernel.Location;

namespace DeliveryApp.UnitTests;

public static class Create
{
    public static Location MiddleLocation()
    {
        var middle = Create(
            MinLocation.X + (MaxLocation.X - MinLocation.X) / 2,
            MinLocation.Y + (MaxLocation.Y - MinLocation.Y) / 2).Value;
        
        return middle;
    }
    
    public static StoragePlace StoragePlace(string name = "test", int totalVolume = 10)
    {
        return DeliveryApp.Core.Domain.Model.CourierAggregate.StoragePlace.Create(name, totalVolume).Value;
    }
    
    public static Order Order(Guid? orderId = null, int volume = 1, Location location = null)
    {
        orderId ??= Guid.NewGuid();
        location ??= MinLocation;
        return DeliveryApp.Core.Domain.Model.OrderAggregate.Order.Create(orderId.Value, location, volume).Value;
    }

    public static Courier Courier(string name = "test", int speed = 1, Location location = null)
    {
        location ??= MinLocation;
        return DeliveryApp.Core.Domain.Model.CourierAggregate.Courier.Create(name, speed, location).Value;
    }
    
    public static Location Location(int x = 1, int y = 1)
        => Create(x, y).Value;

    public static DispatchService DispatchService() => new();
}