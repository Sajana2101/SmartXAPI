
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Part2.Services
{
    public class TelemetryQueueService
    {
        private readonly Queue<Action> _queue = new();

        private readonly ITelemetryStore _telemetryStore;

        private readonly SortedTelemetryHistoryService _history;

        private readonly object _lock = new();

        private long _processedCount;

        public TelemetryQueueService(
            ITelemetryStore telemetryStore,
            SortedTelemetryHistoryService history)
        {
            _telemetryStore = telemetryStore;
            _history = history;
        }

        public int PendingCount
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public long ProcessedCount
        {
            get
            {
                lock (_lock)
                {
                    return _processedCount;
                }
            }
        }

        public void EnqueueFloat(TelemetryPacket<float> packet)
        {
            lock (_lock)
            {
                _queue.Enqueue(() =>
                {
                    _telemetryStore.AddFloat(packet);

                    _history.Record(
                        packet.SensorId,
                        packet.Metric,
                        packet.Value,
                        packet.TimestampUtc);
                });
            }
        }

        public void EnqueueInt(TelemetryPacket<int> packet)
        {
            lock (_lock)
            {
                _queue.Enqueue(() =>
                {
                    _telemetryStore.AddInt(packet);

                    _history.Record(
                        packet.SensorId,
                        packet.Metric,
                        packet.Value,
                        packet.TimestampUtc);
                });
            }
        }

        public void EnqueueBool(TelemetryPacket<bool> packet)
        {
            lock (_lock)
            {
                _queue.Enqueue(() =>
                {
                    _telemetryStore.AddBool(packet);

                    _history.Record(
                        packet.SensorId,
                        packet.Metric,
                        packet.Value,
                        packet.TimestampUtc);
                });
            }
        }

        public int ProcessPending(int maximum = 100)
        {
            if (maximum < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximum));
            }

            lock (_lock)
            {
                int processed = 0;

                while (_queue.Count > 0 && processed < maximum)
                {
                    Action operation = _queue.Peek();

                    operation();

                    _queue.Dequeue();

                    processed++;
                    _processedCount++;
                }

                return processed;
            }
        }
    }
}
