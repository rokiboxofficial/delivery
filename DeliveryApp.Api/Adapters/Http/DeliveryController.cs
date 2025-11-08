using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllShortCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedShortOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http;

public sealed class DeliveryController(IMediator mediator) : DefaultApiController
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public override Task<IActionResult> CreateCourier(NewCourier newCourier)
        => throw new NotImplementedException();

    public override async Task<IActionResult> CreateOrder()
    {
        var createOrderCommand = CreateOrderCommand.Create(
            Guid.NewGuid(),
            "Несуществующая",
            1).Value;
        var result = await _mediator.Send(createOrderCommand);
        
        return result.IsSuccess
            ? Ok()
            : Conflict();
    }

    public override async Task<IActionResult> GetCouriers()
    {
        var getAllShortCouriersQuery = new GetAllShortCouriersQuery();
        var result = await _mediator.Send(getAllShortCouriersQuery);

        if (result?.Couriers is var couriers && couriers is null) return NotFound();
        return Ok(couriers.Select(courier => new Courier 
        {
            Id = courier.Id,
            Location = new Location
            {
                X = courier.Location.X,
                Y = courier.Location.Y,
            },
            Name = courier.Name,
        }));
    }

    public override async Task<IActionResult> GetOrders()
    {
        var getNotCompletedShortOrders = new GetNotCompletedShortOrdersQuery();
        var result = await _mediator.Send(getNotCompletedShortOrders);
        
        if (result?.Orders is var orders && orders is null) return NotFound();
        return Ok(orders.Select(order => new Order
        {
            Id = order.Id,
            Location = new Location
            {
                X = order.Location.X,
                Y = order.Location.Y
            }
        }));
    }
}