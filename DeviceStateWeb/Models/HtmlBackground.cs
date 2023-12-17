using System;
using System.Globalization;

namespace DeviceStateWeb.Models
{
    public record HtmlBackground(string Rotation, string Color1, string Color2);

    static class HtmlBackgroundHelper
    {
        private static readonly Random rnd = new Random();

        private static string GetRandomHexColor()
        {
            byte[] bytes = new byte[3];
            rnd.NextBytes(bytes);

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] < 128) bytes[i] += 128;
            }

            string hex = Convert.ToHexString(bytes);
            return $"#{hex}";
        }

        public static HtmlBackground GetRandomBackground()
        {
            double rotation = rnd.NextDouble();
            string color1 = GetRandomHexColor();
            string color2 = GetRandomHexColor();

            return new HtmlBackground(rotation.ToString(CultureInfo.InvariantCulture), color1, color2);
        }
    }
}
