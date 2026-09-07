using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public interface ITelemetryStore
    {
        void AddFloat(TelemetryPacket<float> packet);

        void AddInt(TelemetryPacket<int> packet);

        void AddBool(TelemetryPacket<bool> packet);

        IReadOnlyList<TelemetryPacket<float>> GetFloatPackets();

        IReadOnlyList<TelemetryPacket<int>> GetIntPackets();

        IReadOnlyList<TelemetryPacket<bool>> GetBoolPackets();

        IReadOnlyList<TelemetryPacket<float>> GetFloatPackets(Guid sensorId);

        IReadOnlyList<TelemetryPacket<int>> GetIntPackets(Guid sensorId);

        IReadOnlyList<TelemetryPacket<bool>> GetBoolPackets(Guid sensorId);
    }
}