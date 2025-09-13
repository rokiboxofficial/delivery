using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.UnitTests;

public static class ErrorCodes
{
    public static string ValueIsInvalid { get; } = GeneralErrors.ValueIsInvalid("test").Code;
    public static string ValueIsRequired { get; } = GeneralErrors.ValueIsRequired("test").Code;
    public static string OrderCannotBeStored { get; } = StoragePlace.Errors.OrderCannotBeStored().Code;
    public static string OrderCannotBeCleared { get; } = StoragePlace.Errors.OrderCannotBeCleared().Code;
    public static string NotAssignedOrderCannotBeCompleted { get; } = Order.Errors.NotAssignedOrderCannotBeCompleted().Code;
    public static string OnlyOrderWithStatusCreatedCanBeAssigned { get; } = Order.Errors.OnlyOrderWithStatusCreatedCanBeAssigned().Code;
    public static string StoragePlaceCannotBeAddedToCourier { get; } = Courier.Errors.StoragePlaceCannotBeAddedToCourier().Code;
    public static string OrderCannotBeTakenByCourier { get; } = Courier.Errors.OrderCannotBeTakenByCourier().Code;
    public static string CourierHasNoSpecifiedOrder { get; } = Courier.Errors.CourierHasNoSpecifiedOrder(Create.Order()).Code;
}