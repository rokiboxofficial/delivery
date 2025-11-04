using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;

namespace DeliveryApp.TestsCommon;

public static partial class Create
{
    public static AssignOrderCommand AssignOrderCommand()
        => new ();
}