namespace IoTmate.Common.Exceptions
{
    // Thrown when the operation conflicts with the current state of the resource (HTTP 409)
    public sealed class ConflictException(string? message) : DomainException(message)
    {
        public override int StatusCode => 409;
    }
}