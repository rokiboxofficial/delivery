using Confluent.Kafka;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Primitives;
using Queues.Order;

namespace DeliveryApp.Infrastructure.Adapters.Kafka.Order;

public sealed class Producer(IProducer<string, string> producer, IOptions<Settings> settings, ILogger<Producer> logger)
    : IMessageBusProducer
{
    private readonly IProducer<string, string> _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    private readonly ILogger<Producer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string _topicName = settings?.Value.OrderStatusChangedTopic ?? throw new ArgumentNullException(nameof(settings));

    public async Task Publish(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new OrderCreatedIntegrationEvent
        {
            EventId = notification.EventId.ToString(),
            OccurredAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(notification.OccurredAt),
            
            OrderId = notification.OrderId.ToString()
        };
        
        await ProduceAsync(notification, integrationEvent, cancellationToken);
    }

    public async Task Publish(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new OrderCompletedIntegrationEvent
        {
            EventId = notification.EventId.ToString(),
            OccurredAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(notification.OccurredAt),
            
            OrderId = notification.OrderId.ToString(),
            CourierId = notification.CourierId.ToString()
        };
        
        await ProduceAsync(notification, integrationEvent, cancellationToken);
    }
    
    private async Task ProduceAsync(DomainEvent domainEvent, object integrationEvent, CancellationToken cancellationToken)
    {
        var message = new Message<string, string>
        {
            Key = domainEvent.EventId.ToString(),
            Value = JsonConvert.SerializeObject(integrationEvent)
        };
        
        var delivery = await _producer.ProduceAsync(_topicName, message, cancellationToken);
        _logger.LogTrace("Delivered '{value}' to '{topicPartitionOffset}'", delivery.Value,
            delivery.TopicPartitionOffset);
    }
}