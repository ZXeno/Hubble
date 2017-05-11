namespace DeviceMonitor.Infrastructure
{
    public interface IRegistryServices
    {
        void CreateStartupRegistryKey();
        void RemoveStartupRegistryKey();
        bool CheckForStartupRegistryKey();
        void ToggleRunOnStartup();
    }
}