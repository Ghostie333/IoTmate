namespace IoTmate.Common.Exceptions
{
    // Thrown when a requested resource does not exist (HTTP 404)
    public sealed class NotFoundException(string? message) : DomainException(message)
    {
        public override int StatusCode => 404;
    }
}