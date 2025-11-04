using DeliveryApp.Core.Ports.ReadModelProviders;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetAllShortCouriers;

public sealed class GetAllShortCouriersHandler(ICourierReadModelProvider courierReadModelProvider)
    : IRequestHandler<GetAllShortCouriersQuery, GetAllShortCouriersResponse>
{
    private readonly ICourierReadModelProvider _courierReadModelProvider = courierReadModelProvider ?? throw new ArgumentNullException(nameof(courierReadModelProvider));

    public Task<GetAllShortCouriersResponse> Handle(GetAllShortCouriersQuery request, CancellationToken cancellationToken)
        => _courierReadModelProvider.GetAllShortCouriersAsync();
}