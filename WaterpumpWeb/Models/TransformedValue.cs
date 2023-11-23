using System.Xml.Linq;

namespace WaterpumpWeb.Models
{
    public readonly struct TransformedValue
    {
        public bool HasValue { get; init; }

        public double Value { get; init; }

        public CompareType Type { get; init; }

        public string Name { get; init; }

        public static TransformedValue Empty(string name)
        {
            return new TransformedValue()
            {
                HasValue = false,
                Name = name,
            };
        }

        public static TransformedValue FromValue(string name, double value)
        {
            return new TransformedValue()
            {
                HasValue = true,
                Value = value,
                Type = CompareType.Equal,
                Name = name,
            };
        }

        public static TransformedValue FromSmallerThan(string name, double value)
        {
            return new TransformedValue()
            {
                HasValue = true,
                Value = value,
                Type = CompareType.SmallerThan,
                Name = name,
            };
        }

        public static TransformedValue FromGreaterThan(string name, double value)
        {
            return new TransformedValue()
            {
                HasValue = true,
                Value = value,
                Type = CompareType.GreaterThan,
                Name = name,
            };
        }
    }
}
