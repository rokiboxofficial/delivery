using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class StoragePlaceTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData("", 1)]
    public void WhenCreating_AndNameIsNullOrEmpty_ThenCreatingResultErrorCodeShouldBeValueIsRequired(string name, int totalVolume)
    {
        // Arrange and Act.
        var creatingResult = StoragePlace.Create(name, totalVolume);
        
        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Theory]
    [InlineData("test", -1)]
    [InlineData("test", 0)]
    public void WhenCreating_AndTotalVolumeIsNegativeOr0_ThenCreatingResultErrorCodeShouldBeValueIsInvalid(string name, int totalVolume)
    {
        // Arrange and Act.
        var creatingResult = StoragePlace.Create(name, totalVolume);
        
        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsInvalid);
    }

    [Theory]
    [InlineData("test", 1)]
    [InlineData("test____2", 100)]
    public void WhenCreating_AndNameAndTotalVolumeAreCorrect_ThenCreatingResultNameAndTotalVolumeShouldBeInitialNameAndInitialTotalVolumeAndIdShouldNotBeEmptyGuid(string initialName, int initialTotalVolume)
    {
        // Arrange and Act.
        var creatingResult = StoragePlace.Create(initialName, initialTotalVolume);

        // Assert.
        new { creatingResult.Value.Name, creatingResult.Value.TotalVolume }.Should()
            .Be(new { Name = initialName, TotalVolume = initialTotalVolume });
        creatingResult.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void WhenCanStoring_AndOrderVolumeIsNegativeOr0_ThenArgumentOutOfRangeExceptionShouldBeThrown(int volume)
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        
        // Act.
        Action act = () => storagePlace.CanStore(volume);

        // Assert.
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WhenCanStoring_AndOrderVolumeIsGreaterThanStoragePlaceTotalVolume_ThenResultShouldBeFalse()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();

        // Act.
        var result = storagePlace.CanStore(storagePlace.TotalVolume + 1);

        // Assert.
        result.Should().BeFalse();
    }

    [Fact]
    public void WhenCanStoring_AndStoragePlaceOrderIdIsNotNull_ThenResultShouldBeFalse()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        storagePlace.Store(Guid.NewGuid(), 1);
        
        // Act.
        var result = storagePlace.CanStore(1);

        // Assert.
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void WhenCanStoring_AndOrderVolumeIsLessThanOrEqualToStoragePlaceTotalVolumeAndStoragePlaceIsNotOccupied_ThenResultShouldBeTrue(int volume)
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        
        // Act.
        var result = storagePlace.CanStore(volume);
        
        // Assert.
        result.Should().BeTrue();
    }
    
    [Fact]
    public void WhenStoring_AndStoragePlaceIsOccupied_ThenStoringResultErrorCodeShouldBeOrderCannotBeStored()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        storagePlace.Store(Guid.NewGuid(), 1);

        // Act.
        var storingResult = storagePlace.Store(Guid.NewGuid(), 1);

        // Assert.
        storingResult.Error.Code.Should().Be(ErrorCodes.OrderCannotBeStored);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void WhenStoring_AndOrderVolumeIsNegativeOr0_ThenExceptionShouldBeThrown(int volume)
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();

        // Act.
        Action act = () => storagePlace.Store(Guid.NewGuid(), volume);

        // Assert.
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void WhenStoring_AndOrderVolumeIsGreaterThanStoragePlaceTotalVolume_ThenStoringResultErrorCodeShouldBeOrderCannotBeStored()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        
        // Act.
        var storingResult = storagePlace.Store(Guid.NewGuid(), storagePlace.TotalVolume + 1);
        
        // Assert.
        storingResult.Error.Code.Should().Be(ErrorCodes.OrderCannotBeStored);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void WhenStoring_AndOrderVolumeIsLessThanOrEqualToStoragePlaceTotalVolumeAndStoragePlaceIsNotOccupied_ThenStoragePlaceOrderIdShouldBeInitialOrderIdAndStoringResultShouldBeSuccessful(int orderVolume)
    {
        // Arrange.
        var initialOrderId = Guid.NewGuid();
        var storagePlace = Create.StoragePlace();

        // Act.
        var storingResult = storagePlace.Store(initialOrderId, orderVolume);

        // Assert.
        storagePlace.OrderId.Should().Be(initialOrderId);
        storingResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void WhenClearing_AndStoragePlaceOrderIdIsNull_ThenClearingResultErrorCodeShouldBeOrderCannotBeCleared()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();

        // Act.
        var clearingResult = storagePlace.Clear(Guid.NewGuid());

        // Assert.
        clearingResult.Error.Code.Should().Be(ErrorCodes.OrderCannotBeCleared);
    }

    [Fact]
    public void WhenClearing_AndStoragePlaceOrderIdNotEqualToOrderId_ThenClearingResultErrorCodeShouldBeOrderCannotBeCleared()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        storagePlace.Store(Guid.NewGuid(), 1);

        // Act.
        var clearingResult = storagePlace.Clear(Guid.NewGuid());

        // Assert.
        clearingResult.Error.Code.Should().Be(ErrorCodes.OrderCannotBeCleared);
    }

    [Fact]
    public void WhenClearing_AndStoragePlaceIsOccupiedAndOrderIdIsSameAsOccupied_ThenStoragePlaceOrderIdShouldBeNullAndClearingResultShouldBeSuccessful()
    {
        // Arrange.
        var orderId = Guid.NewGuid();
        var storagePlace = Create.StoragePlace();
        storagePlace.Store(orderId, 1);

        // Act.
        var clearingResult = storagePlace.Clear(orderId);

        // Assert.
        storagePlace.OrderId.Should().BeNull();
        clearingResult.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void WhenEqualingByOperatorAndByEqualsMethod_AndStoragePlacesAreSame_ThenStoragePlacesEqualityByOperatorAndMethodShouldBeTrue()
    {
        // Arrange.
        var first = Create.StoragePlace();
        var second = first;

        // Act.
        var equalByOperator = first == second;
        var equalByMethod = first.Equals(second);

        // Assert.
        new {equalByOperator, equalByMethod}.Should().Be(new { equalByOperator = true, equalByMethod = true });
    }
    
    [Fact]
    public void WhenEqualingByOperatorAndByEqualsMethod_AndStoragePlacesHaveDifferentIds_ThenStoragePlacesEqualityByOperatorAndMethodShouldBeFalse()
    {
        // Arrange.
        var first = Create.StoragePlace();
        var second = Create.StoragePlace();

        // Act.
        var equalByOperator = first == second;
        var equalByMethod = first.Equals(second);

        // Assert.
        new {equalByOperator, equalByMethod}.Should().Be(new { equalByOperator = false, equalByMethod = false });
    }
}