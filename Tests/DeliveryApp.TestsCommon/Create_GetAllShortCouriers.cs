using DeliveryApp.Core.Application.UseCases.Queries.GetAllShortCouriers;

namespace DeliveryApp.TestsCommon;

public static partial class Create
{
    public static GetAllShortCouriersQuery GetAllShortCouriersQuery()
        => new();
}