using System;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Application.UseCases.Commands.CreateOrder;

public class CreateOrderCommandTests
{
    [Fact]
    public void WhenCreating_AndOrderIdIsEmpty_ThenErrorShouldBeValueIsInvalid()
    {
        // Arrange.
        var orderId = Guid.Empty;

        // Act.
        var creatingResult = CreateOrderCommand.Create(orderId, "street", 1);

        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsInvalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenCreating_AndStreetIsNullOrEmpty_ThenErrorShouldBeValueIsRequired(string street)
    {
        // Arrange and Act.
        var creatingResult = CreateOrderCommand.Create(Guid.NewGuid(), street, 1);
        
        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void WhenCreating_AndVolumeIsLessThanOrEqualTo0_ThenErrorShouldBeValueIsInvalid(int volume)
    {
        // Arrange and Act.
        var creatingResult = CreateOrderCommand.Create(Guid.NewGuid(), "street", volume);

        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsInvalid);
    }

    [Fact]
    public void WhenCreating_AndOrderIdAndStreetAndVolumeAreCorrect_ThenResultShouldHasCorrectProperties()
    {
        // Arrange.
        var orderId = Guid.NewGuid();
        const string street = "street";
        const int volume = 1;

        // Act.
        var createOrderCommand = CreateOrderCommand.Create(orderId, street, volume).Value;

        // Assert.
        new { createOrderCommand.OrderId, createOrderCommand.Street, createOrderCommand.Volume }
            .Should().Be(new { OrderId = orderId, Street = street, Volume = volume });
    }
}