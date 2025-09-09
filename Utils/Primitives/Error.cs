using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;

namespace Primitives;

public sealed class Error : ValueObject
{
    private const string Separator = "||";
    private const int RequiredPropertiesCount = 2;

    [ExcludeFromCodeCoverage]
    private Error()
    {
        
    }

    public Error(string code, string message, Error innerError = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(code, nameof(code));
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        
        Code = code;
        Message = message;
        InnerError = innerError;
    }

    /// <summary>
    ///     Код ошибки
    /// </summary>
    public string Code { get; }

    /// <summary>
    ///     Текст ошибки
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Вложенная ошибка
    /// </summary>
    public Error InnerError { get; }

    public string Serialize()
    {
        return InnerError is null
            ? $"{Code}{Separator}{Message}"
            : string.Join(Separator, Code, Message, InnerError.Serialize());
    }

    public static Error Deserialize(string serialized)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serialized, nameof(serialized));

        var data = serialized.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        return data.Length == 0 || data.Length % RequiredPropertiesCount != 0
            ? throw new FormatException($"{nameof(serialized)} parts count is 0 or not divisible by {RequiredPropertiesCount}: '{serialized}'")
            : CreateErrorRecursively(data, 0);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        var error = this;
        
        while (true)
        {
            if (error is null) yield break;

            yield return error.Code;
            yield return error.Message;

            error = error.InnerError;
        }
    }

    private static Error CreateErrorRecursively(string[] data, int index)
    {
        return index >= data.Length
            ? null
            : new Error(data[index], data[index + 1], CreateErrorRecursively(data, index + 2));
    }
}