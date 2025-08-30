using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;

namespace DeliveryApp.Core.Application.Services;

public sealed class RandomLocationProvider(IRandomNumberProvider randomNumberProvider)
{
    public Location GetRandomLocation()
    {
        var x = randomNumberProvider.GetRandomNumber(Location.MinLocation.X, Location.MaxLocation.X );
        var y = randomNumberProvider.GetRandomNumber(Location.MinLocation.Y, Location.MaxLocation.Y);

        return Location.Create(x, y).Value;
    }
}