using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeviceMonitor.Infrastructure
{
    public class FileAndFolderService : IFileAndFolderServices
    {
        public async void CreateNewTextFile(string filepath, string contents)
        {
            using (var outfile = new StreamWriter(filepath, false))
            {
                try { await outfile.WriteAsync(contents); }
                catch (Exception e)
                {
                    throw new Exception($"Unable to create file. Error: {e.Message}");
                }
            }
        }

        public void SaveDeviceList(IEnumerable<string> deviceList)
        {
            var sb = new StringBuilder();
            var path = $"{App.UserFolder}\\devicelist.txt";
            foreach (var dev in deviceList)
            {
                sb.AppendLine(dev);
            }

            CreateNewTextFile(path, sb.ToString());
        }

        public void SaveReport(IEnumerable<string> devices, string filePath)
        {
            var sb = new StringBuilder();
            foreach (var device in devices)
            {
                sb.AppendLine($"{device},");
            }

            using (var outfile = new StreamWriter(filePath, false))
            {
                try { outfile.WriteAsync(sb.ToString()); }
                catch (Exception e)
                {
                    throw new Exception($"Unable to create file. Error: {e.Message}");
                }
            }
        }

        public IEnumerable<string> LoadDeviceList()
        {
            if (!File.Exists($"{App.UserFolder}\\devicelist.txt")) { return new List<string>(); }

            var rawFileText = File.ReadAllText($"{App.UserFolder}\\devicelist.txt");

            var devList = new List<string>(rawFileText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            
            return devList;
        }
    }
}