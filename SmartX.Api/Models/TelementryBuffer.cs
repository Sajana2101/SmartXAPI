namespace SmartX.Api.Models
{
    public class TelemetryBuffer<T>
    {
        private readonly List<TelemetryPacket<T>> _items = new();

        public int Count => _items.Count;

        public void Add(TelemetryPacket<T> packet)
        {
            _items.Add(packet);
        }

        public IReadOnlyList<TelemetryPacket<T>> GetAll()
        {
            return _items.AsReadOnly();
        }

        public IReadOnlyList<TelemetryPacket<T>> GetBySensor(Guid sensorId)
        {
            return _items
                .Where(packet => packet.SensorId == sensorId)
                .OrderByDescending(packet => packet.TimestampUtc)
                .ToList()
                .AsReadOnly();
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}