using System.Linq;
using Microsoft.Win32;

namespace RabCab.Utilities.External
{
    /// <summary>
    ///     Internal Class to find various paths to files of different types
    /// </summary>
    internal class PathFinder
    {
        /// <summary>
        ///     Method for searching the Registry for installed software
        /// </summary>
        /// <param name="productName">The product name to search for</param>
        /// <returns>The folder path to the application - if installed</returns>
        public string GetAppPath(string productName)
        {
            //Define the entry point to search for in the registry
            const string foldersPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\Folders";
            var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            //Check for sub keys in the base folder
            var subKey = baseKey.OpenSubKey(foldersPath);
            if (subKey == null)
            {
                baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                subKey = baseKey.OpenSubKey(foldersPath);
            }


            if (subKey != null)
                return subKey.GetValueNames().FirstOrDefault(kv => kv.Contains(productName));
            return "ERROR";
        }
    }
}