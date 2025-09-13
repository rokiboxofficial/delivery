using System;
using System.Reflection;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Model.SharedKernel;

public class LocationTests
{
    [Fact]
    public void WhenCheckingIsValueObjectAssignableFromLocation_ThenResultShouldBeTrue()
    {
        // Act.
        var isValueObjectAssignableFromLocation = typeof(ValueObject).IsAssignableFrom(typeof(Location));

        // Assert.
        isValueObjectAssignableFromLocation.Should().BeTrue();
    }
    
    [Fact]
    public void WhenGettingConstructors_ThenEachConstructorShouldBePrivate()
    {
        // Act.
        var constructors = typeof(Location).GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Assert.
        constructors.Should().AllSatisfy(x => x.IsPrivate.Should().BeTrue());
    }
    
    [Theory]
    [MemberData(nameof(BothXandYInBounds_ButAtLeastXorYIsMinOrMaxBound_Data))]
    public void WhenCreatingLocation_AndBothXandYInBounds_ButAtLeastXorYIsMinOrMaxBound_ThenCreatingResultXandYShouldBeInitialXandY(int initialX, int initialY)
    {
        // Arrange and Act.
        var creatingResult = Location.Create(initialX, initialY);
        
        // Assert.
        new { creatingResult.Value.X, creatingResult.Value.Y }.Should().Be(new { X = initialX, Y = initialY });
    }

    [Theory]
    [MemberData(nameof(AtLeastXorYIsGreaterOrLowerThanMaxOrMinBound_Data))]
    public void WhenCreatingLocation_AndAtLeastXorYIsGreaterOrLowerThanMaxOrMinBound_ThenCreatingResultErrorShouldBeValueIsInvalid(int? initialX, int? initialY)
    {
        // If either MinLocation or MaxLocation is either int.MinValue or int.MaxValue, then we should skip
        var minLocationHasIntMinValue =
            Location.MinLocation is var min && (min.X is int.MinValue || min.Y is int.MinValue);
        var maxLocationHasIntMaxValue =
            Location.MaxLocation is var max && (max.X is int.MaxValue || max.Y is int.MaxValue);
        if (initialX is null && initialY is null && (minLocationHasIntMinValue || maxLocationHasIntMaxValue )) return;
        
        // Arrange and Act.
        var creatingResult = Location.Create(initialX!.Value, initialY!.Value);
        
        // Assert.
        creatingResult.Error.Code.Should().Be(ErrorCodes.ValueIsInvalid);
    }
    
    [Fact]
    public void WhenGettingMinAndMaxLocations_ThenMaxLocationXandYShouldBeGreaterThanOrEqualToMinLocationXandY()
    {
        // Act.
        var minLocation = Location.MinLocation;
        var maxLocation = Location.MaxLocation;

        // Assert.
        maxLocation.X.Should().BeGreaterThanOrEqualTo(minLocation.X);
        maxLocation.Y.Should().BeGreaterThanOrEqualTo(minLocation.Y);
    }

    [Theory]
    [MemberData(nameof(DistanceBetweenLocations_Data))]
    public void WhenDistancingToLocation_ThenDistancingResultValueShouldBeExpectedDistance(Location first, Location second, int expectedDistance)
    {
        // If this test would fail at least once due changing of MinLocation or MaxLocation
        // then consider either somehow changing MaxLocation and MinLocation (e.g. via reflection)
        // or auto disabling some testcases like already done in "AtLeastXorYIsGreaterOrLowerThanMaxOrMinBound_Data"

        // Act.
        var distancingResult = first.DistanceTo(second);

        // Assert.
        distancingResult.Value.Should().Be(expectedDistance);
    }

    [Theory]
    [MemberData(nameof(EqualLocations_Data))]
    public void WhenEqualingByOperatorAndByEqualsMethod_AndLocationsAreEqual_ThenLocationsEqualityByOperatorAndMethodShouldBeTrue(Location first, Location second)
    {
        // Arrange and Act.
        var equalByOperator = first == second;
        var equalByMethod = first.Equals(second);
        
        // Assert.
        new {equalByOperator, equalByMethod}.Should().Be(new { equalByOperator = true, equalByMethod = true });
    }

    [Theory]
    [MemberData(nameof(NotEqualLocations_Data))]
    public void WhenEqualingByOperatorAndByEqualsMethod_AndLocationsAreNotEqual_ThenLocationsEqualityByOperatorAndMethodShouldBeFalse(Location first, Location second)
    {
        // Arrange and Act.
        var equalByOperator = first == second;
        var equalByMethod = first.Equals(second);

        // Assert.
        new {equalByOperator, equalByMethod}.Should().Be(new { equalByOperator = false, equalByMethod = false });
    }
    
    public static TheoryData<int, int> BothXandYInBounds_ButAtLeastXorYIsMinOrMaxBound_Data()
    {
        var middle = Create.MiddleLocation();
        
        return new TheoryData<int, int>
        {
            { Location.MinLocation.X, Location.MinLocation.Y },
            { Location.MinLocation.X, middle.Y },
            { Location.MinLocation.X, Location.MaxLocation.Y },
            
            { middle.X, Location.MinLocation.Y},
            { middle.X, middle.Y },
            { middle.X, Location.MaxLocation.Y},
            
            { Location.MaxLocation.X, Location.MinLocation.Y },
            { Location.MaxLocation.X, middle.Y },
            { Location.MaxLocation.X, Location.MaxLocation.Y },
        };
    }
    
