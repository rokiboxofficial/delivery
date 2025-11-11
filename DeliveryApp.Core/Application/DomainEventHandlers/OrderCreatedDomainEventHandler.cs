using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers;

public sealed class OrderCreatedDomainEventHandler(IMessageBusProducer messageBusProducer)
    : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly IMessageBusProducer _messageBusProducer =
        messageBusProducer ?? throw new ArgumentNullException(nameof(messageBusProducer));

    public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        => _messageBusProducer.Publish(notification, cancellationToken);
}