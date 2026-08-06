namespace IoTmate.Common.Exceptions
{
    /// <summary>Base class for expected, user-facing errors that map to an HTTP status code.</summary>
    public abstract class DomainException(string? message) : Exception(message)
    {

        /// <summary>The HTTP status code this error should be returned with.</summary>
        public abstract int StatusCode { get; }
    }
}