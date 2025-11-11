using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;

namespace DeliveryApp.Core.Ports;

public interface IMessageBusProducer
{
    public Task Publish(OrderCreatedDomainEvent notification, CancellationToken cancellationToken);
    public Task Publish(OrderCompletedDomainEvent notification, CancellationToken cancellationToken);
}