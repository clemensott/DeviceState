using WaterpumpWeb.Models;

namespace WaterpumpWeb.Services.ValueTransformers
{
    public interface IValueTransformer
    {
        TransformedValue Transform(string name, double value);
    }
}
