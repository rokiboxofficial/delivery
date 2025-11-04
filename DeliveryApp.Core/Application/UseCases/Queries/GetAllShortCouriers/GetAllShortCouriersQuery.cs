using DeliveryApp.Core.Ports.ReadModelProviders;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetAllShortCouriers;

public sealed class GetAllShortCouriersQuery : IRequest<GetAllShortCouriersResponse>;