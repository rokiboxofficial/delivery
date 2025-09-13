using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.SharedKernel;

public sealed class Location : ValueObject
{
    /// <summary>
    /// The inclusive lower bound of location.
    /// </summary>
    public static readonly Location MinLocation = new(1, 1);
    
    /// <summary>
    /// The inclusive upper bound of location.
    /// </summary>
    public static readonly Location MaxLocation = new(10, 10);
    
    [ExcludeFromCodeCoverage]
    private Location()
    {
        
    }

    private Location(int x, int y) : this()
    {
        X = x;
        Y = y;
    }
    
    public int X { get; }
    public int Y { get; }

    public static Result<Location, Error> Create(int x, int y)
    {
        if (x < MinLocation.X || x > MaxLocation.X) return GeneralErrors.ValueIsInvalid(nameof(x));
        if (y < MinLocation.Y || y > MaxLocation.Y) return GeneralErrors.ValueIsInvalid(nameof(y));

        return new Location(x, y);
    }
    
    public Result<int, Error> DistanceTo(Location target)
    {
        if (target is null) return GeneralErrors.ValueIsRequired(nameof(target));
        
        var distance = Math.Abs(target.X - X) + Math.Abs(target.Y - Y);
        return distance;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}