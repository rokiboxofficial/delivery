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
}