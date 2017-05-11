using System;
using System.Management;

namespace DeviceMonitor.Infrastructure
{
    public class WmiServices : IWmiServices
    {
        private static string RootNamespace => "\\root\\cimv2";

        public ManagementScope ConnectToRemoteWmi(string hostname, ConnectionOptions options)
        {
            try
            {
                var wmiscope = new ManagementScope($"\\\\{hostname}{RootNamespace}", options);
                wmiscope.Connect();
                return wmiscope;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string QueryLoggedOnUser(string device)
        {
            var result = "";
            var remote = ConnectToRemoteWmi(device, new ConnectionOptions());
            if (remote == null) return result;

            var query = new ObjectQuery("SELECT username FROM Win32_ComputerSystem");

            var searcher = new ManagementObjectSearcher(remote, query);
            var queryCollection = searcher.Get();

            foreach (var resultobject in queryCollection)
            {
                result = $"{resultobject["username"]}";
            }

            return result;
        }
    }
}