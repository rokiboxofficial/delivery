using System;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.Services;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports.Repositories;
using DeliveryApp.TestsCommon;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Application.UseCases.Commands.CreateOrder;

public class CreateOrderHandlerTests
{
    private readonly IRandomLocationProvider _randomLocationProvider = Substitute.For<IRandomLocationProvider>();
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly CreateOrderHandler _createOrderHandler;

    public CreateOrderHandlerTests()
    {
        _createOrderHandler = new CreateOrderHandler(_randomLocationProvider, _orderRepositoryMock, _unitOfWorkMock);
    }
    
    [Fact]
    public async Task WhenHandling_AndRandomLocationProviderReturnedNull_ThenResultShouldBeError()
    {
        // Arrange.
        var createOrderCommand = Create.CreateOrderCommand();
        _randomLocationProvider.GetRandomLocation().Returns((Location)null);

        // Act.
        var handlingResult = await _createOrderHandler.Handle(createOrderCommand, CancellationToken.None);

        // Assert.
        handlingResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task WhenHandling_AndOrderRepositoryReturnedOrder_ThenErrorShouldBeOrderAlreadyExists()
    {
        // Arrange.
        var createOrderCommand = Create.CreateOrderCommand();
        _randomLocationProvider.GetRandomLocation().Returns(Create.Location());
        _orderRepositoryMock.GetAsync(Arg.Any<Guid>()).Returns(Create.Order());
        
        // Act.
        var handlingResult = await _createOrderHandler.Handle(createOrderCommand, CancellationToken.None);

        // Assert.
        handlingResult.Error.Code.Should().Be(ErrorCodes.OrderAlreadyExists);
    }

    [Fact]
    public async Task WhenHandling_AndOrderRepositoryReturnedNull_ThenResultShouldBeSuccessfulAndServicesShouldBeCalled()
    {
        // Arrange.
        var createOrderCommand = Create.CreateOrderCommand();
        _randomLocationProvider.GetRandomLocation().Returns(Create.Location());
        _orderRepositoryMock.GetAsync(Arg.Any<Guid>()).Returns((Order) null);

        // Act.
        var handlingResult = await _createOrderHandler.Handle(createOrderCommand, CancellationToken.None);

        // Assert.
        handlingResult.IsSuccess.Should().BeTrue();
        await _orderRepositoryMock.Received(1).AddAsync(Arg.Any<Order>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
}