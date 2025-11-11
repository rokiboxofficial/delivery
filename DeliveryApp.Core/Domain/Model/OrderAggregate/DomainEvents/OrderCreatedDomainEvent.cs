using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;

public sealed record OrderCreatedDomainEvent(Guid OrderId) : DomainEvent;