namespace DeviceMonitor.Infrastructure
{
    public class ReportSaveRequestEvent
    {
        public SaveEventEnum ReportType { get; set; }
        public string SavePath { get; set; }
    }
}