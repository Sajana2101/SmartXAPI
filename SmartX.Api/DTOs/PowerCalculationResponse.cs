namespace SmartX.Api.DTOs
{
    public class PowerCalculationResponse
    {
        public int FirstReading { get; set; }

        public int SecondReading { get; set; }

        public int CombinedPower { get; set; }

        public int Delta { get; set; }
    }
}