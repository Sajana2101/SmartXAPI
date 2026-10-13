
using System.Globalization;
using SmartX.Api.Part2.Models;

namespace SmartX.Api.Part2.Services
{
    public class SortedTelemetryHistoryService
    {
        private readonly SortedDictionary<
            (DateTime Timestamp, long Sequence),
            HistoricalTelemetryEntry> _history = new();

        private readonly object _lock = new();

        private long _sequence;

        public void Record<T>(
            Guid sensorId,
            string metric,
            T value,
            DateTime timestampUtc)
        {
            HistoricalTelemetryEntry entry = new()
            {
                SensorId = sensorId,
                Metric = metric,
                Value = Convert.ToString(
                    value, CultureInfo.InvariantCulture) ?? string.Empty,
                ValueType = typeof(T).Name,
                TimestampUtc = timestampUtc
            };

            lock (_lock)
            {
                _history.Add(
                    (timestampUtc, _sequence++),
                    entry);
            }
        }

        public IReadOnlyList<HistoricalTelemetryEntry> GetHistory(
            Guid? sensorId = null)
        {
            lock (_lock)
            {
                IEnumerable<HistoricalTelemetryEntry> entries =
                    _history.Values;

                if (sensorId.HasValue)
                {
                    entries = entries.Where(
                        entry => entry.SensorId == sensorId.Value);
                }

                return entries.Reverse().ToList();
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _history.Count;
                }
            }
        }
    }
}
