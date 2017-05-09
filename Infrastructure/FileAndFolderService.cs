using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeviceMonitor.ViewModel;

namespace DeviceMonitor.Infrastructure
{
    public static class FileAndFolderService
    {
        public static async void CreateNewTextFile(string filepath)
        {
            if (File.Exists(filepath)) { return; }

            using (var outfile = new StreamWriter(filepath, true))
            {
                try { await outfile.WriteAsync(""); }
                catch (Exception e)
                {
                    throw new Exception($"Unable to create file. Error: {e.Message}");
                }
            }
        }

        public static void WriteToTextFile(string filepath, string contents)
        {
            var sb = new StringBuilder();
            sb.Append(contents);

            using (var outfile = new StreamWriter(filepath, true))
            {
                try
                {
                    outfile.WriteAsync(sb.ToString());
                }
                catch (Exception e)
                {
                    throw new Exception($"Unable to write to directory {filepath}", e);
                }
            }
        }

        public static void SaveDeviceList(List<string> deviceList)
        {
            var sb = new StringBuilder();
            var path = $"{App.UserFolder}\\devicelist.txt";
            foreach (var dev in deviceList)
            {
                sb.AppendLine(dev);
            }

            if (!File.Exists(path))
            {
                CreateNewTextFile(path);
            }

            WriteToTextFile(path, sb.ToString());
        }

        public static void LoadDeviceList()
        {
            var rawFileText = File.ReadAllText($"{App.UserFolder}\\devicelist.txt");

            var devList = new List<string>(rawFileText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            var resultList = new List<string>();

            foreach (var d in devList)
            {
                var t = d;

                t = new string(t.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());

                resultList.Add(t);
            }

            MainWindowViewModel.OnDeviceListChangeEvent(null, new DeviceListUpdateEventArgs { DeviceList = resultList });
        }
    }
}