using DeliveryApp.Core.Domain.Model.CourierAggregate;

namespace DeliveryApp.Core.Ports.ReadModelProviders;

public interface ICourierReadModelProvider
{
    public Task<GetAllShortCouriersResponse> GetAllShortCouriersAsync(); 
}

public record GetAllShortCouriersResponse(List<ShortCourierDto> Couriers);

public sealed class ShortCourierDto
{
    public ShortCourierDto()
    {
        
    }

    public ShortCourierDto(Courier courier)
    {
        Id = courier.Id;
        Name = courier.Name;
        Location = new LocationDto(courier.Location);
    }
    
    public Guid Id { get; set; }
    public string Name { get; set; }
    public LocationDto Location { get; set; }
}