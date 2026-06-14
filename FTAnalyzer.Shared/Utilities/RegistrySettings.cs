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
                using RegistryKey appKey = Registry.CurrentUser.CreateSubKey($@"Software\{APPNAME}\{APPNAME}", true);
                return appKey.Name;
            }
        }
        public static int GetIntRegistryValue(string name, int defaultValue)
        {
            object? value = GetRegistryValue(name, defaultValue);
            if (value is null)
                return defaultValue;
            return int.Parse(value.ToString()!, CultureInfo.InvariantCulture);
        }

        public static string GetStringRegistryValue(string name, string defaultValue)
        {
            object? value = GetRegistryValue(name, defaultValue);
            return value?.ToString() ?? defaultValue;
        }

        public static bool GetBoolRegistryValue(string name, bool defaultValue)
        {
            object? value = GetRegistryValue(name, defaultValue);
            if (value is null)
                return defaultValue;
            return value.ToString() == "True";
        }

        static object? GetRegistryValue(string name, object defaultValue)
        {
            return Registry.GetValue(AppKey, name, defaultValue);
        }

        public static void SetRegistryValue(string name, object value, RegistryValueKind kind)
        {
            Registry.SetValue(AppKey, name, value, kind);
        }
    }
}
