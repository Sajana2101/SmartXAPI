
using System.Globalization;
using SmartX.Api.Part2.Models;
using SmartX.Api.Part3.Structures;

namespace SmartX.Api.Part2.Services;

public sealed record TelemetryAvlStatistics(
    int TotalReadings,
    int IndexedSensors,
    int Height,
    long Rotations,
    bool Balanced
);

public class SortedTelemetryHistoryService
{
    private readonly AvlTree<HistoricalTelemetryEntry> _allHistory =
        new();

    private readonly Dictionary<Guid, AvlTree<HistoricalTelemetryEntry>>
        _sensorHistory = new();

    private readonly object _lock = new();

    public void Record<T>(
        Guid sensorId,
        string metric,
        T value,
        DateTime timestampUtc)
    {
        var normalizedTimestamp = NormalizeUtc(timestampUtc);

        var entry = new HistoricalTelemetryEntry
        {
            SensorId = sensorId,
            Metric = metric,
            Value = Convert.ToString(
                value, CultureInfo.InvariantCulture) ?? string.Empty,
            ValueType = typeof(T).Name,
            TimestampUtc = normalizedTimestamp
        };

        lock (_lock)
        {
            _allHistory.Insert(normalizedTimestamp, entry);

            if (!_sensorHistory.TryGetValue(
                sensorId, out var sensorTree))
            {
                sensorTree = new AvlTree<HistoricalTelemetryEntry>();
                _sensorHistory.Add(sensorId, sensorTree);
            }

            sensorTree.Insert(normalizedTimestamp, entry);
        }
    }

    public IReadOnlyList<HistoricalTelemetryEntry> GetHistory(
        Guid? sensorId = null)
    {
        lock (_lock)
        {
            if (sensorId.HasValue)
            {
                if (!_sensorHistory.TryGetValue(
                    sensorId.Value, out var sensorTree))
                {
                    return Array.Empty<HistoricalTelemetryEntry>();
                }

                return sensorTree.GetRange(
                    DateTime.MinValue,
                    DateTime.MaxValue);
            }

            return _allHistory.GetRange(
                DateTime.MinValue,
                DateTime.MaxValue);
        }
    }

    public IReadOnlyList<HistoricalTelemetryEntry> GetRange(
        DateTime fromUtc,
        DateTime toUtc,
        Guid? sensorId = null,
        int limit = 500)
    {
        var from = NormalizeUtc(fromUtc);
        var to = NormalizeUtc(toUtc);

        if (from > to)
        {
            throw new ArgumentException(
                "The start date cannot be after the end date.");
        }

        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit));
        }

        lock (_lock)
        {
            if (sensorId.HasValue)
            {
                if (!_sensorHistory.TryGetValue(
                    sensorId.Value, out var sensorTree))
                {
                    return Array.Empty<HistoricalTelemetryEntry>();
                }

                return sensorTree.GetRange(
                    from, to, limit);
            }

            return _allHistory.GetRange(
                from, to, limit);
        }
    }

    public TelemetryAvlStatistics GetStatistics()
    {
        lock (_lock)
        {
            return new TelemetryAvlStatistics(
                _allHistory.Count,
                _sensorHistory.Count,
                _allHistory.Height,
                _allHistory.Rotations,
                _allHistory.IsBalanced());
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _allHistory.Count;
            }
        }
    }

    private static DateTime NormalizeUtc(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(
                timestamp, DateTimeKind.Utc)
        };
    }
}
