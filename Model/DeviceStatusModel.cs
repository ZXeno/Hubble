namespace DeviceMonitor.Model
{
    public class DeviceStatusModel
    {
        public string Device { get; set; }
        public string IpAddress { get; set; }
        public string LastSeen { get; set; }
        public bool Online { get; set; }
        public string LoggedOnUser { get; set; }
        public string Tag { get; set; }

        public string OnlineString => !Online ? "Offline" : "Online";
    }
}