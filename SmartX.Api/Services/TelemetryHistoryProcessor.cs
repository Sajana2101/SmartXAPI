namespace SmartX.Api.Services
{
    public class TelemetryHistoryProcessor
    {
        public List<T> ConvertBatchesToList<T>(T[][] batches)
        {
            List<T> readings = new();

            foreach (T[] batch in batches)
            {
                foreach (T reading in batch)
                {
                    readings.Add(reading);
                }
            }

            return readings;
        }
    }
}