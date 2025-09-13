using System;
using System.Linq;
using System.Reflection;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Model.CourierAggregate;

public class CourierTests
{
    [Fact]
    public void WhenCheckingIsAggregateAssignableFromCourier_ThenResultShouldBeTrue()
    {
        // Act.
        var isAggregateAssignableFromCourier = typeof(Aggregate<Guid>).IsAssignableFrom(typeof(Courier));

        // Assert.
        isAggregateAssignableFromCourier.Should().BeTrue();
    }
    
    [Fact]
    public void WhenGettingConstructors_ThenEachConstructorShouldBePrivate()
    {
        // Act.
        var constructors = typeof(Courier).GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Assert.
        constructors.Should().AllSatisfy(x => x.IsPrivate.Should().BeTrue());
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenCreating_AndNameIsNullOrEmpty_ThenErrorShouldBeValueIsRequired(string name)
    {
        // Arrange and Act.
        var creatingResult = Courier.Create(name, 1, Location.MinLocation);

        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void WhenCreating_AndSpeedIsNegativeOr0_ThenErrorShouldBeValueIsInvalid(int speed)
    {
        // Arrange and Act.
        var creatingResult = Courier.Create("test", speed, Location.MinLocation);
        
        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsInvalid);
    }

    [Fact]
    public void WhenCreating_AndLocationIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange.
        const Location location = null;
        
        // Act.
        var creatingResult = Courier.Create("test", 1, location);
        
        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Fact]
    public void WhenCreating_AndNameAndSpeedAndLocationAreValid_ThenCourierShouldHasCorrectNameAndSpeedAndLocationAndNonEmptyId()
    {
        // Arrange.
        const string name = "test";
        const int speed = 1;
        var location = Location.MinLocation;

        // Act.
        var courier = Courier.Create(name, speed, location).Value;

        // Assert.
        new { courier.Name, courier.Speed, courier.Location, IdIsNotEmptyGuid = courier.Id != Guid.Empty }.Should().Be(
            new { Name = name, Speed = speed, Location = location, IdIsNotEmptyGuid = true });
    }

    [Fact]
    public void WhenCreating_AndArgumentsAreValid_ThenCourierShouldHasOnlyBagWith10TotalVolume()
    {
        // Arrange and Act.
        var courier =  Courier.Create("test", 1, Location.MinLocation).Value;

        // Assert.
        courier.StoragePlaces.Count.Should().Be(1);
        courier.StoragePlaces.Should().ContainSingle(storagePlace =>
            storagePlace.Name == "Bag" && storagePlace.TotalVolume == 10);
    }

    [Theory]
    [InlineData("test", -1)]
    [InlineData("test", 0)]
    [InlineData(null, 1)]
    [InlineData(null, -1)]
    [InlineData(null, 0)]
    [InlineData("", 1)]
    [InlineData("", -1)]
    [InlineData("", 0)]
    public void WhenAddingStoragePlace_AndNameOrVolumeAreInvalid_ThenErrorShouldBeStoragePlaceCannotBeAddedToCourier(string name, int volume)
    {
        // Arrange.
        var courier = Create.Courier();

        // Act.
        var addingStoragePlaceResult = courier.AddStoragePlace(name, volume);

        // Assert.
        addingStoragePlaceResult.Error.Code.Should().Be(ErrorCodes.StoragePlaceCannotBeAddedToCourier);
    }

