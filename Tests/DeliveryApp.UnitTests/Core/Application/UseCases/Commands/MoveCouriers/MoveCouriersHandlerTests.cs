using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports.Repositories;
using DeliveryApp.TestsCommon;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Application.UseCases.Commands.MoveCouriers;

public class MoveCouriersHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly ICourierRepository _courierRepositoryMock = Substitute.For<ICourierRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly MoveCouriersHandler _moveCouriersHandler;

    public MoveCouriersHandlerTests()
    {
        _moveCouriersHandler = new MoveCouriersHandler(_courierRepositoryMock, _orderRepositoryMock, _unitOfWorkMock);
    }
    
    [Fact]
    public async Task WhenHandling_AndRepositoryHasNoOrdersInAssignedStatus_ThenResultShouldBeSuccessful()
    {
        // Arrange.
        var moveCouriersCommand = Create.MoveCouriersCommand();
        _orderRepositoryMock.GetAllInAssignedStatusAsync().Returns([]);

        // Act.
        var handlingResult = await _moveCouriersHandler.Handle(moveCouriersCommand, CancellationToken.None);

        // Assert.
        handlingResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task WhenHandling_AndGetAllInAssignedStatusReturnedOrderWhereCourierIdIsNull_ThenInvalidOperationExceptionShouldBeThrown()
    {
        // Arrange.
        var moveCouriersCommand = Create.MoveCouriersCommand();
        _orderRepositoryMock.GetAllInAssignedStatusAsync().Returns([Create.Order()]);

        // Act.
        var act = () => _moveCouriersHandler.Handle(moveCouriersCommand, CancellationToken.None);

        // Assert.
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task WhenHandling_AndCourierRepositoryNotFoundCourier_ThenInvalidOperationExceptionShouldBeThrown()
    {
        // Arrange.
        var moveCouriersCommand = Create.MoveCouriersCommand();
        _orderRepositoryMock.GetAllInAssignedStatusAsync().Returns([Setup.AssignedOrder()]);
        _courierRepositoryMock.GetAsync(Arg.Any<Guid>()).Returns(Maybe<Courier>.None);
        
        // Act.
        var act = () => _moveCouriersHandler.Handle(moveCouriersCommand, CancellationToken.None);
        
        // Assert.
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task WhenHandling_AndRepositoriesHaveAssignedOrderAndCourier_ThenResultShouldBeSuccessfulAndEntitiesShouldBeUpdated()
    {
        // Arrange.
        var moveCouriersCommand = Create.MoveCouriersCommand();
        var order = Create.Order();
        var courier = Create.Courier();
        courier.TakeOrder(order);
        order.Assign(courier);
        _orderRepositoryMock.GetAllInAssignedStatusAsync().Returns([order]);
        _courierRepositoryMock.GetAsync(Arg.Any<Guid>()).Returns(courier);

        // Act.
        var handlingResult = await _moveCouriersHandler.Handle(moveCouriersCommand, CancellationToken.None);

        // Assert.
        handlingResult.IsSuccess.Should().BeTrue();
        _orderRepositoryMock.Received(1).Update(Arg.Any<Order>());
        _courierRepositoryMock.Received(1).Update(Arg.Any<Courier>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task WhenHandling_AndRepositoriesHaveAssignedOrderAndCourierAndCourierReachedDestination_ThenResultShouldBeSuccessfulAndOrderShouldBeCompletedForCourierAndOrder()
    {
        // Arrange.
        var moveCouriersCommand = Create.MoveCouriersCommand();
        var order = Create.Order();
        var courier = Create.Courier();
        courier.TakeOrder(order);
        order.Assign(courier);
        
        _orderRepositoryMock.GetAllInAssignedStatusAsync().Returns([order]);
        _courierRepositoryMock.GetAsync(Arg.Any<Guid>()).Returns(courier);

        // Act.
        var handlingResult = await _moveCouriersHandler.Handle(moveCouriersCommand, CancellationToken.None);
        
        // Assert.
        handlingResult.IsSuccess.Should().BeTrue();
        courier.StoragePlaces.Should().AllSatisfy(x => x.OrderId.Should().BeNull());
        order.Status.Should().Be(OrderStatus.Completed);
    }
}