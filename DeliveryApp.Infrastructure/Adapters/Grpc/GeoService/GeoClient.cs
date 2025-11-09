using Clients.Geo;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports;
using Primitives;
using Location = DeliveryApp.Core.Domain.Model.SharedKernel.Location;

namespace DeliveryApp.Infrastructure.Adapters.Grpc.GeoService;

public sealed class GeoClient(Geo.GeoClient geoClient) : IGeoClient
{
    private readonly Geo.GeoClient _geoClient = geoClient ?? throw new ArgumentNullException(nameof(geoClient));

    public async Task<Result<Location, Error>> GetLocation(string street)
    {
        var reply = await _geoClient.GetGeolocationAsync(new GetGeolocationRequest()
        {
            Street = street
        });

        return Location.Create(reply.Location.X, reply.Location.Y);
    }
}