
namespace SmartX.Api.Part2.Models
{
    public class DeviceSearchEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Query { get; set; } = string.Empty;

        public Guid? SensorId { get; set; }

        public DateTime SearchedAtUtc { get; set; }
            = DateTime.UtcNow;
    }

    public class RecordDeviceSearchRequest
    {
        public string Query { get; set; } = string.Empty;

        public Guid? SensorId { get; set; }
    }

    public class DeviceBehaviorSummary
    {
        public Guid SensorId { get; set; }

        public int SearchCount { get; set; }

        public int CommandCount { get; set; }

        public int RecentTelemetryCount { get; set; }

        public int ConsecutiveAnomalousReadings { get; set; }

        public int SearchCommandCorrelations { get; set; }

        public DateTime? LastInteractionUtc { get; set; }
    }

    public class PredictiveRecommendation
    {
        public string Id { get; set; } = string.Empty;

        public Guid SensorId { get; set; }

        public string DeviceIdentifier { get; set; }
            = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Explanation { get; set; } = string.Empty;

        public string SuggestedCommand { get; set; }
            = string.Empty;

        public int Score { get; set; }

        public string Priority { get; set; } = string.Empty;

        public string Evidence { get; set; } = string.Empty;

        public DateTime GeneratedAtUtc { get; set; }
    }

    public class PredictiveInsightsResponse
    {
        public DateTime GeneratedAtUtc { get; set; }

        public int AnalysedSensorCount { get; set; }

        public int RecordedSearchCount { get; set; }

        public int RecordedCommandCount { get; set; }

        public List<PredictiveRecommendation> Recommendations
        {
            get; set;
        } = new();

        public List<DeviceBehaviorSummary> Behavior
        {
            get; set;
        } = new();
    }
}
