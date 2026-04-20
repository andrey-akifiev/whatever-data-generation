namespace Whatever.TestData.Generator.Validation.Specifications.Exceptions;

/// <summary>
/// Thrown when a specification file cannot be interpreted.
/// </summary>
public sealed class SpecificationParseException : Exception
{
    /// <inheritdoc />
    public SpecificationParseException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public SpecificationParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}