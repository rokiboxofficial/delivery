using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports.ReadModelProviders;
using DeliveryApp.Infrastructure.Adapters.Postgres.ReadModelProviders;
using DeliveryApp.TestsCommon;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.IntegrationTests.ReadModelProviders;

public class OrderReadModelProviderTests : ReadModelProviderBaseTests<OrderReadModelProvider>
{
    [Fact]
    public async Task WhenGettingNotCompletedShortOrders_AndThereAreNoOrders_ThenResponseOrdersShouldBeEmpty()
    {
        // Arrange and Act.
        var response = await ReadModelProvider.GetNotCompletedShortOrdersAsync();

        // Assert.
        response.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenGettingNotCompletedShortOrders_AndThereAreNoNotCompletedShortOrders_ThenResponseOrdersShouldBeEmpty()
    {
        // Arrange.
        await Context.Orders.AddRangeAsync(Setup.CompletedOrder(), Setup.CompletedOrder());
        await Context.SaveChangesAsync();
        
        // Act.
        var response = await ReadModelProvider.GetNotCompletedShortOrdersAsync();

        // Assert.
        response.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenGettingNotCompletedShortOrders_AndThereAreFewNotCompletedAndCompletedOrders_ThenResponseOrdersShouldBeNotCompletedOrders()
    {
        // Arrange.
        Order[] notCompletedOrders = [Create.Order(), Create.Order()];
        Order[] completedOrders = [Setup.CompletedOrder(), Setup.CompletedOrder()];
        
        await Context.Orders.AddRangeAsync(notCompletedOrders);
        await Context.Orders.AddRangeAsync(completedOrders);
        
        await Context.SaveChangesAsync();

        // Act.
        var response = await ReadModelProvider.GetNotCompletedShortOrdersAsync();

        // Assert.
        response.Orders.Should().BeEquivalentTo(notCompletedOrders.Select(x => new ShortOrderDto(x)));
    }
}