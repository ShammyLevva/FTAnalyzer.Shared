using GoogleAnalyticsTracker.Core;
using GoogleAnalyticsTracker.Simple;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

#if __PC__
using FTAnalyzer.Windows.Properties;
using System.Windows.Forms;
#elif __MACOS__
using AppKit;
using Foundation;
#elif __IOS__
using UIKit;
using Foundation;
#endif

namespace FTAnalyzer.Utilities
{
    class Analytics
    {
        static readonly SimpleTrackerEnvironment trackerEnvironment;
        static readonly SimpleTracker tracker;
        static readonly AnalyticsSession analyticsSession;

        public const string MainFormAction = "Main Form Action", FactsFormAction = "Facts Form Action", CensusTabAction = "Census Tab Action",
                            ReportsAction = "Reports Action", LostCousinsAction = "Lost Cousins Action", GeocodingAction = "Geocoding Action",
                            ExportAction = "Export Action", MapsAction = "Maps Action", CensusSearchAction = "Census Search Action",
                            BMDSearchAction = "BMD Search Action", FTAStartupAction = "FTAnalyzer Startup", FTAShutdownAction = "FTAnalyzer Shutdown",
                            MainListsAction = "Main Lists Action", ErrorsFixesAction = "Error Fixes Action", GEDCOMAction = "GEDCOM Action";

        public const string LoadProgramEvent = "Load Program", UsageEvent = "Usage Time", LoadGEDCOMEvent = "Load GEDCOM", TreetopsEvent = "Treetops Report Clicked",
                            WWIReportEvent = "WWI Report Clicked", WWIIReportEvent = "WWII Report Clicked", BirthProfileEvent = "Birth Profiles Viewed",
                            OnlineManualEvent = "Online Manual Viewed", OnlineGuideEvent = "Online Guides Viewed", PrivacyEvent = "Privacy Policy Viewed",
                            OlderParentsEvent = "Older Parents Viewed", ReportIssueEvent = "Report Issue Visited", WhatsNewEvent = "Whats New Viewed",
                            ShowTimelinesEvent = "Show Timelines Viewed", GoogleGeocodingEvent = "Google Geocoding Clicked", OSGeocodingEvent = "OS Geocoding Clicked",
                            ReverseGeocodingEvent = "Reverse Geocoding Clicked", GeocodesEvent = "Geocodes Viewed", LifelinesEvent = "Show Lifelines Viewed",
                            ShowPlacesEvent = "Show Places Viewed", ViewAllSurnameEvent = "View all with Surname Clicked", GOONSEvent = "Show Guild of One Name Studies",
                            PossibleCensusEvent = "Possible Census Facts Viewed", MainListsEvent = "Main Lists Tab Viewed", ErrorsFixesEvent = "Errors Fixes Tab Viewed",
                            FactsTabEvent = "Facts Tab Viewed", SurnamesTabEvent = "Surnames Tab Viewed", CensusTabEvent = "Census Tab Viewed",
                            TreetopsTabEvent = "Treetops Tab Viewed", WorldWarsTabEvent = "World Wars Tab Viewed", TodayTabEvent = "Today Tab Viewed",
                            LostCousinsTabEvent = "Lost Cousins Tab Viewed", LocationTabViewed = "Locations Tab Viewed", IndividualsTabEvent = "Individuals Tab Viewed",
                            FamilyTabEvent = "Families Tab Viewed", SourcesTabEvent = "Sources Tab Viewed", OccupationsTabEvent = "Occupations Tab Viewed",
                            DuplicatesTabEvent = "Duplicates Tab Viewed", LooseBirthsEvent = "Loose Births Tab Viewed", LooseDeathsEvent = "Loose Deaths Tab Viewed",
                            LCReportYearEvent = "LC Year Report Run", NoLCCountryEvent = "No LC Country Clicked", LCDuplicatesEvent = "LC Duplicates Clicked",
                            NoLCCensusEvent = "No LC Census Clicked", LCWebLinkEvent = "LC Weblink Clicked", OptionsEvent = "Options Viewed", DBBackupEvent = "Database Backedup",
                            DBRestoreEvent = "Database Restored", ShowCensusEvent = "Show on Census", MissingCensusEvent = "Show Missing from Census", 
                            MissingCensusLocationEvent = "Missing Census Location Clicked", DuplicateCensusEvent = "Duplicate Census Clicked", DataErrorsTabEvent = "Data Errors Tab Viewed",
                            NoChildrenStatusEvent = "No Children Status Clicked", MisMatchedEvent = "Mismatched Children Clicked", UnrecognisedCensusEvent = "Unrecognised Census Ref",
                            ColourBMDEvent = "Colour BMD Report Clicked", ColourCensusEvent = "Colour Census Report Clicked", ExportIndEvent = "Individuals Exported",
                            ExportFamEvent = "Families Exported", ExportFactsEvent = "Facts Exported", ExportLooseBirthsEvent = "Loose Births Exported", 
                            ExportLooseDeathsEvent = "Loose Deaths Exported", ExportSourcesEvent = "Sources Exported", ExportDataErrorsEvent = "Data Errors Exported",
                            ExportTreeTopsEvent = "Treetops Exported", ExportWorldWarsEvent = "World Wars Exported", TodayClickedEvent = "Todays Events Clicked", 
                            ShowSurnamesEvent = "Show Surnames Clicked", CousinCountEvent = "Cousins Count Viewed", DirectsReportEvent = "How Many Directs Viewed", 
                            FacebookSupportEvent = "Visited Facebook Support", FacebookUsersEvent = "Visited Facebook Usergroup", CountriesTabEvent = "Countries Tab Viewed", 
                            RegionsTabEvent = "Regions Tab Viewed", SubRegionsTabEvent = "SubRegions Tab Viewed", AddressesTabEvent = "Addresses Tab Viewed", 
                            PlacesTabEvent = "Places Tab Viewed", ExportLocationsEvent ="Locations Exported", GoogleAPIKey = "Get Google API Key",
                            ReadLostCousins = "Read Lost Cousins Records", UpdateLostCousins = "Update Records on Lost Cousins", PreviewLostCousins = "Preview records for update",
                            SoftwareProvider = "Software Provider", SoftwareVersion = "Software Version", LostCousinsStats = "Lost Cousins Statistics",
                            PossiblyMissingChildren = "Possibly Missing Children Viewed", AgedOver99Report = "Aged Over 99 Viewed", AliveAtDate = "Alive at Date",
                            CustomFactTabEvent = "Custom Fact Tab Viewed", ExportSurnamesEvent = "Surnames Exported", ExportCustomFactEvent = "Custom Facts Exported";

