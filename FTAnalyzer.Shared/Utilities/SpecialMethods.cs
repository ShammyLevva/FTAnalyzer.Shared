#pragma warning disable CA2000 // Modeless WinForms forms are owned by the Windows message loop; lifetime is managed externally
using System.Diagnostics;
using System.Runtime.InteropServices;
#if __PC__
using static FTAnalyzer.Mapping.GeoResponse.CResult.CGeometry;
#endif


namespace FTAnalyzer.Utilities
{
    public static class SpecialMethods
    {
        // Note: WinForms font-scaling helpers (formerly GetAllControls/SetFonts) moved to
        // FTAnalyzer.Windows/Utilities/FontScaler.cs — this Shared project holds business
        // logic used by both FTAnalyzer.Windows and FTAnalyzer.Web, and those methods were
        // WinForms-only with no Web caller.
        public static void VisitWebsite(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            Process? process = null;
            try
            {
                process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = url;
                process.Start();
            }
            catch (Exception e)
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // launch via explorer.exe rather than cmd.exe so the url is passed as a
                    // literal argument (ArgumentList) instead of being parsed by a shell -
                    // avoids command injection via GEDCOM-derived weblink values.
                    ProcessStartInfo startInfo = new("explorer.exe") { CreateNoWindow = true };
                    startInfo.ArgumentList.Add(url);
                    Process.Start(startInfo);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    UIHelpers.ShowMessage($"Error processing web request. Error was : {e.Message}\nSite was: {url}");
                }
            }
            process?.Dispose();
        }

        static readonly string[] SizeSuffixes = ["bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

        public static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }

            int i = 0;
            decimal dValue = value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }
            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }

#if __PC__
        public static bool LatLongIsZero(CLocation loc) 
            => ExtensionMethods.DoubleEquals(loc.Lat, 0) && ExtensionMethods.DoubleEquals(loc.Long, 0);

        public static bool LatLongIsZero(FactLocation loc) 
            => ExtensionMethods.DoubleEquals(loc.Latitude, 0) && ExtensionMethods.DoubleEquals(loc.Longitude, 0);
#endif
    }
}