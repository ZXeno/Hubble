using System.Management;

namespace DeviceMonitor.Infrastructure
{
    public interface IWmiServices
    {
        ManagementScope ConnectToRemoteWmi(string hostname, ConnectionOptions options);
        string QueryLoggedOnUser(string device);
    }
}