        public const string FactsIndividualsEvent = "Individual Facts Viewed", FactsFamiliesEvent = "Family Facts Viewed", LooseInfoEvent = "Loose Info Tab Viewed",
                            FactsGroupIndividualsEvent = "Various Individuals Facts Viewed", FactsDuplicatesEvent = "Duplicate Facts Viewed", 
                            FactsCensusRefEvent = "Census References Facts Viewed", FactsSourceEvent = "Facts for Source Viewed", FactsCensusRefIssueEvent = "Census Ref Issue Viewed",
                            BirthdayEffectEvent = "Birthday Effect Report Viewed";

        public static string AppVersion { get; }
        public static string OSVersion { get; }
        public static string DeploymentType { get; }
        public static string GUID { get; }
        public static string Resolution { get; }

        static Analytics()
        {
#if __PC__
            if (Settings.Default.GUID == "00000000-0000-0000-0000-000000000000")
            {
                Settings.Default.GUID = Guid.NewGuid().ToString();
                Settings.Default.Save();
            }
            GUID = Settings.Default.GUID;
            OperatingSystem os = Environment.OSVersion;
            trackerEnvironment = new SimpleTrackerEnvironment(os.Platform.ToString(), os.Version.ToString(), os.VersionString);
            analyticsSession = new AnalyticsSession();
            tracker = new SimpleTracker("UA-125850339-2", analyticsSession, trackerEnvironment);
            AppVersion = MainForm.VERSION;
            OSVersion = SetWindowsVersion(os.Version.ToString());
            bool windowsStoreApp = Application.ExecutablePath.Contains("WindowsApps");
            bool debugging = Application.ExecutablePath.Contains("GitRepo");
            DeploymentType = windowsStoreApp ? "Windows Store" : debugging ? "Development" : "Zip File";
            string resolution = Screen.PrimaryScreen.Bounds.ToString();
#elif __MACOS__
            var userDefaults = new NSUserDefaults();
            GUID = userDefaults.StringForKey("AnalyticsKey");
            if (string.IsNullOrEmpty(GUID))
            {
                GUID = Guid.NewGuid().ToString();
                userDefaults.SetString(GUID, "AnalyticsKey");
                userDefaults.Synchronize();
            }
            NSProcessInfo info = new NSProcessInfo();
            OSVersion = $"MacOSX {info.OperatingSystemVersionString}";
            trackerEnvironment = new SimpleTrackerEnvironment("Mac OSX", info.OperatingSystemVersion.ToString(), OSVersion);
            analyticsSession = new AnalyticsSession();
            tracker = new SimpleTracker("UA-125850339-2", analyticsSession, trackerEnvironment);
            var app = (AppDelegate)NSApplication.SharedApplication.Delegate;
            AppVersion = FamilyTree.Instance.Version;
            DeploymentType = "Mac Website";
            string resolution = NSScreen.MainScreen.Frame.ToString();
#elif __IOS__
            var userDefaults = new NSUserDefaults();
            GUID = userDefaults.StringForKey("AnalyticsKey");
            if (string.IsNullOrEmpty(GUID))
            {
                GUID = Guid.NewGuid().ToString();
                userDefaults.SetString(GUID, "AnalyticsKey");
                userDefaults.Synchronize();
            }
            NSProcessInfo info = new NSProcessInfo();
            OSVersion = $"MacOSX {info.OperatingSystemVersionString}";
            trackerEnvironment = new SimpleTrackerEnvironment("Mac OSX", info.OperatingSystemVersion.ToString(), OSVersion);
            analyticsSession = new AnalyticsSession();
            tracker = new SimpleTracker("UA-125850339-2", analyticsSession, trackerEnvironment);
            var app = (AppDelegate)UIApplication.SharedApplication.Delegate;
            AppVersion = FamilyTree.Instance.Version;
            DeploymentType = "Mac Website";
            string resolution = UIScreen.MainScreen.Bounds.ToString();

#endif
            Resolution = resolution.Length > 11 ? resolution.Substring(9, resolution.Length - 10) : resolution;
        }

