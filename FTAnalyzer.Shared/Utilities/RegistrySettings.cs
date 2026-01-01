using Microsoft.Win32;
using System.Globalization;

namespace FTAnalyzer.Shared.Utilities
{
    public static class RegistrySettings
    {

        const string APPNAME = "FTAnalyzer";
        static string AppKey
        {
            get
            {
                RegistryKey? softKey = Registry.CurrentUser.OpenSubKey("Software");
                RegistryKey? companyKey = softKey?.OpenSubKey(APPNAME);
                if (companyKey is null)
                    softKey.CreateSubKey(APPNAME);
                RegistryKey? appKey = companyKey?.OpenSubKey(APPNAME);
                if (appKey is null)
                    companyKey.CreateSubKey(APPNAME);
                return appKey.ToString();
            }
        }
        public static object? GetRegistryValue(string name, object defaultValue)
        {
            return Registry.GetValue(AppKey, name, defaultValue);
        }
        public static object? GetRegistryValue(string name)
        {
            return GetRegistryValue(name, null);
        }
        public static void SetRegistryValue(string name, object value, RegistryValueKind kind)
        {
            Registry.SetValue(AppKey, name, value, kind);
        }
    }
}
