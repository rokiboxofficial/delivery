using System;
using System.Linq;
using DeliveryApp.Core.Application.Services;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DeliveryApp.UnitTests.Application.Services;

public class RandomLocationProviderTests
{
    [Fact]
    public void WhenGettingRandomLocation_AndRandomNumberProviderHasImplementation_ThenGetRandomNumberShouldBeCalledCorrectlyAndRandomLocationShouldBeCorrect()
    {
        // Arrange.
        var randomNumberProvider = Substitute.For<IRandomNumberProvider>();
        randomNumberProvider.GetRandomNumber(Arg.Any<int>(), Arg.Any<int>()).Returns(Location.MinLocation.X, Location.MaxLocation.Y);
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
        randomLocation.Should().Be(Location.Create(Location.MinLocation.X, Location.MaxLocation.X).Value);
    }
}