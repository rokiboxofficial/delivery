using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

public sealed class OrderStatus : ValueObject
{
    [ExcludeFromCodeCoverage]
    private OrderStatus()
    {
    }
    
    private OrderStatus(string name)
    {
        Name = name;
    }

    public static OrderStatus Created => new(nameof(Created));
    public static OrderStatus Assigned => new(nameof(Assigned));
    public static OrderStatus Completed => new(nameof(Completed));
    
    public string Name { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}