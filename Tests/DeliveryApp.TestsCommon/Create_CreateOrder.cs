using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using static DeliveryApp.Core.Application.UseCases.Commands.CreateOrder.CreateOrderCommand;

namespace DeliveryApp.TestsCommon;

public static partial class Create
{
    public static CreateOrderCommand CreateOrderCommand()
        => Create(Guid.NewGuid(), "street", 1).Value;
}