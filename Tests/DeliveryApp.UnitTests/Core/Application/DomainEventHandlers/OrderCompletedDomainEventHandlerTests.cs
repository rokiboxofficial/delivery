using System;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.DomainEventHandlers;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using NSubstitute;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Application.DomainEventHandlers;

public class OrderCompletedDomainEventHandlerTests
{
    private readonly IMessageBusProducer _messageBusProducerMock = Substitute.For<IMessageBusProducer>();
    private readonly OrderCompletedDomainEventHandler _orderCompletedDomainEventHandler;

    public OrderCompletedDomainEventHandlerTests()
    {
        _orderCompletedDomainEventHandler = new OrderCompletedDomainEventHandler(_messageBusProducerMock);
    }

    [Fact]
    public async Task WhenHandling_ThenDomainEventShouldBePublished()
    {
        // Arrange.
        var domainEvent = new OrderCompletedDomainEvent(Guid.NewGuid(), Guid.NewGuid());
        var cancellationToken = CancellationToken.None;
        
        // Act.
        await _orderCompletedDomainEventHandler.Handle(domainEvent, cancellationToken);

        // Assert.
        await _messageBusProducerMock.Received(1).Publish(domainEvent, cancellationToken);
    }
}