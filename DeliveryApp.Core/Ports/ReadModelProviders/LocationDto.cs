using DeliveryApp.Core.Domain.Model.SharedKernel;

namespace DeliveryApp.Core.Ports.ReadModelProviders;

public sealed class LocationDto
{
    public LocationDto()
    {
        
    }

    public LocationDto(Location location)
    {
        X = location.X;
        Y = location.Y;
    }
    
    public int X { get; set; }
    public int Y { get; set; }
}