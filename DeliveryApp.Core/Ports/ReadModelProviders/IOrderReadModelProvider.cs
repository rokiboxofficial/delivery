using DeliveryApp.Core.Domain.Model.OrderAggregate;

namespace DeliveryApp.Core.Ports.ReadModelProviders;

public interface IOrderReadModelProvider
{
    public Task<GetNotCompletedShortOrdersResponse> GetNotCompletedShortOrdersAsync();
}

public record GetNotCompletedShortOrdersResponse(List<ShortOrderDto> Orders);

public sealed class ShortOrderDto
{
    public ShortOrderDto()
    {
        
    }

    public ShortOrderDto(Order order)
    {
        Id = order.Id;
        Location = new LocationDto(order.Location);
    }
    
    public Guid Id { get; set; }
    public LocationDto Location { get; set; }
}