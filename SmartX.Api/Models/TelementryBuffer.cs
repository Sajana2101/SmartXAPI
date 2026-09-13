namespace SmartX.Api.Models
{
    public class TelemetryBuffer<T>
    {
        private readonly
            List<TelemetryPacket<T>>
            _items =
                new();

        private readonly object
            _lock =
                new();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }

        public void Add(
            TelemetryPacket<T> packet)
        {
            lock (_lock)
            {
                _items.Add(packet);
            }
        }

        public IReadOnlyList<
            TelemetryPacket<T>>
            GetAll()
        {
            lock (_lock)
            {
                return _items
                    .OrderByDescending(
                        packet =>
                            packet.TimestampUtc)
                    .ToList();
            }
        }

        public IReadOnlyList<
            TelemetryPacket<T>>
            GetBySensor(
                Guid sensorId)
        {
            lock (_lock)
            {
                return _items
                    .Where(
                        packet =>
                            packet.SensorId ==
                            sensorId)
                    .OrderByDescending(
                        packet =>
                            packet.TimestampUtc)
                    .ToList();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _items.Clear();
            }
        }
    }
}