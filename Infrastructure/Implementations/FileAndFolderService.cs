﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeviceMonitor.Infrastructure.Events;

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

        public IEnumerable<string> LoadDeviceList()
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

            return resultList;
        }
    }
}