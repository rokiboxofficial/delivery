using System;
using System.Reflection;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Model.OrderAggregate;

public class OrderTests
{
    [Fact]
    public void WhenCheckingIsAggregateAssignableFromOrder_ThenResultShouldBeTrue()
    {
        // Act.
        var isAggregateAssignableFromOrder = typeof(Aggregate<Guid>).IsAssignableFrom(typeof(Order));

        // Assert.
        isAggregateAssignableFromOrder.Should().BeTrue();
    }
    
    [Fact]
    public void WhenGettingConstructors_ThenEachConstructorShouldBePrivate()
    {
        // Act.
        var constructors = typeof(Order).GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Assert.
        constructors.Should().AllSatisfy(x => x.IsPrivate.Should().BeTrue());
    }
    
    [Fact]
    public void WhenCreating_AndOrderIdIsEmptyGuid_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange and Act.
        var creatingResult = Order.Create(Guid.Empty, Location.MinLocation, 1);
        
        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Fact]
    public void WhenCreating_AndLocationIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange and Act.
        var creatingResult = Order.Create(Guid.NewGuid(), null, 1);

        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void WhenCreating_AndAndVolumeIsNegativeOr0_ThenErrorShouldBeValueIsInvalid(int volume)
    {
        // Arrange and Act.
        var creatingResult = Order.Create(Guid.NewGuid(), Location.MinLocation, volume);
        
        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsInvalid);
    }

    [Fact]
    public void WhenCreating_AndOrderIdIsNotEmptyGuidAndLocationIsNotNullAndVolumeIsGreaterThan0_ThenResultValueShouldHasInitialOrderIdAndInitialLocationAndInitialVolume_AndStatusShouldBeCreated()
    {
        // Arrange.
        var initialOrderId = Guid.NewGuid();
        var initialLocation = Location.MinLocation;
        const int initialVolume = 1;

        // Act.
        var creatingResult = Order.Create(initialOrderId, initialLocation, initialVolume).Value;

        // Assert.
        new { creatingResult.Id, creatingResult.Location, creatingResult.Volume, creatingResult.Status }.Should().Be(
            new { Id = initialOrderId, Location = initialLocation, Volume = initialVolume, Status = OrderStatus.Created });
    }

    [Fact]
    public void WhenAssigning_AndCourierIsNull_ThenErrorShouldBeValueIsRequired()
    {
        // Arrange.
        var order = Create.Order();
        const Courier courier = null;

        // Act.
        var assigningResult = order.Assign(courier);

        // Assert.
        assigningResult.Error.Code.Should().Be(ErrorCodes.ValueIsRequired);
    }

    [Theory, MemberData(nameof(OrdersWhereStatusIsNotCreated))]
    public void WhenAssigning_AndOrderStatusIsNotCreatedAndCourierIsNotNull_ThenErrorShouldBeOnlyOrderWithStatusCreatedCanBeAssigned(Order order)
    {
        // Arrange.
        var courier = Create.Courier();

        // Act.
        var assigningResult = order.Assign(courier);

        // Assert.
        assigningResult.Error.Code.Should().Be(ErrorCodes.OnlyOrderWithStatusCreatedCanBeAssigned);
    }

    [Fact]
    public void WhenAssigning_AndOrderStatusIsCreatedAndCourierIsNotNull_ThenResultIsSuccessShouldBeTrueAndOrderCourierIdShouldBeCourierIdAndOrderStatusShouldBeAssigned()
    {
        // Arrange.
        var order = Create.Order();
        var courier = Create.Courier();
        
        // Act.
        var assigningResult = order.Assign(courier);

        // Assert.
        new { assigningResult.IsSuccess, order.CourierId, order.Status }.Should().Be(
            new { IsSuccess = true, CourierId = (Guid?) courier.Id, Status = OrderStatus.Assigned });
    }

    [Theory, MemberData(nameof(OrdersWhereStatusIsNotAssigned))]
    public void WhenCompleting_AndOrderStatusIsNotAssigned_ThenErrorShouldBeNotAssignedOrderCannotBeCompleted(Order order)
    {
        // Arrange and Act.
        var completingResult = order.Complete();

        // Assert.
        completingResult.Error.Code.Should().Be(ErrorCodes.NotAssignedOrderCannotBeCompleted);
    }

    [Fact]
    public void WhenCompleting_AndOrderCourierIdIsNull_ThenErrorShouldBeNotAssignedOrderCannotBeCompleted()
    {
        // Arrange.
        var order = Create.Order();

        // Act.
        var completingResult = order.Complete();

        // Assert.
        completingResult.Error.Code.Should().Be(ErrorCodes.NotAssignedOrderCannotBeCompleted);
    }

    [Fact]
    public void WhenCompleting_AndOrderStatusIsAssignedAndOrderCourierIdIsNotNull_ThenResultIsSuccessShouldBeTrueAndOrderStatusShouldBeCompletedAndOrderCourierIdShouldBeNotChanged()
    {
        // Arrange.
        var assignedOrder = Setup.AssignedOrder();
        var courierId = assignedOrder.CourierId;

        // Act.
        var completingResult = assignedOrder.Complete();

        // Assert.
        new { completingResult.IsSuccess, assignedOrder.Status, assignedOrder.CourierId }.Should().Be(
            new { IsSuccess = true, Status = OrderStatus.Completed, CourierId = courierId });
    }
    
    public static TheoryData<Order> OrdersWhereStatusIsNotCreated()
        => [Setup.AssignedOrder(), Setup.CompletedOrder()];

    public static TheoryData<Order> OrdersWhereStatusIsNotAssigned()
        => [Create.Order(), Setup.CompletedOrder()];
}