namespace SmartX.Api.Models
{
    public readonly struct PowerReading
    {
        public int Watts { get; }

        public PowerReading(int watts)
        {
            Watts = watts;
        }

        public static PowerReading operator +(
            PowerReading left,
            PowerReading right)
        {
            return new PowerReading(left.Watts + right.Watts);
        }

        public static PowerReading operator -(
            PowerReading left,
            PowerReading right)
        {
            return new PowerReading(left.Watts - right.Watts);
        }
    }
}