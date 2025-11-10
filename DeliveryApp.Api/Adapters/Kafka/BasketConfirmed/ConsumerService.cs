using Confluent.Kafka;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Infrastructure;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Queues.Basket;

namespace DeliveryApp.Api.Adapters.Kafka.BasketConfirmed;

public sealed class ConsumerService(
    IServiceScopeFactory serviceScopeFactory,
    IConsumer<Ignore, string> consumer,
    IOptions<Settings> settings,
    ILogger<ConsumerService> logger)
    : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    private readonly IConsumer<Ignore, string> _consumer = consumer  ?? throw new ArgumentNullException(nameof(consumer));
    private readonly string _topic = settings?.Value.BasketConfirmedTopic ?? throw new ArgumentNullException(nameof(settings));
    private readonly ILogger<ConsumerService> _logger = logger  ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                
                var consumeResult = _consumer.Consume(cancellationToken);
                if (consumeResult.IsPartitionEOF) continue;
                if(consumeResult.Message?.Value is null)
                    throw new InvalidOperationException("Consuming result message is null");
                
                var basketConfirmedIntegrationEvent =
                    JsonConvert.DeserializeObject<BasketConfirmedIntegrationEvent>(consumeResult.Message.Value)
                    ?? throw new InvalidOperationException("Deserialized event is null");
                
                var createOrderCommandResult = CreateOrderCommand.Create(
                    Guid.Parse(basketConfirmedIntegrationEvent.BasketId),
                    basketConfirmedIntegrationEvent.Address?.Street,
                    basketConfirmedIntegrationEvent.Volume);

                if (createOrderCommandResult.IsFailure)
                {
                    _logger.LogError("Failed to create order. Message: {message}",
                        createOrderCommandResult.Error?.Message);
                    continue;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var sendResult = await mediator.Send(createOrderCommandResult.Value, cancellationToken);
                if (sendResult.IsFailure)
                {
                    _logger.LogError("Failed to send command to mediator. Message: {message}",
                        sendResult.Error?.Message);
                    continue;
                }

                try
                {
                    _consumer.StoreOffset(consumeResult);
                }
                catch (KafkaException e)
                {
                    _logger.LogError(e, "Store Offset error: {reason}", e.Error?.Reason);
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.LogError(e, "Consumer cancelled");
        }
    }
}