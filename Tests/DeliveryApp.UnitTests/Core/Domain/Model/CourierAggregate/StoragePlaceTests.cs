using System;
using System.Reflection;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Model.CourierAggregate;

public class StoragePlaceTests
{
    [Fact]
    public void WhenCheckingIsEntityAssignableFromStoragePlace_ThenResultShouldBeTrue()
    {
        // Act.
        var isEntityAssignableFromStoragePlace = typeof(Entity<Guid>).IsAssignableFrom(typeof(StoragePlace));

        // Assert.
        isEntityAssignableFromStoragePlace.Should().BeTrue();
    }
    
    [Fact]
    public void WhenGettingConstructors_ThenEachConstructorShouldBePrivate()
    {
        // Act.
        var constructors = typeof(StoragePlace).GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Assert.
        constructors.Should().AllSatisfy(x => x.IsPrivate.Should().BeTrue());
    }
    
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
    
    [Fact]
    public void WhenCanStoring_AndOrderVolumeIsGreaterThanStoragePlaceTotalVolume_ThenResultShouldBeFalse()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        var order = Create.Order(volume: storagePlace.TotalVolume + 1);

        // Act.
        var result = storagePlace.CanStore(order);

        // Assert.
        result.Should().BeFalse();
    }

    [Fact]
    public void WhenCanStoring_AndStoragePlaceOrderIdIsNotNull_ThenResultShouldBeFalse()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        var order = Create.Order(Guid.NewGuid());
        
        storagePlace.Store(order);
        
        // Act.
        var result = storagePlace.CanStore(order);

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
        var order = Create.Order(volume: volume);
        
        // Act.
        var result = storagePlace.CanStore(order);
        
        // Assert.
        result.Should().BeTrue();
    }
    
    [Fact]
    public void WhenStoring_AndStoragePlaceIsOccupied_ThenStoringResultErrorCodeShouldBeOrderCannotBeStored()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        storagePlace.Store(Create.Order());
        var order = Create.Order();

        // Act.
        var storingResult = storagePlace.Store(order);

        // Assert.
        storingResult.Error.Code.Should().Be(ErrorCodes.OrderCannotBeStored);
    }

    [Fact]
    public void WhenStoring_AndOrderVolumeIsGreaterThanStoragePlaceTotalVolume_ThenStoringResultErrorCodeShouldBeOrderCannotBeStored()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        var order = Create.Order(volume: storagePlace.TotalVolume + 1);
        
        // Act.
        var storingResult = storagePlace.Store(order);
        
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
        var order = Create.Order(initialOrderId, orderVolume);

        // Act.
        var storingResult = storagePlace.Store(order);

        // Assert.
        storagePlace.OrderId.Should().Be(initialOrderId);
        storingResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void WhenClearing_AndStoragePlaceOrderIdIsNull_ThenClearingResultErrorCodeShouldBeOrderCannotBeCleared()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        var order = Create.Order();

        // Act.
        var clearingResult = storagePlace.Clear(order);

        // Assert.
        clearingResult.Error.Code.Should().Be(ErrorCodes.OrderCannotBeCleared);
    }

    [Fact]
    public void WhenClearing_AndStoragePlaceOrderIdNotEqualToOrderId_ThenClearingResultErrorCodeShouldBeOrderCannotBeCleared()
    {
        // Arrange.
        var storagePlace = Create.StoragePlace();
        storagePlace.Store(Create.Order());
        var order = Create.Order();

        // Act.
        var clearingResult = storagePlace.Clear(order);

        // Assert.
        clearingResult.Error.Code.Should().Be(ErrorCodes.OrderCannotBeCleared);
    }

    [Fact]
    public void WhenClearing_AndStoragePlaceIsOccupiedAndOrderIdIsSameAsOccupied_ThenStoragePlaceOrderIdShouldBeNullAndClearingResultShouldBeSuccessful()
    {
        // Arrange.
        var orderId = Guid.NewGuid();
        var storagePlace = Create.StoragePlace();
        var order = Create.Order(orderId);
        storagePlace.Store(order);

        // Act.
        var clearingResult = storagePlace.Clear(order);

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