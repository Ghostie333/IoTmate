namespace IoTmate.Common.Exceptions
{
    // Thrown when the request contains semantically invalid data (HTTP 400)
    public sealed class ValidationException(string? message) : DomainException(message)
    {
        public override int StatusCode => 400;
    }
}