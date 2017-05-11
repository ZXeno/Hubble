using System.Net.NetworkInformation;

namespace DeviceMonitor.Infrastructure
{
    public interface INetworkServices
    {
        PingReply PingTest(string hostname);
        bool DnsResolvesSuccessfully(string device);
        bool VerifyDeviceConnectivity(string device);
        string GetIpStatusMessage(IPStatus status);
        IPStatus Pingable(string device);
    }
}