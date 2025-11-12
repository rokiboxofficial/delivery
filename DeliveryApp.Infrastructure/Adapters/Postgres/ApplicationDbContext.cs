using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.OrderAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.Outbox;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Courier> Couriers { get; set; }
    public DbSet<Outbox> Outboxes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply Configuration
        modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CourierEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new StoragePlaceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxEntityTypeConfiguration());
    }
}