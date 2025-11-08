using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Api.Adapters.Http;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllShortCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedShortOrders;
using DeliveryApp.Core.Ports.ReadModelProviders;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Api.Adapters.Http;

public class DeliveryControllerTests
{
    private readonly IMediator _mediatorMock = Substitute.For<IMediator>();
    private readonly DeliveryController _deliveryController;
    
    public DeliveryControllerTests()
    {
        _deliveryController = new DeliveryController(_mediatorMock);
    }

    [Fact]
    public async Task WhenCreatingOrder_AndMediatorReturnedSuccess_ThenMediatorShouldBeCalledAndResultShouldBeOk()
    {
        // Arrange.
        _mediatorMock
            .Send(Arg.Any<CreateOrderCommand>())
            .Returns(UnitResult.Success<Error>());

        // Act.
        var actionResult = await _deliveryController.CreateOrder();

        // Assert.
        await _mediatorMock.Received(1).Send(Arg.Any<CreateOrderCommand>());
        actionResult.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task WhenCreatingOrder_AndMediatorReturnedFailure_ThenMediatorShouldBeCalledAndResultShouldBeConflict()
    {
        // Arrange.
        _mediatorMock
            .Send(Arg.Any<CreateOrderCommand>())
            .Returns(UnitResult.Failure(GeneralErrors.NotFound()));

        // Act.
        var actionResult = await _deliveryController.CreateOrder();

        // Assert.
        await _mediatorMock.Received(1).Send(Arg.Any<CreateOrderCommand>());
        actionResult.Should().BeOfType<ConflictResult>();
    }

    [Fact]
    public async Task WhenGettingCouriers_AndMediatorReturnedCouriersResponse_ThenMediatorShouldBeCalledAndResultShouldBeOkObject()
    {
        // Arrange.
        _mediatorMock
            .Send(Arg.Any<GetAllShortCouriersQuery>())
            .Returns(new GetAllShortCouriersResponse([]));

        // Act.
        var actionResult = await _deliveryController.GetCouriers();

        // Assert.
        await _mediatorMock.Received(1).Send(Arg.Any<GetAllShortCouriersQuery>());
        actionResult.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task WhenGettingCouriers_AndMediatorReturnedNull_ThenMediatorShouldBeCalledAndResultShouldBeNotFound()
    {
        // Arrange.
        _mediatorMock
            .Send(Arg.Any<GetAllShortCouriersQuery>())
            .Returns((GetAllShortCouriersResponse)null);

        // Act.
        var actionResult = await _deliveryController.GetCouriers();

        // Assert.
        await _mediatorMock.Received(1).Send(Arg.Any<GetAllShortCouriersQuery>());
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task WhenGettingOrders_AndMediatorReturnedOrdersResponse_ThenMediatorShouldBeCalledAndResultShouldBeOkObject()
    {
        // Arrange.
        _mediatorMock
            .Send(Arg.Any<GetNotCompletedShortOrdersQuery>())
            .Returns(new GetNotCompletedShortOrdersResponse([]));

        // Act.
        var actionResult = await _deliveryController.GetOrders();

        // Assert.
        await _mediatorMock.Received(1).Send(Arg.Any<GetNotCompletedShortOrdersQuery>());
        actionResult.Should().BeOfType<OkObjectResult>();
    }
    
    [Fact]
    public async Task WhenGettingOrders_AndMediatorReturnedNull_ThenMediatorShouldBeCalledAndResultShouldBeNotFound()
    {
        // Arrange.
        _mediatorMock
            .Send(Arg.Any<GetNotCompletedShortOrdersQuery>())
            .Returns((GetNotCompletedShortOrdersResponse)null);

        // Act.
        var actionResult = await _deliveryController.GetOrders();

        // Assert.
        await _mediatorMock.Received(1).Send(Arg.Any<GetNotCompletedShortOrdersQuery>());
        actionResult.Should().BeOfType<NotFoundResult>();
    }
}