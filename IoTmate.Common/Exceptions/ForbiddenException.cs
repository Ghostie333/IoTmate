namespace IoTmate.Common.Exceptions
{
    /// <summary>Thrown when the caller is not allowed to access or modify a resource (HTTP 403).</summary>
    public sealed class ForbiddenException(string? message) : DomainException(message)
    {
        public override int StatusCode => 403;
    }
}