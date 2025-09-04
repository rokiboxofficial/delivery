using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Primitives;

namespace DeliveryApp.UnitTests;

public static class ErrorCodes
{
    public static string ValueIsInvalid { get; } = GeneralErrors.ValueIsInvalid("test").Code;
    public static string ValueIsRequired { get; } = GeneralErrors.ValueIsRequired("test").Code;
    public static string OrderCannotBeStored { get; } = StoragePlace.Errors.OrderCannotBeStored().Code;
    public static string OrderCannotBeCleared { get; } = StoragePlace.Errors.OrderCannotBeCleared().Code;
}