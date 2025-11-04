using DeliveryApp.Core.Domain.Model.SharedKernel;

namespace DeliveryApp.Core.Application.Services;

public interface IRandomLocationProvider
{
    public Location GetRandomLocation();
}