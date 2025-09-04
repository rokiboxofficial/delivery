using DeliveryApp.Core.Domain.Model.CourierAggregate;
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
        return Core.Domain.Model.CourierAggregate.StoragePlace.Create(name, totalVolume).Value;
    }
}