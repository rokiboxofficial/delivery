using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Services;

public class DispatchServiceTests
{
    [Fact]
    public void WhenGettingInterfaces_ThenInterfacesShouldContainSingleWithNameIDispatchService()
    {
        // Act.
        var interfaces = typeof(DispatchService).GetInterfaces();

        // Assert.
        interfaces.Should().ContainSingle(x => x.Name == $"I{nameof(DispatchService)}");
    }
    
    [Fact]
    public void WhenDispatching_AndOrderIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange.
        const Order order = null;
        List<Courier> couriers = [Create.Courier()];
        var dispatchService = Create.DispatchService();

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Theory, MemberData(nameof(NotCreatedOrders))]
    public void WhenDispatching_AndOrderStatusIsNotCreated_ThenErrorShouldBeOnlyOrderWithStatusCreatedCanBeDispatched(Order order)
    {
        // Arrange.
        List<Courier> couriers = [Create.Courier()];
        var dispatchService = Create.DispatchService();
        
        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Error.Code.Should().Be(ErrorCodes.OnlyOrderWithStatusCreatedCanBeDispatched);
    }

    [Fact]
    public void WhenDispatching_AndCouriersIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        var order = Create.Order();
        const List<Courier> couriers = null;

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Fact]
    public void WhenDispatching_AndCouriersAreEmpty_ThenErrorShouldBeCollectionIsTooSmall()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        var order = Create.Order();
        var couriers = Array.Empty<Courier>();

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Error.Code.Should().Be(ErrorCodes.CollectionIsTooSmall);
    }

    [Fact]
    public void WhenDispatching_AndSingleCourierIsBusy_ThenErrorShouldBeSuitableCourierNotFound()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        List<Courier> couriers = [Setup.BusyCourier()];
        var order = Create.Order();

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Error.Code.Should().Be(ErrorCodes.SuitableCourierNotFound);
    }

    [Fact]
    public void WhenDispatching_AndSingleCourierHasTooSmallStorage_ThenErrorShouldBeSuitableCourierNotFound()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        List<Courier> couriers = [Create.Courier()];
        var order = Create.Order(volume: 100);
        
        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Error.Code.Should().Be(ErrorCodes.SuitableCourierNotFound);
    }

    [Fact]
    public void WhenDispatching_AndSingleCourierIsBusyAndHasTooSmallStorage_ThenErrorShouldBeSuitableCourierNotFound()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        List<Courier> couriers = [Setup.BusyCourier()];
        var order = Create.Order(volume: 100);

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Error.Code.Should().Be(ErrorCodes.SuitableCourierNotFound);
    }

    [Fact]
    public void WhenDispatching_AndThereAre5CouriersWhere3AreBusyAnd2HaveToSmallStorages_ThenErrorShouldBeSuitableCourierNotFound()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        
        List<Courier> couriers = [
            Setup.BusyCourier(),
            Setup.BusyCourier(),
            Setup.BusyCourier(),
            Create.Courier(),
            Create.Courier()
        ];
        
        var order = Create.Order(volume: 11);

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Error.Code.Should().Be(ErrorCodes.SuitableCourierNotFound);
    }

    [Fact]
    public void WhenDispatching_AndThereIsSingleSuitableCourier_ThenResultShouldBeThatCourierWithTakenOrderAndOrderShouldBeAssignedToThatCourier()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        var order = Create.Order();
        var courier = Create.Courier();
        List<Courier> couriers = [courier];

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Value.Should().Be(courier);
        courier.StoragePlaces.Single().OrderId.Should().Be(order.Id);
        order.CourierId.Should().Be(courier.Id);
    }

    [Fact]
    public void WhenDispatching_AndAllCouriersAreSuitableAndHaveSameRemainingMovesCount_ThenOnlyReturnedCourierOrderIdShouldNotBeNullAndOrderShouldBeAssignedToReturnedCourier()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        var order = Create.Order();
        List<Courier> couriers = [
            Create.Courier(),
            Create.Courier(),
            Create.Courier(), 
            Create.Courier(),
            Create.Courier(),
            Create.Courier()
        ];
        
        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);
        
        // Assert.
        dispatchingResult.Value.StoragePlaces[0].OrderId.Should().Be(order.Id);
        order.CourierId.Should().Be(dispatchingResult.Value.Id);
        
        couriers.Should().ContainSingle(x => x.StoragePlaces[0].OrderId == order.Id);
        couriers.Count(x => x.StoragePlaces[0].OrderId is null).Should().Be(couriers.Count - 1);
    }

    [Fact]
    public void WhenDispatching_AndAllCouriersAreSuitable_ThenNearestCourierShouldBeDispatched()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        var order = Create.Order(location: Create.Location());

        var nearest = Create.Courier(location: Create.Location(x: 3));
        
        List<Courier> couriers = [
            Create.Courier(location: Create.Location(x: 5)),
            Create.Courier(location: Create.Location(x: 4)), 
            Create.Courier(location: Create.Location(x: 6)),
            nearest,
            Create.Courier(location: Create.Location(x: 4)),
            Create.Courier(location: Create.Location(x: 10))
        ];

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Value.Should().Be(nearest);
        nearest.StoragePlaces[0].OrderId.Should().Be(order.Id);
        order.CourierId.Should().Be(nearest.Id);
    }

    [Fact]
    public void WhenDispatching_AndFewCouriersAreSuitableAndFewCouriersAreNot_ThenNearestSuitableCourierShouldBeDispatched()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        var order = Create.Order(location: Create.Location());
        
        var nearest = Create.Courier(location: Create.Location(x: 3));

        List<Courier> couriers = [
            Create.Courier(location: Create.Location(x: 5)),
            Setup.BusyCourier(order.Location),
            Create.Courier(location: Create.Location(x: 6)),
            nearest,
            Setup.BusyCourier(),
            Create.Courier(location: Create.Location(x: 10)),
        ];

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Value.Should().Be(nearest);
        nearest.StoragePlaces[0].OrderId.Should().Be(order.Id);
        order.CourierId.Should().Be(nearest.Id);
    }
    
    [Fact]
    public void WhenDispatching_AndFirstCourierIsNearestButBusyAndAnotherIsSuitableButFarthest_ThenFarthestShouldBeDispatched()
    {
        // Arrange.
        var dispatchService = Create.DispatchService();
        var order = Create.Order(location: Create.Location());

        var nearest = Setup.BusyCourier(location: Create.Location(x: 2));
        var farthest = Create.Courier(location: Create.Location(x: 10));

        List<Courier> couriers = [nearest, farthest];

        // Act.
        var dispatchingResult = dispatchService.Dispatch(order, couriers);

        // Assert.
        dispatchingResult.Value.Should().Be(farthest);
        farthest.StoragePlaces[0].OrderId.Should().Be(order.Id);
        order.CourierId.Should().Be(farthest.Id);
    }
    
    public static TheoryData<Order> NotCreatedOrders()
        => [Setup.AssignedOrder(), Setup.CompletedOrder()];
}