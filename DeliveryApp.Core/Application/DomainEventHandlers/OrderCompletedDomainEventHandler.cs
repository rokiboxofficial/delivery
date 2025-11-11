using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers;

public sealed class OrderCompletedDomainEventHandler(IMessageBusProducer messageBusProducer)
    : INotificationHandler<OrderCompletedDomainEvent>
{
    private readonly IMessageBusProducer _messageBusProducer =
        messageBusProducer ?? throw new ArgumentNullException(nameof(messageBusProducer));

    public Task Handle(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
        => _messageBusProducer.Publish(notification, cancellationToken);
}