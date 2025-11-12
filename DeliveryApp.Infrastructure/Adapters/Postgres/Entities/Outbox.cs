namespace DeliveryApp.Infrastructure.Adapters.Postgres.Entities;

public sealed class Outbox
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
}