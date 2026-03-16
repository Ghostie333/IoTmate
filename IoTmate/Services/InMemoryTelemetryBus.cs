using System.Collections.Concurrent;
using IoTmate.Contracts.Telemetry;

namespace IoTmate.Services;

public sealed class InMemoryTelemetryBus : ITelemetryBus
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<TelemetryPointDto>> _byDevice = new(StringComparer.OrdinalIgnoreCase);

    public void Publish(TelemetryPointDto point)
    {
        var queue = _byDevice.GetOrAdd(point.DeviceId, _ => new ConcurrentQueue<TelemetryPointDto>());
        queue.Enqueue(point);

        while (queue.Count > 200)
        {
            queue.TryDequeue(out _);
        }
    }

    public IReadOnlyList<TelemetryPointDto> GetLatest(string deviceId, int take)
    {
        if (!_byDevice.TryGetValue(deviceId, out var queue))
        {
            return Array.Empty<TelemetryPointDto>();
        }

        var snapshot = queue.ToArray();
        return snapshot
            .OrderByDescending(p => p.TimestampUtc)
            .Take(Math.Clamp(take, 1, 200))
            .ToArray();
    }
}
