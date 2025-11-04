using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports.Repositories;
using MediatR;
using Primitives;
using Primitives.Extensions;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;

public sealed class MoveCouriersHandler(
    ICourierRepository courierRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<MoveCouriersCommand, UnitResult<Error>>
{
    private readonly ICourierRepository _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    
    public async Task<UnitResult<Error>> Handle(MoveCouriersCommand request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllInAssignedStatusAsync();
        
        foreach (var order in orders)
        {
            if (!order.CourierId.HasValue) throw new InvalidOperationException("Order in assigned status should has not null CourierId");
            
            var maybeCourier = await _courierRepository.GetAsync(order.CourierId.Value);
            if(maybeCourier.HasNoValue) throw new InvalidOperationException("Courier should exist");
            
            var courier = maybeCourier.Value;
            
            courier.Move(order.Location).ThrowIfFailure();
            if (courier.Location == order.Location)
            {
                courier.CompleteOrder(order).ThrowIfFailure();
                order.Complete();
            }

            _orderRepository.Update(order);
            _courierRepository.Update(courier);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}