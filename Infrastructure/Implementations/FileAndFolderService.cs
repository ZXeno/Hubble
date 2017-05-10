using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeviceMonitor.Infrastructure
{
    // TODO: FOR THE LOVE OF GOD DON'T LEAVE THIS AS A STATIC CLASS!!

    public static class FileAndFolderService
    {
        public static async void CreateNewTextFile(string filepath, string contents)
        {
            if (File.Exists(filepath)) { return; }

            using (var outfile = new StreamWriter(filepath, true))
            {
                try { await outfile.WriteAsync(contents); }
                catch (Exception e)
                {
                    throw new Exception($"Unable to create file. Error: {e.Message}");
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

            CreateNewTextFile(path, sb.ToString());
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

            App.EventAggregator.Publish(new DeviceListUpdateEvent {DeviceList = resultList});
        }
    }
}