    public static TheoryData<int?, int?> AtLeastXorYIsGreaterOrLowerThanMaxOrMinBound_Data()
    {
        var theoryData = new TheoryData<int?, int?>();
        
        if (Location.MinLocation.X != int.MinValue) theoryData.Add(Location.MinLocation.X - 1, Location.MinLocation.Y);
        if (Location.MinLocation.Y != int.MinValue) theoryData.Add(Location.MinLocation.X, Location.MinLocation.Y - 1);
        if (Location.MaxLocation.X != int.MaxValue) theoryData.Add(Location.MaxLocation.X + 1, Location.MinLocation.Y);
        if (Location.MaxLocation.Y != int.MaxValue) theoryData.Add(Location.MinLocation.X, Location.MaxLocation.Y + 1);
        if(theoryData.Count is 0) theoryData.Add(null, null);
        
        return theoryData;
    }
    
    public static TheoryData<Location, Location, int> DistanceBetweenLocations_Data()
    {
        var middle = Create.MiddleLocation();
        var differenceBetweenMaxAndMinX = Location.MaxLocation.X - Location.MinLocation.X;
        var differenceBetweenMaxAndMinY = Location.MaxLocation.Y - Location.MinLocation.Y;

        return new TheoryData<Location, Location, int>
        {
            { middle, middle, 0},
            { Location.MinLocation, Location.MinLocation, 0},
            { Location.MaxLocation, Location.MaxLocation, 0},
            { Location.MinLocation, Location.MaxLocation, Math.Abs(differenceBetweenMaxAndMinX) + Math.Abs(differenceBetweenMaxAndMinY)},
            { Location.MaxLocation, Location.MinLocation, Math.Abs(differenceBetweenMaxAndMinX) + Math.Abs(differenceBetweenMaxAndMinY)},
            
            { middle, Location.Create(middle.X + 0, middle.Y + 1).Value, 1},
            { middle, Location.Create(middle.X + 1, middle.Y + 0).Value, 1},
            { middle, Location.Create(middle.X + 2, middle.Y + 0).Value, 2},
            { middle, Location.Create(middle.X + 0, middle.Y + 2).Value, 2},
            { middle, Location.Create(middle.X + 1, middle.Y + 2).Value, 3},
            { middle, Location.Create(middle.X + 2, middle.Y + 1).Value, 3},
            { middle, Location.Create(middle.X + 2, middle.Y + 2).Value, 4},
            
            { Location.Create(middle.X + 0, middle.Y + 1).Value, middle, 1},
            { Location.Create(middle.X + 1, middle.Y + 0).Value, middle, 1},
            { Location.Create(middle.X + 2, middle.Y + 0).Value, middle, 2},
            { Location.Create(middle.X + 0, middle.Y + 2).Value, middle, 2},
            { Location.Create(middle.X + 1, middle.Y + 2).Value, middle, 3},
            { Location.Create(middle.X + 2, middle.Y + 1).Value, middle, 3},
            { Location.Create(middle.X + 2, middle.Y + 2).Value, middle, 4},
            
            { middle, Location.Create(middle.X - 1, middle.Y - 1).Value, 2},
            { middle, Location.Create(middle.X - 1, middle.Y + 1).Value, 2},
            { middle, Location.Create(middle.X + 1, middle.Y - 1).Value, 2},
            { middle, Location.Create(middle.X - 1, middle.Y - 2).Value, 3},
            { middle, Location.Create(middle.X - 1, middle.Y + 2).Value, 3},
            { middle, Location.Create(middle.X + 1, middle.Y - 2).Value, 3},
        };
    }
    
    public static TheoryData<Location, Location> EqualLocations_Data()
    {
        var middle = Create.MiddleLocation();

        return new TheoryData<Location, Location>()
        {
            { Location.MinLocation, Location.MinLocation },
            { Location.MaxLocation, Location.MaxLocation },
            { middle, middle },
        };
    }

    public static TheoryData<Location, Location> NotEqualLocations_Data()
    {
        var middle = Create.MiddleLocation();
        
        return new TheoryData<Location, Location>
        {
            {
                middle.X is int.MaxValue
                    ? Location.Create(middle.X - 1, middle.Y).Value
                    : Location.Create(middle.X + 1, middle.Y).Value,
                middle
            },
            {
                middle.Y is int.MaxValue
                    ? Location.Create(middle.X, middle.Y - 1).Value
                    : Location.Create(middle.X, middle.Y + 1).Value,
                middle
            },
            {
                middle.X is int.MaxValue
                    ? Location.Create(middle.X - 1, middle.Y).Value
                    : Location.Create(middle.X + 1, middle.Y).Value,
                middle.Y is int.MaxValue
                    ? Location.Create(middle.X, middle.Y - 1).Value
                    : Location.Create(middle.X, middle.Y + 1).Value
            }
        };
    }
}