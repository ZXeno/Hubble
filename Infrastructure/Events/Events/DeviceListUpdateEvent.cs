using System;
using System.Collections.Generic;

namespace DeviceMonitor.Infrastructure
{
    public class DeviceListUpdateEvent
    {
        public List<string> DeviceList { get; set; }
        public bool DeviceListIsUpdated { get; set; }
    }
}