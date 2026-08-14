namespace IoTmate.Common.Exceptions
{
    // Base class for expected errors that map to an HTTP status code
    public abstract class DomainException(string? message) : Exception(message)
    {
        public abstract int StatusCode { get; }
    }
}