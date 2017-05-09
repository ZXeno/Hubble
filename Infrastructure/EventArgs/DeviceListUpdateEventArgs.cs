using System;
using System.Collections.Generic;

namespace DeviceMonitor.Infrastructure
{
    public class DeviceListUpdateEventArgs : EventArgs
    {
        public List<string> DeviceList { get; set; }
    }
}