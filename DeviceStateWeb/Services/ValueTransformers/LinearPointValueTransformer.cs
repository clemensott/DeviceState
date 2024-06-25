using System.Globalization;
using System.IO;
using System.Linq;
using DeviceStateWeb.Models;

namespace DeviceStateWeb.Services.ValueTransformers
{
    record MapPoint(double Value, double Result);

    class LinearPointValueTransformer : IValueTransformer
    {
        private readonly string filePath;
        private MapPoint[] points;

        public LinearPointValueTransformer(IAppConfiguration appConfiguration)
        {
            filePath = appConfiguration.LinearPointsMapPath;
        }

        private static MapPoint IsPointLine(string line)
        {
            string[] parts = line.Split('=');

            NumberStyles numberStyles = NumberStyles.Integer | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            return parts.Length == 2 &&
                double.TryParse(parts[0], numberStyles, CultureInfo.InvariantCulture, out double raw) &&
                double.TryParse(parts[1], numberStyles, CultureInfo.InvariantCulture, out double temp) ?
                new MapPoint(raw, temp) : null;
        }

        private MapPoint[] GetMapPoints()
        {
            if (points == null)
            {
                if (!File.Exists(filePath)) return new MapPoint[0];

                string[] lines = File.ReadAllLines(filePath);
                points = lines.Select(IsPointLine)
                    .OfType<MapPoint>()
                    .OrderBy(t => t.Value)
                    .ToArray();
            }

            return points;
        }

        public TransformedValue Transform(string name, double value)
        {
            MapPoint[] tempPoints = GetMapPoints();

            if (tempPoints.Select(t => t.Value).Distinct().Count() < 2) return TransformedValue.Empty(name);

            if (!tempPoints.Any(t => t.Value <= value))
            {
                return TransformedValue.FromSmallerThan(name, tempPoints.Min(t => t.Result));
            }
            if (!tempPoints.Any(t => t.Value > value))
            {
                return TransformedValue.FromGreaterThan(name, tempPoints.Max(t => t.Result));
            }

            MapPoint beforePoint = tempPoints.Last(t => t.Value <= value);
            MapPoint afterPoint = tempPoints.First(t => t.Value > value);
            double relMeasure = (value - beforePoint.Value) / (afterPoint.Value - beforePoint.Value);
            double resultValue = (afterPoint.Value - beforePoint.Value) * relMeasure + beforePoint.Result;

            return TransformedValue.FromValue(name, resultValue);
        }
    }
}
