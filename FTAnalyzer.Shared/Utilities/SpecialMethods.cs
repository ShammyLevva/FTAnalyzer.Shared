﻿using GoogleAnalyticsTracker.Core;
using GoogleAnalyticsTracker.Core.TrackerParameters;
using GoogleAnalyticsTracker.Simple;
using System.Globalization;
using System.Threading.Tasks;
using System.Diagnostics;
#if __PC__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
#endif

namespace FTAnalyzer.Utilities
{
    public static class SpecialMethods
    {
#if __PC__
        public static IEnumerable<Control> GetAllControls(Control aControl)
        {
            Stack<Control> stack = new Stack<Control>();
            stack.Push(aControl);
            while (stack.Any())
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
                foreach (Control theControl in GetAllControls(form))
                    if (theControl.Font.Name.Equals(Properties.FontSettings.Default.SelectedFont.Name))
                        theControl.Font = Properties.FontSettings.Default.SelectedFont;
            } catch (Exception e)
            {
                Console.WriteLine($"Error processing font: {e.Message}");
            }
        }
#endif
        public static void VisitWebsite(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception e)
            {
                UIHelpers.ShowMessage($"Error processing web request. Error was : {e.Message}\nSite was: {url}");
            }
        }

        public static async Task<TrackingResult> TrackEventAsync(this SimpleTracker tracker, string category, string action, string label, long value = 1)
        {
            var eventTrackingParameters = new EventTracking
            {
                ClientId = Analytics.GUID,
                UserId = Analytics.GUID,
                ApplicationName = "FTAnalyzer",
                ApplicationVersion = Analytics.AppVersion,
                Category = category,
                Action = action,
                Label = label,
                Value = value,
                ScreenName = category,
                CacheBuster = tracker.AnalyticsSession.GenerateCacheBuster(),
                ScreenResolution = Analytics.Resolution,
                CustomDimension1 = Analytics.DeploymentType,
                CustomDimension2 = Analytics.OSVersion,
                CustomDimension3 = Analytics.GUID,
                GoogleAdWordsId = "201-455-7333",
                UserLanguage = CultureInfo.CurrentUICulture.EnglishName
            };
            return await tracker.TrackAsync(eventTrackingParameters);
        }

        public static async Task<TrackingResult> TrackScreenviewAsync(this SimpleTracker tracker, string screen)
        {
            var screenViewTrackingParameters = new ScreenviewTracking
            {
                ClientId = Analytics.GUID,
                UserId = Analytics.GUID,
                ApplicationName = "FTAnalyzer",
                ApplicationVersion = Analytics.AppVersion,
                ScreenName = screen,
                CacheBuster = tracker.AnalyticsSession.GenerateCacheBuster(),
                ScreenResolution = Analytics.Resolution,
                CustomDimension1 = Analytics.DeploymentType,
                CustomDimension2 = Analytics.OSVersion,
                CustomDimension3 = Analytics.GUID,
                GoogleAdWordsId = "201-455-7333",
                UserLanguage = CultureInfo.CurrentUICulture.EnglishName
            };
            return await tracker.TrackAsync(screenViewTrackingParameters);
        }
    }
}