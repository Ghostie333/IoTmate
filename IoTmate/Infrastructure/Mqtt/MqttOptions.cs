namespace IoTmate.Infrastructure.Mqtt;

public sealed class MqttOptions
{
    public string Host { get; set; } = "mqtt";
    public int Port { get; set; } = 1883;

    public string TelemetryTopic { get; set; } = "devices/+/telemetry/#";
    public string CommandTopicPrefix { get; set; } = "devices/";
}
