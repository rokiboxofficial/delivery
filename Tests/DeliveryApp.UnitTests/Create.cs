using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Domain.Services;

namespace DeliveryApp.UnitTests;

public static partial class Create
{
    public static StoragePlace StoragePlace(string name = "test", int totalVolume = 10)
    {
        return DeliveryApp.Core.Domain.Model.CourierAggregate.StoragePlace.Create(name, totalVolume).Value;
    }
    
    public static Order Order(Guid? orderId = null, int volume = 1, Location location = null)
    {
        orderId ??= Guid.NewGuid();
        location ??= DeliveryApp.Core.Domain.Model.SharedKernel.Location.MinLocation;;
        return DeliveryApp.Core.Domain.Model.OrderAggregate.Order.Create(orderId.Value, location, volume).Value;
    }

    public static Courier Courier(string name = "test", int speed = 1, Location location = null)
    {
        location ??= DeliveryApp.Core.Domain.Model.SharedKernel.Location.MinLocation;
        return DeliveryApp.Core.Domain.Model.CourierAggregate.Courier.Create(name, speed, location).Value;
    }

    public static DispatchService DispatchService() => new();
}