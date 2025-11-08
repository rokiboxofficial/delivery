using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;
using MediatR;
using Quartz;

namespace DeliveryApp.Api.Adapters.BackgroundJobs;

[DisallowConcurrentExecution]
public class AssignOrderJob(IMediator mediator) : IJob
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public async Task Execute(IJobExecutionContext context)
    {
        var assignOrderCommand = new AssignOrderCommand();
        await _mediator.Send(assignOrderCommand);
    }
}