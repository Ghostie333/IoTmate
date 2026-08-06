namespace IoTmate.Common.Exceptions
{
    /// <summary>Thrown when a requested resource does not exist (HTTP 404).</summary>
    public sealed class NotFoundException(string? message) : DomainException(message)
    {
        public override int StatusCode => 404;
    }
}