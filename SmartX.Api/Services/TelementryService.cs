using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryStore : ITelemetryStore
    {
        private readonly TelemetryBuffer<float> _floatPackets = new();
        private readonly TelemetryBuffer<int> _intPackets = new();
        private readonly TelemetryBuffer<bool> _boolPackets = new();

        public void AddFloat(TelemetryPacket<float> packet)
        {
            _floatPackets.Add(packet);
        }

        public void AddInt(TelemetryPacket<int> packet)
        {
            _intPackets.Add(packet);
        }

        public void AddBool(TelemetryPacket<bool> packet)
        {
            _boolPackets.Add(packet);
        }

        public IReadOnlyList<TelemetryPacket<float>> GetFloatPackets()
        {
            return _floatPackets.GetAll();
        }

        public IReadOnlyList<TelemetryPacket<int>> GetIntPackets()
        {
            return _intPackets.GetAll();
        }

        public IReadOnlyList<TelemetryPacket<bool>> GetBoolPackets()
        {
            return _boolPackets.GetAll();
        }

        public IReadOnlyList<TelemetryPacket<float>> GetFloatPackets(
            Guid sensorId)
        {
            return _floatPackets.GetBySensor(sensorId);
        }

        public IReadOnlyList<TelemetryPacket<int>> GetIntPackets(
            Guid sensorId)
        {
            return _intPackets.GetBySensor(sensorId);
        }

        public IReadOnlyList<TelemetryPacket<bool>> GetBoolPackets(
            Guid sensorId)
        {
            return _boolPackets.GetBySensor(sensorId);
        }
    }
}