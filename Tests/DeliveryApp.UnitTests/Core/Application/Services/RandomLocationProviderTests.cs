using System.Linq;
using DeliveryApp.Core.Application.Services;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Application.Services;

public class RandomLocationProviderTests
{
    [Fact]
    public void WhenGettingRandomLocation_AndRandomNumberProviderHasImplementation_ThenGetRandomNumberShouldBeCalledCorrectlyAndRandomLocationShouldBeCorrect()
    {
        // Arrange.
        var expectedX = Location.MinLocation.X;
        var expectedY = Location.MinLocation.Y;
        
        var randomNumberProvider = Substitute.For<IRandomNumberProvider>();
        randomNumberProvider.GetRandomNumber(Arg.Any<int>(), Arg.Any<int>()).Returns(expectedX, expectedY);
        var randomLocationProvider = new RandomLocationProvider(randomNumberProvider);
        
        // Act.
        var randomLocation = randomLocationProvider.GetRandomLocation();

        // Assert.
        randomNumberProvider.Received(2).GetRandomNumber(Arg.Any<int>(), Arg.Any<int>());
        var calls = randomNumberProvider.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(randomNumberProvider.GetRandomNumber))
            .Select(call => call.GetArguments())
            .ToArray();
        calls.Should().BeEquivalentTo(new int[][] { [Location.MinLocation.X, Location.MaxLocation.X], [Location.MinLocation.Y, Location.MaxLocation.Y] });
        randomLocation.Should().Be(Location.Create(expectedX, expectedY).Value);
    }
}