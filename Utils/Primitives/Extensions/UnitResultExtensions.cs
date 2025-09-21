using Primitives.Exceptions;

namespace Primitives.Extensions;

using CSharpFunctionalExtensions;

public static class UnitResultExtensions
{
    /// <summary>
    /// Turns UnitResult.Failure into exception (fail-fast).
    /// Used in places where error is not possible under the contract.
    /// </summary>
    public static void ThrowIfFailure<TError>(this UnitResult<TError> result)
    {
        if (result.IsFailure) throw new DomainInvariantException(result.Error?.ToString() ?? "Unknown error");
    }
}