    [Fact]
    public void WhenAddingStoragePlace_AndNameAndVolumeAreValid_ThenResultIsSuccessShouldBeTrue()
    {
        // Arrange.
        var courier = Create.Courier();

        // Act.
        var addingStoragePlaceResult = courier.AddStoragePlace("test-storage-place", 2);
        
        // Assert.
        addingStoragePlaceResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void WhenAddingStoragePlace_AndNameWithVolumeAreValid_ThenCourierShouldHasAddedStoragePlace()
    {
        // Arrange.
        var courier = Create.Courier();
        const string storagePlaceName = "test-storage-place";
        const int storagePlaceVolume = 2;
        
        // Act.
        courier.AddStoragePlace(storagePlaceName, storagePlaceVolume);

        // Assert.
        courier.StoragePlaces.Count.Should().Be(2);
        courier.StoragePlaces.Should().ContainSingle(storagePlace
            => storagePlace.Name == storagePlaceName && storagePlace.TotalVolume == storagePlaceVolume);
    }

    [Fact]
    public void WhenCanTakingOrder_AndOrderIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange.
        var courier = Create.Courier();
        const Order order = null;

        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Fact]
    public void WhenCanTakingOrder_AndBagIsTooSmall_ThenResultShouldBeFalse()
    {
        // Arrange.
        var courier = Create.Courier();
        var order = Create.Order(volume: 11);

        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Value.Should().BeFalse();
    }
    
    [Fact]
    public void WhenCanTakingOrder_AndSingleStoragePlaceIsOccupied_ThenResultShouldBeFalse()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.TakeOrder(Create.Order());

