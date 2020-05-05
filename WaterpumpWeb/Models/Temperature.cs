namespace WaterpumpWeb.Models
{
    public struct Temperature
    {
        public static Temperature Empty { get; } 
            = new Temperature() { HasValue = false, Value = 0, Type = CompareType.Equal };

        public bool HasValue { get; private set; }

        public double Value { get; private set; }

        public CompareType Type { get; private set; }

        public enum CompareType { Equal, SmallerThan, GreaterThan }

        public static Temperature FromValue(double value)
        {
            return new Temperature()
            {
                HasValue = true,
                Value = value,
                Type = CompareType.Equal,
            };
        }

        public static Temperature FromSmallerThan(double value)
        {
            return new Temperature()
            {
                HasValue = true,
                Value = value,
                Type = CompareType.SmallerThan,
            };
        }

        public static Temperature FromGreaterThan(double value)
        {
            return new Temperature()
            {
                HasValue = true,
                Value = value,
                Type = CompareType.GreaterThan,
            };
        }
    }
}
