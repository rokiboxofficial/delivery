using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports.Repositories;
using DeliveryApp.TestsCommon;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Application.UseCases.Commands.AssignOrder;

public class AssignOrderHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly ICourierRepository _courierRepositoryMock = Substitute.For<ICourierRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IDispatchService _dispatchServiceMock = Substitute.For<IDispatchService>();
    private readonly ILogger<AssignOrderHandler> _loggerMock = Substitute.For<ILogger<AssignOrderHandler>>();
    private readonly AssignOrderHandler _assignOrderHandler;

    public AssignOrderHandlerTests()
    {
        _assignOrderHandler = new AssignOrderHandler(_orderRepositoryMock, _courierRepositoryMock, _unitOfWorkMock, _dispatchServiceMock, _loggerMock);
    }
    
    [Fact]
    public async Task WhenHandling_AndRepositoryHasNoOrdersInCreatedStatus_ThenResultShouldBeSuccessful()
    {
        // Arrange.
        var assignOrderCommand = Create.AssignOrderCommand();
        _orderRepositoryMock.GetFirstInCreatedStatusAsync().Returns(Maybe<Order>.None);

        // Act.
        var handlingResult = await _assignOrderHandler.Handle(assignOrderCommand, CancellationToken.None);

        // Assert.
        handlingResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task WhenHandling_AndRepositoryHasNoFreeCourier_ThenResultShouldBeSuccessful()
    {
        // Arrange.
        var assignOrderCommand = Create.AssignOrderCommand();
        _orderRepositoryMock.GetFirstInCreatedStatusAsync().Returns(Create.Order());
        _courierRepositoryMock.GetAllFreeCouriersAsync().Returns([]);

        // Act.
        var handlingResult = await _assignOrderHandler.Handle(assignOrderCommand, CancellationToken.None);
        
        // Assert.
        handlingResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task WhenHandling_AndRepositoryHasOrderInCreatedStatusAndAtLeastOneFreeCourier_ThenResultShouldBeSuccessfulAndServicesShouldBeCalled()
    {
        // Arrange.
        var assignOrderCommand = Create.AssignOrderCommand();
        _orderRepositoryMock.GetFirstInCreatedStatusAsync().Returns(Create.Order());
        _courierRepositoryMock.GetAllFreeCouriersAsync().Returns([Create.Courier()]);

        // Act.
        var handlingResult = await _assignOrderHandler.Handle(assignOrderCommand, CancellationToken.None);
        
        // Assert.
        handlingResult.IsSuccess.Should().BeTrue();
        _dispatchServiceMock.Received(1).Dispatch(Arg.Any<Order>(), Arg.Any<IList<Courier>>());
        _courierRepositoryMock.Received(1).Update(Arg.Any<Courier>());
        _orderRepositoryMock.Received(1).Update(Arg.Any<Order>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}