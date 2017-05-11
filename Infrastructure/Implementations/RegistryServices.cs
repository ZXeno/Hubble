using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace DeviceMonitor.Infrastructure
{
    public class RegistryServices : IRegistryServices
    {
        private const string RunRegPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        public void CreateStartupRegistryKey()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var p = Path.GetDirectoryName(path) ?? "";

            if (string.IsNullOrEmpty(p)) { return; }

            var regKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).CreateSubKey(RunRegPath);
            if (regKey != null)
            {
                regKey.SetValue("Hubble", p, RegistryValueKind.String);
                regKey.Close();
            }
        }

        public void RemoveStartupRegistryKey()
        {
            RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).DeleteSubKey(RunRegPath, false);
        }

        public bool CheckForStartupRegistryKey()
        {
            try
            {
                var targetkey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(RunRegPath);
                var value = targetkey?.GetValue("Hubble");
                if (value != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ToggleRunOnStartup()
        {
            if (CheckForStartupRegistryKey())
            {
                RemoveStartupRegistryKey();
                return;
            }

            CreateStartupRegistryKey();
        }
    }
}