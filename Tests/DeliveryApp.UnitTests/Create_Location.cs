using DeliveryApp.Core.Domain.Model.SharedKernel;
using static DeliveryApp.Core.Domain.Model.SharedKernel.Location;

namespace DeliveryApp.UnitTests;

public static partial class Create
{
    public static Location MiddleLocation()
    {
        var middle = Create(
            MinLocation.X + (MaxLocation.X - MinLocation.X) / 2,
            MinLocation.Y + (MaxLocation.Y - MinLocation.Y) / 2)
            .Value;

        return middle;
    }

    public static Location Location(int x = 1, int y = 1)
        => Create(x, y).Value;
}