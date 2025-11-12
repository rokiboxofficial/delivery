using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using Newtonsoft.Json;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await SaveDomainEventsInOutboxAsync();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task SaveDomainEventsInOutboxAsync()
    {
        var outboxes = _dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(x => x.Entity)
            .SelectMany(aggregate =>
                {
                    var domainEvents = aggregate.GetDomainEvents();
                    aggregate.ClearDomainEvents();

                    return domainEvents;
                }
            )
            .Select(domainEvent => new Outbox
            {
                Id = domainEvent.EventId,
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(
                    domainEvent,
                    new JsonSerializerSettings
                    {
                        // Эта настройка нужна, чтобы сериализовать Domain Event с указанием типов
                        // Если ее не указать, то десеарилизатор не поймет в какой тип восстанавоивать сообщение
                        TypeNameHandling = TypeNameHandling.All
                    })
            })
            .ToArray();

        if(outboxes.Length > 0)
            await _dbContext.Set<Outbox>().AddRangeAsync(outboxes);
    }
}