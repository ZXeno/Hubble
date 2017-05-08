using System;
using System.Net;
using System.Net.NetworkInformation;

namespace DeviceMonitor.Infrastructure
{
    public static class NetworkServices
    {
        /// <summary>
        /// Ping test for single machine. 
        /// 
        /// If exception is caught, returns null.
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static PingReply PingTest(string hostname)
        {
            try
            {
                return new Ping().Send(hostname, 3000);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool DnsResolvesSuccessfully(string device)
        {
            bool didResolve;
            IPHostEntry hostentry;
            try
            {
                hostentry = Dns.GetHostEntry(device);
                didResolve = true;
            }
            catch (Exception)
            {
                didResolve = false;
            }

            return didResolve;
        }

        public static bool VerifyDeviceConnectivity(string device)
        {
            try
            {
                return Pingable(device) == IPStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetIpStatusMessage(IPStatus status)
        {
            switch (status)
            {
                case IPStatus.Success:
                    return "successfully connected.";

                case IPStatus.TimedOut:
                    return "failed to connect: Timeout.";

                case IPStatus.BadDestination:
                    return "failed to connect: Bad Destination.";

                case IPStatus.BadHeader:
                    return "failed to connect: Bad Header.";

                case IPStatus.BadOption:
                    return "failed to connect: Bad Option.";

                case IPStatus.BadRoute:
                    return "failed to connect: Bad Route.";

                case IPStatus.DestinationHostUnreachable:
                    return "failed to connect: Destination Host Unreachable.";

                case IPStatus.DestinationNetworkUnreachable:
                    return "failed to connect: Destination Network Unreachable.";

                case IPStatus.DestinationPortUnreachable:
                    return "failed to connect: Destination Port Unreachable.";

                case IPStatus.DestinationProtocolUnreachable:
                    return "failed to connect: Destination Network Unreachable.";

                case IPStatus.DestinationScopeMismatch:
                    return "failed to connect: Destination Scope Mismatch.";

                case IPStatus.HardwareError:
                    return "failed to connect: Hardware Error.";

                case IPStatus.IcmpError:
                    return "failed to connect: ICMP Error.";

                case IPStatus.NoResources:
                    return "failed to connect: No Resources.";

                case IPStatus.TimeExceeded:
                    return "failed to connect: Time Exceeded.";

                case IPStatus.TtlExpired:
                    return "failed to connect: TTL Expired.";

                case IPStatus.PacketTooBig:
                    return "failed to connect: Packet Too Big.";

                case IPStatus.SourceQuench:
                    return "failed to connect: Source Quench.";

                case IPStatus.TtlReassemblyTimeExceeded:
                    return "failed to connect: TTL Reassembly Time Exceeded.";

                case IPStatus.ParameterProblem:
                    return "failed to connect: IPSTATUS.PARAMETERPROBLEM .";

                case IPStatus.UnrecognizedNextHeader:
                    return "failed to connect: Unrecognized Next Header.";

                default:
                    return "failed to connect: Unknown reason.";
            }
        }

        public static IPStatus Pingable(string device)
        {
            var reply = new Ping().Send(device, 3000);

            return reply?.Status ?? IPStatus.Unknown;
        }
    }
}
