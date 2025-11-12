using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.Outbox;

internal class OutboxEntityTypeConfiguration : IEntityTypeConfiguration<Entities.Outbox>
{
    public void Configure(EntityTypeBuilder<Entities.Outbox> entityTypeBuilder)
    {
        entityTypeBuilder
            .ToTable("outbox");

        entityTypeBuilder
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        entityTypeBuilder
            .Property(entity => entity.Type)
            .HasColumnName("type")
            .IsRequired();

        entityTypeBuilder
            .Property(entity => entity.Content)
            .HasColumnName("content")
            .IsRequired();

        entityTypeBuilder
            .Property(entity => entity.OccurredOnUtc)
            .HasColumnName("occurred_on_utc")
            .IsRequired();

        entityTypeBuilder
            .Property(entity => entity.ProcessedOnUtc)
            .HasColumnName("processed_on_utc")
            .IsRequired(false);
    }
}