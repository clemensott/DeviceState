using DeviceStateWeb.Models;

namespace DeviceStateWeb.Services.ValueTransformers
{
    public interface IValueTransformer
    {
        TransformedValue Transform(string name, double value);
    }
}
