namespace Primitives.Exceptions;

public sealed class DomainInvariantException(string message) : Exception(message);