namespace WaterpumpWeb.Services
{
    public interface IAppConfiguration
    {
        string DatabaseConnectionString { get; }

        string DefaultDeviceId { get; }
    }
}
