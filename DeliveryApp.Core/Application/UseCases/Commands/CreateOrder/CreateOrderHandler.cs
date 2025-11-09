using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.Services;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using DeliveryApp.Core.Ports.Repositories;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;

public sealed class CreateOrderHandler(
    IGeoClient geoClient,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrderCommand, UnitResult<Error>>
{
    private readonly IGeoClient _geoClient = geoClient ?? throw new ArgumentNullException(nameof(geoClient));
    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<UnitResult<Error>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var getLocationResult = await _geoClient.GetLocation(request.Street);
        if (getLocationResult.IsFailure) return GeneralErrors.GeoClientError(getLocationResult.Error);
        
        var (_, isFailure, order, error) = Order.Create(request.OrderId, getLocationResult.Value, request.Volume);
        if (isFailure) return error;
        
        var existingOrder = await _orderRepository.GetAsync(order.Id);
        if (existingOrder.HasValue) return Errors.OrderAlreadyExists(existingOrder.Value);

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return UnitResult.Success<Error>();
    }
    
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error OrderAlreadyExists(Order order)
        {
            return new Error(
                "order.already.exists",
                $"Order with id {order.Id} already exists."
            );
        }
    }
}