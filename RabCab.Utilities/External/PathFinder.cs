using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RabCab.Utilities.External
{
    internal class PathFinder
    {

        public PathFinder()
        {
        }

        public string GetAppPath(string productName)
        {
            const string foldersPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\Folders";
            var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            var subKey = baseKey.OpenSubKey(foldersPath);
            if (subKey == null)
            {
                baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                subKey = baseKey.OpenSubKey(foldersPath);
            }
            return subKey != null ? subKey.GetValueNames().FirstOrDefault(kv => kv.Contains(productName)) : "ERROR";
        }
    }
}
