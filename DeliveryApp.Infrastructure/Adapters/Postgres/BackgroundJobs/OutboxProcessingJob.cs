using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using JsonNet.ContractResolvers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Primitives;
using Quartz;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.BackgroundJobs;

[DisallowConcurrentExecution]
public class OutboxProcessingJob(ApplicationDbContext dbContext, IMediator mediator) : IJob
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public async Task Execute(IJobExecutionContext context)
    {
        var outboxes = await _dbContext
            .Set<Outbox>()
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(20)
            .ToArrayAsync(context.CancellationToken);

        foreach (var outbox in outboxes)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new PrivateSetterContractResolver(),
                TypeNameHandling = TypeNameHandling.All
            };

            var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(outbox.Content, settings);

            await _mediator.Publish(domainEvent, context.CancellationToken);
            outbox.ProcessedOnUtc = DateTime.UtcNow;
        }
        
        if(outboxes.Length > 0)
            await _dbContext.SaveChangesAsync();
    }
}