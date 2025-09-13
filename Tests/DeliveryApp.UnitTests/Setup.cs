using DeliveryApp.Core.Domain.Model.OrderAggregate;

namespace DeliveryApp.UnitTests;

public static class Setup
{
    public static Order AssignedOrder()
    {
        var assigned = Create.Order();
        assigned.Assign(Create.Courier());
        return assigned;
    }

    public static Order CompletedOrder()
    {
        var completed = Create.Order();
        completed.Assign(Create.Courier());
        completed.Complete();
        return completed;
    }
}