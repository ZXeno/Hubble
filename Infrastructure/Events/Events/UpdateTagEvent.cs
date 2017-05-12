namespace DeviceMonitor.Infrastructure
{
    public class UpdateTagEvent
    {
        public string Device { get; set; }
        public string NewTag { get; set; }
        public bool OpenPopup { get; set; }
    }
}