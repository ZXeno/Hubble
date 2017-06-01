using System.Collections.Generic;

namespace DeviceMonitor.Infrastructure
{
    public interface IFileAndFolderServices
    {
        void CreateNewTextFile(string filepath, string contents);
        void SaveDeviceList(IEnumerable<string> deviceList);
        void SaveReport(IEnumerable<string> devices, string filePath);
        IEnumerable<string> LoadDeviceList();

    }
}