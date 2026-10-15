
using SmartX.Api.Part2.Models;

namespace SmartX.Api.Part2.Services
{
    public class DeviceIssueTracker
    {
        private readonly HashSet<(Guid, DeviceIssueType)>
            _activeKeys = new();

        private readonly HashSet<(Guid, DeviceIssueType)>
            _manualKeys = new();

        private readonly HashSet<(Guid, DeviceIssueType)>
            _automaticKeys = new();

        private readonly Dictionary<
            (Guid, DeviceIssueType),
            DeviceIssue> _issues = new();

        private readonly object _lock = new();

        public bool Report(Guid sensorId, DeviceIssueType type)
        {
            lock (_lock)
            {
                var key = (sensorId, type);

                _manualKeys.Add(key);

                if (!_activeKeys.Add(key))
                {
                    return false;
                }

                _issues.Add(key, new DeviceIssue
                {
                    SensorId = sensorId,
                    Type = type
                });

                return true;
            }
        }

        public bool ReportAutomatic(
            Guid sensorId,
            DeviceIssueType type)
        {
            lock (_lock)
            {
                var key = (sensorId, type);

                _automaticKeys.Add(key);

                if (!_activeKeys.Add(key))
                {
                    return false;
                }

                _issues.Add(key, new DeviceIssue
                {
                    SensorId = sensorId,
                    Type = type
                });

                return true;
            }
        }

        public bool Resolve(Guid sensorId, DeviceIssueType type)
        {
            lock (_lock)
            {
                var key = (sensorId, type);

                _manualKeys.Remove(key);
                _automaticKeys.Remove(key);

                if (!_activeKeys.Remove(key))
                {
                    return false;
                }

                _issues.Remove(key);

                return true;
            }
        }

        public bool ResolveAutomatic(
            Guid sensorId,
            DeviceIssueType type)
        {
            lock (_lock)
            {
                var key = (sensorId, type);

                if (!_automaticKeys.Remove(key))
                {
                    return false;
                }

                if (_manualKeys.Contains(key))
                {
                    return false;
                }

                if (!_activeKeys.Remove(key))
                {
                    return false;
                }

                _issues.Remove(key);

                return true;
            }
        }

        public IReadOnlyList<DeviceIssue> GetActive()
        {
            lock (_lock)
            {
                return _issues.Values
                    .OrderBy(issue => issue.FirstDetectedAtUtc)
                    .ToList();
            }
        }

        public int ActiveCount
        {
            get
            {
                lock (_lock)
                {
                    return _activeKeys.Count;
                }
            }
        }
    }
}
