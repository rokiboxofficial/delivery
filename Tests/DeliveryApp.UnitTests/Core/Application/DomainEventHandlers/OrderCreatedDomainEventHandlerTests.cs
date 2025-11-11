using System;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.DomainEventHandlers;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using NSubstitute;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Application.DomainEventHandlers;

public class OrderCreatedDomainEventHandlerTests
{
    private readonly IMessageBusProducer _messageBusProducerMock = Substitute.For<IMessageBusProducer>();
    private readonly OrderCreatedDomainEventHandler _orderCreatedDomainEventHandler;
    
    public OrderCreatedDomainEventHandlerTests()
    {
        _orderCreatedDomainEventHandler = new OrderCreatedDomainEventHandler(_messageBusProducerMock);
    }
    
    [Fact]
    public async Task WhenHandling_ThenDomainEventShouldBePublished()
    {
        // Arrange.
        var domainEvent = new OrderCreatedDomainEvent(Guid.NewGuid());
        var cancellationToken = CancellationToken.None;
        
        // Act.
        await _orderCreatedDomainEventHandler.Handle(domainEvent, cancellationToken);

        // Assert.
        await _messageBusProducerMock.Received(1).Publish(domainEvent, cancellationToken);
    }
}