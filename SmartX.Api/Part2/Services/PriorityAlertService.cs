
using SmartX.Api.Part2.Models;

namespace SmartX.Api.Part2.Services
{
    public class PriorityAlertService
    {
        private readonly PriorityQueue<DeviceAlert, (int, long)>
            _alerts = new();

        private readonly List<DeviceAlert> _processed = new();

        private readonly object _lock = new();

        private long _sequence;

        public int PendingCount
        {
            get
            {
                lock (_lock)
                {
                    return _alerts.Count;
                }
            }
        }

        public void Enqueue(DeviceAlert alert)
        {
            lock (_lock)
            {
                _alerts.Enqueue(
                    alert,
                    ((int)alert.Severity, _sequence++));
            }
        }

        public DeviceAlert? ProcessNext()
        {
            lock (_lock)
            {
                if (_alerts.Count == 0)
                {
                    return null;
                }

                DeviceAlert alert = _alerts.Dequeue();

                _processed.Add(alert);

                return alert;
            }
        }

        public IReadOnlyList<DeviceAlert> GetProcessed()
        {
            lock (_lock)
            {
                return _processed.ToList();
            }
        }
    }
}
