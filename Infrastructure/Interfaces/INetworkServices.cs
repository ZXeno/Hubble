using System.Net.NetworkInformation;

namespace DeviceMonitor.Infrastructure
{
    public interface INetworkServices
    {
        PingReply PingTest(string hostname);
        bool DnsResolvesSuccessfully(string device);
        bool VerifyDeviceConnectivity(string device);
        string GetIpStatusMessage(IPStatus status);
        bool VerifyDeviceConnectivity(PingReply reply);
        bool CheckForMultipleRecords(string device);
        IPStatus Pingable(string device);
    }
}