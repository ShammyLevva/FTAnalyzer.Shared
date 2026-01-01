using Microsoft.Win32;

namespace FTAnalyzer.Shared.Utilities
{
    public static class RegistrySettings
    {
        static string VersionIndependentRegKey
        {
            get
            {
                string versionDependent = Application.UserAppDataRegistry.Name;
                string versionIndependent =
                       versionDependent.Substring(0, versionDependent.LastIndexOf("\\"));
                return versionIndependent;
            }
        }
        public static object? GetValue(string name, object defaultValue)
        {
            return Registry.GetValue(VersionIndependentRegKey, name, defaultValue);
        }
        public static object? GetRegistryValue(string name)
        {
            return GetValue(name, null);
        }
        public static void SetValue(string name, object value, RegistryValueKind kind)
        {
            Registry.SetValue(VersionIndependentRegKey, name, value, kind);
        }
    }
}
