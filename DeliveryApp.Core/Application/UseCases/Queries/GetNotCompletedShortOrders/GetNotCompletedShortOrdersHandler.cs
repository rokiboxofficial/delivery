using DeliveryApp.Core.Ports.ReadModelProviders;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedShortOrders;

public class GetNotCompletedShortOrdersHandler(IOrderReadModelProvider orderReadModelProvider) : IRequestHandler<GetNotCompletedShortOrdersQuery, GetNotCompletedShortOrdersResponse>
{
    private readonly IOrderReadModelProvider _courierReadModelProvider = orderReadModelProvider ?? throw new ArgumentNullException(nameof(orderReadModelProvider));

    public Task<GetNotCompletedShortOrdersResponse> Handle(GetNotCompletedShortOrdersQuery request, CancellationToken cancellationToken)
        => _courierReadModelProvider.GetNotCompletedShortOrdersAsync();
}