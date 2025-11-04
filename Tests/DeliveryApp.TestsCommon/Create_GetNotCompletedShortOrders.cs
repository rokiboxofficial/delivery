using DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedShortOrders;

namespace DeliveryApp.TestsCommon;

public static partial class Create
{
    public static GetNotCompletedShortOrdersQuery GetNotCompletedShortOrdersQuery()
        => new();
}