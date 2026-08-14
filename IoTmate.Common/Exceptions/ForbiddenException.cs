namespace IoTmate.Common.Exceptions
{
    // Thrown when the caller is not allowed to access or modify a resource (HTTP 403)
    public sealed class ForbiddenException(string? message) : DomainException(message)
    {
        public override int StatusCode => 403;
    }
}