        var order = Create.Order();

        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Value.Should().BeFalse();
    }

    [Fact]
    public void WhenCanTakingOrder_AndSingleStoragePlaceIsOccupiedAndTooSmall_ThenResultShouldBeFalse()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.TakeOrder(Create.Order(volume: 10));
        
        var order = Create.Order(volume: 11);

        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Value.Should().BeFalse();
    }
    
    [Fact]
    public void WhenCanTakingOrder_AndThereAre2StoragePlacesAndBothAreTooSmall_ThenResultShouldBeFalse()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.AddStoragePlace("bag2", 10);
        
        var order = Create.Order(volume: 11);

        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Value.Should().BeFalse();
    }
    
    [Fact]
    public void WhenCanTakingOrder_AndThereAre2StoragePlacesAndBothAreOccupied_ThenResultShouldBeFalse()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.AddStoragePlace("bag2", 10);
        
        courier.TakeOrder(Create.Order());
        courier.TakeOrder(Create.Order());
        
        var order = Create.Order();

        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Value.Should().BeFalse();
    }

    [Fact]
    public void WhenCanTakingOrder_AndThereAre2StoragePlacesWhereOneIsOccupiedAndAnotherIsTooSmall_ThenResultShouldBeFalse()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.TakeOrder( Create.Order(volume: 10));
        courier.AddStoragePlace("bag2", 1);
        
        var order = Create.Order(volume: 2);

        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Value.Should().BeFalse();
    }

    [Fact]
    public void WhenCanTakingOrder_AndSingleStoragePlaceIsSuitable_ThenResultShouldBeTrue()
    {
        // Arrange.
        var courier = Create.Courier();
        var order = Create.Order();

        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Value.Should().BeTrue();
    }

    [Fact]
    public void WhenCanTakingOrder_AndThereAre2StoragePlacesWhereOneIsOccupiedButAnotherIsSuitable_ThenResultShouldBeTrue()
    {
        // Arrange.
        var courier = Create.Courier();
        
        courier.TakeOrder(Create.Order());
        courier.AddStoragePlace("bag2", 10);
        
        var order = Create.Order();
        
        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Value.Should().BeTrue();
    }
    
    [Fact]
    public void WhenCanTakingOrder_AndThereAre2StoragePlacesWhereOneIsTooSmallButAnotherIsSuitable_ThenResultShouldBeTrue()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.AddStoragePlace("bag2", 100);
        
        var order = Create.Order(volume: 99);
        
        // Act.
        var canTakingOrderResult = courier.CanTakeOrder(order);

        // Assert.
        canTakingOrderResult.Value.Should().BeTrue();
    }

    [Fact]
    public void WhenTakingOrder_AndOrderIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange.
        var courier = Create.Courier();
        const Order order = null;

        // Act.
        var takingOrderResult = courier.TakeOrder(order);

        // Assert.
        takingOrderResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Fact]
    public void WhenTakingOrder_AndBagIsOccupied_ThenErrorShouldBeOrderCannotBeTakenByCourier()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.TakeOrder(Create.Order());
        
        var order = Create.Order();

        // Act.
        var takingOrderResult = courier.TakeOrder(order);

        // Assert.
        takingOrderResult.Error.Code.Should().Be(ErrorCodes.OrderCannotBeTakenByCourier);
    }

    [Fact]
    public void WhenTakingOrder_AndBagIsSuitable_ThenResultIsSuccessShouldBeTrue()
    {
        // Arrange.
        var courier = Create.Courier();
        var order = Create.Order();

        // Act.
        var takingOrderResult = courier.TakeOrder(order);

        // Assert.
        takingOrderResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void WhenTakingOrder_AndBagIsSuitable_ThenBagOrderIdShouldBeOrderId()
    {
        // Arrange.
        var courier = Create.Courier();
        var order = Create.Order();

        // Act.
        courier.TakeOrder(order);
        
        // Assert.
        courier.StoragePlaces.Single().OrderId.Should().Be(order.Id);
    }

    [Fact]
    public void WhenTakingOrder_AndBagAndBackpackAreSuitable_ThenOrderIdOfOnlyOneOfStoragePlacesShouldBeOrderIdAndStoragePlacesCountShouldBe2()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.AddStoragePlace("backpack", 10);
        
        var order = Create.Order();

        // Act.
        courier.TakeOrder(order);

        // Assert.
        courier.StoragePlaces.Should().ContainSingle(x => x.OrderId == order.Id);
        courier.StoragePlaces.Should().ContainSingle(x => x.OrderId == null);
        courier.StoragePlaces.Count.Should().Be(2);
    }

    [Fact]
    public void WhenTakingOrder_AndBagIsOccupiedButBackpackIsSuitable_ThenBagOrderIdShouldNotBeChangedButBackpackOrderIdShouldBeOrderIdAndStoragePlacesCountShouldBe2AndBagOrderIdShouldNotBeBackpackOrderId()
    {
        // Arrange.
        const string backpackName = "backpack";
        
        var courier = Create.Courier();
        courier.TakeOrder(Create.Order());
        var bagOrderId = courier.StoragePlaces.Single().OrderId;
        
        courier.AddStoragePlace(backpackName, 10);
        var order = Create.Order();
        
        // Act.
        courier.TakeOrder(order);

        // Assert.
        courier.StoragePlaces.Should().ContainSingle(x => x.OrderId == bagOrderId);
        courier.StoragePlaces.Should().ContainSingle(x => x.Name == backpackName && x.OrderId == order.Id);
        courier.StoragePlaces.Count.Should().Be(2);
        courier.StoragePlaces[0].OrderId.Should().NotBe(courier.StoragePlaces[1].OrderId!.Value);
    }

    [Fact]
    public void WhenTakingOrder_AndBagIsSuitableAndBackpackIsTooSmall_ThenBagOrderIdShouldBeOrderIdAndBackpackOrderIdShouldBeNullAndStoragePlacesCountShouldBe2()
    {
        // Arrange.
        const string backpackName = "backpack";
        
        var courier = Create.Courier();
        courier.AddStoragePlace(backpackName, 5);
        
        var order = Create.Order(volume: 7);
        
        // Act.
        courier.TakeOrder(order);
        
        // Assert.
        courier.StoragePlaces.Should().ContainSingle(x => x.OrderId == order.Id);
        courier.StoragePlaces.Should().ContainSingle(x => x.Name == backpackName && x.OrderId == null);
        courier.StoragePlaces.Count.Should().Be(2);
    }

    [Fact]
    public void WhenTakingOrder_AndThereIs5SuitableStoragePlaces_ThenOnlySingleOfThemShouldTakeOrder()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.AddStoragePlace("bag2", 10);
        courier.AddStoragePlace("bag3", 10);
        courier.AddStoragePlace("bag4", 10);
        courier.AddStoragePlace("bag5", 10);
        
        var order = Create.Order();
        
        // Act.
        courier.TakeOrder(order);
        
        // Assert.
        courier.StoragePlaces.Should().ContainSingle(x => x.OrderId == order.Id);
        courier.StoragePlaces.Count(x => x.OrderId == null).Should().Be(4);
        courier.StoragePlaces.Count.Should().Be(5);
    }

    [Fact]
    public void WhenCompletingOrder_AndOrderIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange.
        var courier = Create.Courier();
        const Order order = null;

        // Act.
        var completingOrderResult = courier.CompleteOrder(order);

        // Assert.
        completingOrderResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Fact]
    public void WhenCompletingOrder_AndCourierOnlyHasEmptyBag_ThenErrorShouldBeCourierHasNoSpecifiedOrder()
    {
        // Arrange.
        var courier = Create.Courier();
        var order = Create.Order();

        // Act.
        var completingOrderResult = courier.CompleteOrder(order);

        // Assert.
        completingOrderResult.Error.Code.Should().Be(ErrorCodes.CourierHasNoSpecifiedOrder);
    }

    [Fact]
    public void WhenCompletingOrder_AndBagIsOccupiedByAnotherOrder_ThenErrorShouldBeCourierHasNoSpecifiedOrder()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.TakeOrder(Create.Order());
        
        var order = Create.Order();

        // Act.
        var completingOrderResult = courier.CompleteOrder(order);
        
        // Assert.
        completingOrderResult.Error.Code.Should().Be(ErrorCodes.CourierHasNoSpecifiedOrder);
    }

    [Fact]
    public void WhenCompletingOrder_AndBagIsOccupiedByThisOrder_ThenResultIsSuccessShouldBeTrue()
    {
        // Arrange.
        var courier = Create.Courier();
        var order = Create.Order();
        courier.TakeOrder(order);

        // Act.
        var completingOrderResult = courier.CompleteOrder(order);

        // Assert.
        completingOrderResult.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void WhenCompletingOrder_AndBagIsOccupiedByThisOrder_BagOrderIdShouldBeNullAndStoragePlacesCountShouldBe1()
    {
        // Arrange.
        var courier = Create.Courier();
        var order = Create.Order();
        courier.TakeOrder(order);

        // Act.
        courier.CompleteOrder(order);
        
        // Assert.
        courier.StoragePlaces.Should().ContainSingle(x => x.OrderId == null);
        courier.StoragePlaces.Count.Should().Be(1);
    }

    [Fact]
    public void WhenCompletingOrder_AndCourierHas2StoragePlacesAndOneIsEmptyButAnotherIsOccupiedByThisOrder_ThenBothStoragePlaceOrderIdsShouldBeNulls()
    {
        // Arrange.
        var courier = Create.Courier();
        courier.AddStoragePlace("bag2", 10);
        var order = Create.Order();
        courier.TakeOrder(order);

        // Act.
        courier.CompleteOrder(order);
        
        // Assert.
        courier.StoragePlaces.Should().AllSatisfy(storagePlace => storagePlace.OrderId.Should().BeNull());
        courier.StoragePlaces.Count.Should().Be(2);
    }

    [Fact]
    public void WhenCompletingOrder_AndBagIsOccupiedByAnotherOrderAndBackpackIsOccupiedByThisOrder_ThenBagOrderIdShouldNotBeChangedButBackpackOrderIdShouldBeNullAndStoragePlacesCountShouldBe2()
    {
        // Arrange.
        const string backpackName = "backpack";
        var courier = Create.Courier();
        
        courier.TakeOrder(Create.Order());
        var bagOrderId = courier.StoragePlaces.Single().OrderId;

        courier.AddStoragePlace(backpackName, 10);
        var order = Create.Order();
        courier.TakeOrder(order);
        
        // Act.
        courier.CompleteOrder(order);

        // Assert.
        courier.StoragePlaces.Should().ContainSingle(x => x.OrderId == bagOrderId);
        courier.StoragePlaces.Should().ContainSingle(x => x.Name == backpackName && x.OrderId == null);
        courier.StoragePlaces.Count.Should().Be(2);
    }

    [Fact]
    public void WhenGettingRemainingMovesCount_AndTargetIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange.
        var courier = Create.Courier();
        const Location target = null;

        // Act.
        var gettingRemainingMovesCountResult = courier.GetRemainingMovesCount(target);

        // Assert.
        gettingRemainingMovesCountResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Theory, MemberData(nameof(MovesBetweenCourierAndLocation))]
    public void WhenGettingRemainingMovesCount_AndTargetIsNotNull_ThenResultShouldBeCorrect(Courier courier, Location target, int expectedRemainingMovesCount)
    {
        // Arrange and Act.
        var gettingRemainingMovesCountResult = courier.GetRemainingMovesCount(target);

        // Assert.
        gettingRemainingMovesCountResult.Value.Should().Be(expectedRemainingMovesCount);
    }

    [Fact]
    public void WhenMoving_AndTargetIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange.
        var courier = Create.Courier();
        const Location target = null;

        // Act.
        var movingResult = courier.Move(target);

        // Assert.
        movingResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Fact]
    public void WhenMoving_AndTargetIsNotNull_ThenResultIsSuccessShouldBeTrue()
    {
        // Arrange.
        var courier = Create.Courier();
        var target = Location.MinLocation;

        // Act.
        var movingResult = courier.Move(target);

        // Assert.
        movingResult.IsSuccess.Should().BeTrue();
    }
    
    [Theory, MemberData(nameof(CourierLocationsAfterMovingToTarget))]
    public void WhenMoving_AndTargetIsNotNull_ThenCourierLocationShouldBeExpected(Courier courier, Location target, Location expectedCourierLocation)
    {
        // Arrange and Act.
        courier.Move(target);

        // Assert.
        courier.Location.Should().Be(expectedCourierLocation);
    }
    
    public static TheoryData<Courier, Location, int> MovesBetweenCourierAndLocation()
    {
        var theoryData = new TheoryData<Courier, Location, int>();
        
        Add(Location.Create(1, 1).Value, 2, Location.Create(5, 5).Value, 4);
        Add(Location.Create(3, 1).Value, 2, Location.Create(5, 5).Value, 3);
        Add(Location.Create(5, 1).Value, 2, Location.Create(5, 5).Value, 2);
        Add(Location.Create(5, 3).Value, 2, Location.Create(5, 5).Value, 1);
        
        Add(Location.Create(1, 1).Value, 1, Location.Create(5, 5).Value, 8);
        Add(Location.Create(2, 1).Value, 1, Location.Create(5, 5).Value, 7);
        Add(Location.Create(3, 1).Value, 1, Location.Create(5, 5).Value, 6);
        Add(Location.Create(4, 1).Value, 1, Location.Create(5, 5).Value, 5);
        Add(Location.Create(5, 1).Value, 1, Location.Create(5, 5).Value, 4);
        Add(Location.Create(5, 2).Value, 1, Location.Create(5, 5).Value, 3);
        Add(Location.Create(5, 3).Value, 1, Location.Create(5, 5).Value, 2);
        Add(Location.Create(5, 4).Value, 1, Location.Create(5, 5).Value, 1);
        
        Add(Location.Create(1, 1).Value, 1, Location.Create(1, 1).Value, 0);
        Add(Location.Create(1, 1).Value, 5, Location.Create(1, 1).Value, 0);
        
        Add(Location.Create(1, 1).Value, 1, Location.Create(2, 1).Value, 1);
        Add(Location.Create(1, 1).Value, 5, Location.Create(2, 1).Value, 1);
        
        Add(Location.Create(1, 1).Value, 1, Location.Create(3, 1).Value, 2);
        Add(Location.Create(2, 1).Value, 1, Location.Create(3, 1).Value, 1);
        
        Add(Location.Create(1, 1).Value, 2, Location.Create(3, 1).Value, 1);
        Add(Location.Create(1, 1).Value, 2, Location.Create(4, 1).Value, 2);
        Add(Location.Create(1, 1).Value, 2, Location.Create(5, 1).Value, 2);
        
        return theoryData;

        void Add(Location courier, int courierSpeed, Location target, int movesCount)
            => theoryData.Add(Create.Courier(location: courier, speed: courierSpeed), target, movesCount);
    }

    public static TheoryData<Courier, Location, Location> CourierLocationsAfterMovingToTarget()
    {
        var theoryData = new TheoryData<Courier, Location, Location>();
        
        Add(Location.Create(1, 1).Value, 2, Location.Create(5, 5).Value, Location.Create(3, 1).Value);
        Add(Location.Create(3, 1).Value, 2, Location.Create(5, 5).Value, Location.Create(5, 1).Value);
        Add(Location.Create(5, 1).Value, 2, Location.Create(5, 5).Value, Location.Create(5, 3).Value);
        Add(Location.Create(5, 3).Value, 2, Location.Create(5, 5).Value, Location.Create(5, 5).Value);
        
        Add(Location.Create(1, 1).Value, 1, Location.Create(5, 5).Value, Location.Create(2, 1).Value);
        Add(Location.Create(2, 1).Value, 1, Location.Create(5, 5).Value, Location.Create(3, 1).Value);
        Add(Location.Create(3, 1).Value, 1, Location.Create(5, 5).Value, Location.Create(4, 1).Value);
        Add(Location.Create(4, 1).Value, 1, Location.Create(5, 5).Value, Location.Create(5, 1).Value);
        Add(Location.Create(5, 1).Value, 1, Location.Create(5, 5).Value, Location.Create(5, 2).Value);
        Add(Location.Create(5, 2).Value, 1, Location.Create(5, 5).Value, Location.Create(5, 3).Value);
        Add(Location.Create(5, 3).Value, 1, Location.Create(5, 5).Value, Location.Create(5, 4).Value);
        Add(Location.Create(5, 4).Value, 1, Location.Create(5, 5).Value, Location.Create(5, 5).Value);
        
        Add(Location.Create(1, 1).Value, 1, Location.Create(1, 1).Value, Location.Create(1, 1).Value);
        Add(Location.Create(1, 1).Value, 5, Location.Create(1, 1).Value, Location.Create(1, 1).Value);
        
        Add(Location.Create(1, 1).Value, 1, Location.Create(2, 1).Value, Location.Create(2, 1).Value);
        Add(Location.Create(1, 1).Value, 5, Location.Create(2, 1).Value, Location.Create(2, 1).Value);
        
        Add(Location.Create(1, 1).Value, 1, Location.Create(3, 1).Value, Location.Create(2, 1).Value);
        Add(Location.Create(2, 1).Value, 1, Location.Create(3, 1).Value, Location.Create(3, 1).Value);
        
        Add(Location.Create(1, 1).Value, 2, Location.Create(3, 1).Value, Location.Create(3, 1).Value);
        Add(Location.Create(1, 1).Value, 2, Location.Create(4, 1).Value, Location.Create(3, 1).Value);
        Add(Location.Create(1, 1).Value, 2, Location.Create(5, 1).Value, Location.Create(3, 1).Value);
        
        return theoryData;

        void Add(Location courier, int courierSpeed, Location target, Location expectedCourierLocation)
        {
            theoryData.Add(Create.Courier(location: courier, speed: courierSpeed), target, expectedCourierLocation);
        }
    }
}