        public static async Task CheckProgramUsageAsync() // pre demise of Windows 7 add tracker to check how many machines still use old versions
        {
            try
            {
                await tracker.TrackEventAsync(FTAStartupAction, LoadProgramEvent, AppVersion).ConfigureAwait(false);
                await tracker.TrackScreenviewAsync(FTAStartupAction).ConfigureAwait(false);
            }
            catch (Exception e)
                { Debug.WriteLine(e.Message); }
        }

        public static Task TrackAction(string category, string action) => TrackActionAsync(category, action, "default");
        public static async Task TrackActionAsync(string category, string action, string value)
        {
            try
            {
                await tracker.TrackEventAsync(category, action, value).ConfigureAwait(false);
                await tracker.TrackScreenviewAsync(category).ConfigureAwait(false);
            }
            catch (Exception e)
                { Debug.WriteLine(e.Message); }
        }

#if __PC__
        public static async Task EndProgramAsync()
        {
            try
            {
                TimeSpan duration = DateTime.Now - Settings.Default.StartTime;
                await SpecialMethods.TrackEventAsync(tracker, FTAShutdownAction, UsageEvent, duration.ToString("c")).ConfigureAwait(false);
            }
            catch (Exception e)
            { Debug.WriteLine(e.Message); }
        }

        static string SetWindowsVersion(string version)
        {
            string result = string.Empty;
            if (version.StartsWith("6.1.7600")) result = "Windows 7";
            if (version.StartsWith("6.1.7601")) result = "Windows 7 SP1";
            if (version.StartsWith("6.2.9200")) result = "Windows 8";
            if (version.StartsWith("6.3.9200")) result = "Windows 8.1";
            if (version.StartsWith("6.3.9600")) result = "Windows 8.1 Update 1";
            if (version.StartsWith("10.0.10240")) result = "Windows 10";
            if (version.StartsWith("10.0.10586")) result = "Windows 10 (1511)";
            if (version.StartsWith("10.0.14393")) result = "Windows 10 (1607)";
            if (version.StartsWith("10.0.15063")) result = "Windows 10 (1703)";
            if (version.StartsWith("10.0.16299")) result = "Windows 10 (1709)";
            if (version.StartsWith("10.0.17134")) result = "Windows 10 (1803)";
            if (version.StartsWith("10.0.17763")) result = "Windows 10 (1809)";
            if (version.StartsWith("10.0.18362")) result = "Windows 10 (1903)";
            if (version.StartsWith("10.0.18363")) result = "Windows 10 (1909)";
            if (version.StartsWith("10.0.19041")) result = "Windows 10 (2004)";
            if (version.StartsWith("10.0.19042")) result = "Windows 10 (20H2)";
            if (version.StartsWith("10.0.19043")) result = "Windows 10 (21H1)";
            if (version.StartsWith("10.0.22000")) return "Windows 11 (21H2)";
            if (version.StartsWith("10.0.22621")) return "Windows 11 (22H2)";
            if (result.Length == 0)
                return version;
            result += Environment.Is64BitOperatingSystem ? " (x64)" : " (x86)";
            return result;
        }
#endif
    }
}
