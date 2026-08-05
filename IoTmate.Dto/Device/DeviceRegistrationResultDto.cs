namespace IoTmate.Dto.Device;
using IoTmate.Model.Entities;

public record DeviceRegistrationResult
{
    public required Device Device { get; set; }
    public required string RawSecretToken { get; set; }
}
