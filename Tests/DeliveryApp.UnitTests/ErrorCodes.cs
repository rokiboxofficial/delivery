using Primitives;

namespace DeliveryApp.UnitTests;

public static class ErrorCodes
{
    public static string ValueIsInvalid { get; } = GeneralErrors.ValueIsInvalid("test").Code;
}