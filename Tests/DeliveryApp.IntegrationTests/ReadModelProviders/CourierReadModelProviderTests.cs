using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports.ReadModelProviders;
using DeliveryApp.Infrastructure.Adapters.Postgres.ReadModelProviders;
using DeliveryApp.TestsCommon;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.IntegrationTests.ReadModelProviders;

public class CourierReadModelProviderTests : ReadModelProviderBaseTests<CourierReadModelProvider>
{
    [Fact]
    public async Task WhenGettingAllShortCouriers_AndThereAreNoCouriers_ThenResponseCouriersShouldBeEmpty()
    {
        // Arrange and Act.
        var response = await ReadModelProvider.GetAllShortCouriersAsync();

        // Assert.
        response.Couriers.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenGettingAllShortCouriers_AndThereAreFewBusyAndFreeCouriers_ThenResponseCouriersShouldBeEquivalentToAllCouriers()
    {
        // Arrange.
        Courier[] allCouriers = [Setup.BusyCourier(), Setup.BusyCourier(), Create.Courier(), Create.Courier()];
        
        await Context.Couriers.AddRangeAsync(allCouriers);
        await Context.SaveChangesAsync();

        // Act.
        var response = await ReadModelProvider.GetAllShortCouriersAsync();

        // Assert.
        response.Couriers.Should().BeEquivalentTo(allCouriers.Select(x => new ShortCourierDto(x)));
    }
}