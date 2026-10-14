using System.Collections.Concurrent;

namespace IoTmate.Common.TimeZones
{
    public static class TimeZoneHelper
    {
        private static readonly ConcurrentDictionary<string, Lazy<TimeZoneInfo?>> Cache = new();

        public static bool IsValidTimeZoneId(string? timeZoneId)
        {
            return !string.IsNullOrWhiteSpace(timeZoneId) && Resolve(timeZoneId.Trim()) != null;
        }

        public static bool TryGetLocalTime(DateTime utcNow, string? timeZoneId, out DateTime localTime)
        {
            localTime = default;
            if (string.IsNullOrWhiteSpace(timeZoneId)) return false;

            var timeZone = Resolve(timeZoneId.Trim());
            if (timeZone == null) return false;

            localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
            return true;
        }

        private static TimeZoneInfo? Resolve(string timeZoneId)
        {
            var lazy = Cache.GetOrAdd(timeZoneId, id => new Lazy<TimeZoneInfo?>(() => Find(id)));
            return lazy.Value;
        }

        private static TimeZoneInfo? Find(string timeZoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                return null;
            }
            catch (InvalidTimeZoneException)
            {
                return null;
            }
        }
    }
}