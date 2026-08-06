namespace IoTmate.Common.Exceptions
{
    /// <summary>Thrown when the operation conflicts with the current state of the resource (HTTP 409).</summary>
    public sealed class ConflictException(string? message) : DomainException(message)
    {
        public override int StatusCode => 409;
    }
}