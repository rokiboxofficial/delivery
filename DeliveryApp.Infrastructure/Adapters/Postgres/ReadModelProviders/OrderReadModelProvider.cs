using Dapper;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports.ReadModelProviders;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.ReadModelProviders;

public sealed class OrderReadModelProvider(IOptions<Settings> settings) : IOrderReadModelProvider
{
    private readonly string _connectionString = !string.IsNullOrEmpty(settings?.Value.ConnectionString)
        ? settings.Value.ConnectionString
        : throw new ArgumentNullException(nameof(settings));

    public async Task<GetNotCompletedShortOrdersResponse> GetNotCompletedShortOrdersAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var shortOrders = await connection.QueryAsync<ShortOrderDto, LocationDto, ShortOrderDto>(
            $"""
            SELECT o.id,
                   o.location_x AS {nameof(LocationDto.X)},
                   o.location_y AS {nameof(LocationDto.Y)}
            FROM orders o
            WHERE status <> '{nameof(OrderStatus.Completed)}'
            """,
            (shortOrderDto, locationDto) =>
            {
                shortOrderDto.Location = locationDto;
                return shortOrderDto;
            },
            splitOn: nameof(LocationDto.X));

        return new GetNotCompletedShortOrdersResponse(shortOrders.ToList());
    }
}