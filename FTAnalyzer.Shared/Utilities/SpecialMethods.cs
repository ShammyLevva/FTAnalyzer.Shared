#pragma warning disable CA2000 // Modeless WinForms forms are owned by the Windows message loop; lifetime is managed externally
using System.Diagnostics;
using System.Runtime.InteropServices;
#if __PC__
using FTAnalyzer.Properties;
using static FTAnalyzer.Mapping.GeoResponse.CResult.CGeometry;
#endif


namespace FTAnalyzer.Utilities
{
    public static class SpecialMethods
    {
#if __PC__
        public static IEnumerable<Control> GetAllControls(Control aControl)
        {
            Stack<Control> stack = new();
            stack.Push(aControl);
            while (stack.Count != 0)
            {
                var nextControl = stack.Pop();
                foreach (Control childControl in nextControl.Controls)
                    stack.Push(childControl);
                yield return nextControl;
            }
        }

        public static void SetFonts(Form form)
        {
            try
            {
                Font font = new(FontSettings.Default.SelectedFont.Name, FontSettings.Default.SelectedFont.Size);
                foreach (Control theControl in GetAllControls(form))
                    if (theControl is not Form  // skip form itself — changing form.Font triggers PerformAutoScale (AutoScaleMode.Font) at runtime, re-scaling all control positions
                        && theControl.Font.Name.Equals(FontSettings.Default.SelectedFont.Name, StringComparison.OrdinalIgnoreCase)
                        && !ExtensionMethods.DoubleEquals(theControl.Font.Size, FontSettings.Default.SelectedFont.Size))
                    {
                        theControl.Font = font;
                        theControl.Refresh();
                    }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error processing font: {e.Message}");
            }
        }
#endif
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
                    url = url.Replace("&", "^&", StringComparison.Ordinal);
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
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