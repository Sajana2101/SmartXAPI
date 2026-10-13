
using SmartX.Api.Part2.Models;

namespace SmartX.Api.Part2.Services
{
    public class DeviceSearchHistoryService
    {
        private const int MaximumEntries = 2000;

        private readonly List<DeviceSearchEvent> _searches = new();

        private readonly object _lock = new();

        public DeviceSearchEvent Record(
            string query,
            Guid? sensorId)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException(
                    "A search query is required.");
            }

            string normalized = query.Trim();

            if (normalized.Length > 150)
            {
                throw new ArgumentException(
                    "Search queries cannot exceed 150 characters.");
            }

            DeviceSearchEvent entry = new()
            {
                Query = normalized,
                SensorId = sensorId,
                SearchedAtUtc = DateTime.UtcNow
            };

            lock (_lock)
            {
                _searches.Add(entry);

                if (_searches.Count > MaximumEntries)
                {
                    _searches.RemoveAt(0);
                }
            }

            return Copy(entry);
        }

        public IReadOnlyList<DeviceSearchEvent> GetHistory()
        {
            lock (_lock)
            {
                return _searches
                    .OrderByDescending(x => x.SearchedAtUtc)
                    .Select(Copy)
                    .ToList();
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _searches.Count;
                }
            }
        }

        private static DeviceSearchEvent Copy(
            DeviceSearchEvent search)
        {
            return new DeviceSearchEvent
            {
                Id = search.Id,
                Query = search.Query,
                SensorId = search.SensorId,
                SearchedAtUtc = search.SearchedAtUtc
            };
        }
    }
}
