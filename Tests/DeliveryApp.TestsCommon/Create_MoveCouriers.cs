using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;

namespace DeliveryApp.TestsCommon;

public static partial class Create
{
    public static MoveCouriersCommand MoveCouriersCommand()
        => new();
}