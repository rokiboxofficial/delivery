using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;

public sealed class AssignOrderHandler(
    IOrderRepository orderRepository,
    ICourierRepository courierRepository,
    IUnitOfWork unitOfWork,
    IDispatchService dispatchService,
    ILogger<AssignOrderHandler> logger)
    : IRequestHandler<AssignOrderCommand, UnitResult<Error>>
{
    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    private readonly ICourierRepository _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IDispatchService _dispatchService = dispatchService ?? throw new ArgumentNullException(nameof(dispatchService));
    private readonly ILogger<AssignOrderHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> Handle(AssignOrderCommand request, CancellationToken cancellationToken)
    {
        var maybeCreatedOrder = await _orderRepository.GetFirstInCreatedStatusAsync();
        if(maybeCreatedOrder.HasNoValue) return UnitResult.Success<Error>();
        var createdOrder = maybeCreatedOrder.Value;
        
        var freeCouriers = await _courierRepository.GetAllFreeCouriersAsync();
        if(freeCouriers.Length == 0) return UnitResult.Success<Error>();

        var dispatchResult = _dispatchService.Dispatch(createdOrder, freeCouriers);
        if (dispatchResult.IsFailure)
        {
            _logger.LogError("Dispatch failed. Message: {message}", dispatchResult.Error?.Message);
            return UnitResult.Failure(dispatchResult.Error);
        }
        var dispatchedCourier = dispatchResult.Value;

        _courierRepository.Update(dispatchedCourier);
        _orderRepository.Update(createdOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return UnitResult.Success<Error>();
    }
}