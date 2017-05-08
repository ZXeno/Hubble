using System;
using System.IO;
using System.Text;

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
    }
}