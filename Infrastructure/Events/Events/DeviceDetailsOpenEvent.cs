using DeviceMonitor.Model;

namespace DeviceMonitor.Infrastructure
{
    public class DeviceDetailsOpenEvent
    {
        public bool OpenDetails { get; set; }
        public DeviceStatusModel Status { get; set; }

    }
}