using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Ports;

public interface IGeoClient
{
    public Task<Result<Location, Error>> GetLocation(string street);
}