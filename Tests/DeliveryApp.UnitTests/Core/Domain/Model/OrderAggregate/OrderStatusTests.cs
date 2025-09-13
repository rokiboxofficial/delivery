using System.Reflection;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Model.OrderAggregate;

public sealed class OrderStatusTests
{
    [Fact]
    public void WhenCheckingIsValueObjectAssignableFromOrderStatus_ThenResultShouldBeTrue()
    {
        // Act.
        var isAssignableFromOrderStatus = typeof(ValueObject).IsAssignableFrom(typeof(OrderStatus));

        // Assert.
        isAssignableFromOrderStatus.Should().BeTrue();
    }
    
    [Fact]
    public void WhenGettingConstructors_ThenEachConstructorShouldBePrivate()
    {
        // Act.
        var constructors = typeof(OrderStatus).GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Assert.
        constructors.Should().AllSatisfy(x => x.IsPrivate.Should().BeTrue());
    }
    
    [Fact]
    public void WhenGettingPublicStaticProperties_ThenEachPropertyTypeShouldBeOrderStatusAndPropertyValueOrderStatusNameShouldBeNameOfProperty()
    {
        // Act.
        var publicStaticProperties = typeof(OrderStatus)
            .GetProperties(BindingFlags.Public | BindingFlags.Static);

        // Assert.
        publicStaticProperties.Should().AllSatisfy(x =>
        {
            x.PropertyType.Should().Be<OrderStatus>();
            ((OrderStatus)x.GetValue(null))!.Name.Should().Be(x.Name);
        });
    }
}