using System;
using System.Collections.ObjectModel;
using DeviceMonitor.Model;

namespace DeviceMonitor.Infrastructure
{
    public class TimedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public ObservableCollection<DeviceStatusModel> DeviceStatusList;
    }
}