using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;

public sealed class CreateOrderCommand : IRequest<UnitResult<Error>>
{
    private CreateOrderCommand(Guid orderId, string street, int volume)
    {
        OrderId = orderId;
        Street = street;
        Volume = volume;
    }

    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    /// Улица
    /// </summary>
    public string Street { get; }
    
    /// <summary>
    /// Объем
    /// </summary>
    public int Volume { get; }

    public static Result<CreateOrderCommand, Error> Create(Guid orderId, string street, int volume)
    {
        if(orderId == Guid.Empty) return GeneralErrors.ValueIsInvalid(nameof(orderId));
        if(string.IsNullOrEmpty(street)) return GeneralErrors.ValueIsRequired(nameof(orderId));
        if(volume <= 0) return GeneralErrors.ValueIsInvalid(nameof(volume));
        
        return new CreateOrderCommand(orderId, street, volume);
    }
}