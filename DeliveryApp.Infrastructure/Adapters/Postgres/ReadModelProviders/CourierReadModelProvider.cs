using Dapper;
using DeliveryApp.Core.Ports.ReadModelProviders;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.ReadModelProviders;

public sealed class CourierReadModelProvider(IOptions<Settings> settings) : ICourierReadModelProvider
{
    private readonly string _connectionString = !string.IsNullOrEmpty(settings?.Value.ConnectionString)
        ? settings.Value.ConnectionString
        : throw new ArgumentNullException(nameof(settings));
    
    public async Task<GetAllShortCouriersResponse> GetAllShortCouriersAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var shortCouriers = await connection.QueryAsync<ShortCourierDto, LocationDto, ShortCourierDto>(
            $"""
             SELECT c.id,
                    c.name,
                    c.location_x AS {nameof(LocationDto.X)},
                    c.location_y AS {nameof(LocationDto.Y)}
             FROM couriers c
             """,
            (shortCourierDto, locationDto) =>
            {
                shortCourierDto.Location = locationDto;
                return shortCourierDto;
            },
            splitOn: nameof(LocationDto.X));
        
        return new GetAllShortCouriersResponse(shortCouriers.ToList());
    }
}