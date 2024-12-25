namespace FluentInjections;

/// <summary>
/// Represents an interface for objects that can be validated.
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// Validates the current state of the object.
    /// </summary>
    /// <exception cref="ValidationException">Thrown when the object is in an invalid state.</exception>
    void Validate();
}
