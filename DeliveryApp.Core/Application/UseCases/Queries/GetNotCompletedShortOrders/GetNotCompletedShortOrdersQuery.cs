using DeliveryApp.Core.Ports.ReadModelProviders;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedShortOrders;

public sealed class GetNotCompletedShortOrdersQuery : IRequest<GetNotCompletedShortOrdersResponse>;