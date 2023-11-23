using System.Linq;
using WaterpumpWeb.Models;

namespace WaterpumpWeb
{
    readonly struct TemperatureMapPoint
    {
        public double Measure { get; }

        public double Temperature { get; }

        public TemperatureMapPoint(double measure, double temperature)
        {
            Measure = measure;
            Temperature = temperature;
        }
    }

    class TemperatureConverter
    {
        private static TemperatureMapPoint[] tempPoints;

        private static TemperatureMapPoint? IsTempPointLine(string line)
        {
            string[] parts = line.Split('=');

            return parts.Length == 2 &&
                double.TryParse(parts[0], out double raw) &&
                double.TryParse(parts[1], out double temp) ?
                new TemperatureMapPoint(raw, temp) : null;
        }

        private static TemperatureMapPoint[] GetTemperatureMapPoints()
        {
            if (tempPoints == null)
            {
                string[] lines = System.IO.File.ReadAllLines(@"TempPoints.txt");
                tempPoints = lines.Select(IsTempPointLine)
                    .OfType<TemperatureMapPoint>()
                    .OrderBy(t => t.Measure)
                    .ToArray();
            }
            return tempPoints;
        }

        public static TransformedValue Convert(string name, double measure)
        {
            try
            {
                TemperatureMapPoint[] tempPoints = GetTemperatureMapPoints();

                if (tempPoints.Select(t => t.Measure).Distinct().Count() < 2) return TransformedValue.Empty(name);

                if (!tempPoints.Any(t => t.Measure <= measure)) return TransformedValue.FromSmallerThan(name, tempPoints.Min(t => t.Temperature));
                if (!tempPoints.Any(t => t.Measure > measure)) return TransformedValue.FromGreaterThan(name, tempPoints.Max(t => t.Temperature));

                TemperatureMapPoint beforePoint = tempPoints.Last(t => t.Measure <= measure);
                TemperatureMapPoint afterPoint = tempPoints.First(t => t.Measure > measure);
                double relMeasure = (measure - beforePoint.Measure) / (afterPoint.Measure - beforePoint.Measure);
                double resultValue = (afterPoint.Temperature - beforePoint.Temperature) * relMeasure + beforePoint.Temperature;

                return TransformedValue.FromValue(name, resultValue);
            }
            catch
            {
                return TransformedValue.Empty(name);
            }
        }
    }
}
