
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Part2.Services
{
    public class TelemetryQueueService
    {
        private readonly Queue<Action<ITelemetryStore>> _queue = new();

        private readonly ITelemetryStore _telemetryStore;

        private readonly object _lock = new();

        private long _processedCount;

        public TelemetryQueueService(
            ITelemetryStore telemetryStore)
        {
            _telemetryStore = telemetryStore;
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
                _queue.Enqueue(store => store.AddFloat(packet));
            }
        }

        public void EnqueueInt(TelemetryPacket<int> packet)
        {
            lock (_lock)
            {
                _queue.Enqueue(store => store.AddInt(packet));
            }
        }

        public void EnqueueBool(TelemetryPacket<bool> packet)
        {
            lock (_lock)
            {
                _queue.Enqueue(store => store.AddBool(packet));
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
                    Action<ITelemetryStore> operation = _queue.Peek();

                    operation(_telemetryStore);

                    _queue.Dequeue();

                    processed++;
                    _processedCount++;
                }

                return processed;
            }
        }
    }
}
