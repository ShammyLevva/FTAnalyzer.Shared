using GoogleAnalyticsTracker.Core;
using GoogleAnalyticsTracker.Simple;
using System;
using System.Threading.Tasks;

#if __PC__
using FTAnalyzer.Properties;
using System.Deployment.Application;
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
                            MainListsAction = "Main Lists Action", ErrorsFixesAction = "Error Fixes Action";

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
                            PlacesTabEvent = "Places Tab Viewed", ExportLocationsEvent ="Locations Exported", GoogleAPIKey = "Get Google API Key";

        public const string FactsIndividualsEvent = "Individual Facts Viewed", FactsFamiliesEvent = "Family Facts Viewed", 
                            FactsGroupIndividualsEvent = "Various Individuals Facts Viewed", FactsDuplicatesEvent = "Duplicate Facts Viewed", 
                            FactsCensusRefEvent = "Census References Facts Viewed", FactsSourceEvent = "Facts for Source Viewed", FactsCensusRefIssueEvent = "Census Ref Issue Viewed";

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
            DeploymentType = ApplicationDeployment.IsNetworkDeployed ? "ClickOnce" : "Zip File";
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
            AppVersion = app.Version;
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
            AppVersion = app.Version;
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
                { Console.WriteLine(e.Message); }
        }

        public static Task TrackAction(string category, string action) => TrackActionAsync(category, action, "default");
        public static async Task TrackActionAsync(string category, string action, string value)
        {
            try
            {
                await tracker.TrackEventAsync(category, action, value);
                await tracker.TrackScreenviewAsync(category);
            }
            catch (Exception e)
                { Console.WriteLine(e.Message); }
        }

#if __PC__
        public static async Task EndProgramAsync()
        {
            try
            {
                TimeSpan duration = DateTime.Now - Settings.Default.StartTime;
                await SpecialMethods.TrackEventAsync(tracker, FTAShutdownAction, UsageEvent, duration.ToString("c"));
            }
            catch (Exception e)
            { Console.WriteLine(e.Message); }
        }

        static string SetWindowsVersion(string version)
        {
            if (version.StartsWith("6.1.7600")) return "Windows 7";
            if (version.StartsWith("6.1.7601")) return "Windows 7 SP1";
            if (version.StartsWith("6.2.9200")) return "Windows 8";
            if (version.StartsWith("6.3.9200")) return "Windows 8.1";
            if (version.StartsWith("6.3.9600")) return "Windows 8.1 Update 1";
            if (version.StartsWith("10.0.10240")) return "Windows 10";
            if (version.StartsWith("10.0.10586")) return "Windows 10 (1511)";
            if (version.StartsWith("10.0.14393")) return "Windows 10 (1607)";
            if (version.StartsWith("10.0.15063")) return "Windows 10 (1703)";
            if (version.StartsWith("10.0.16299")) return "Windows 10 (1709)";
            if (version.StartsWith("10.0.17134")) return "Windows 10 (1803)";
            if (version.StartsWith("10.0.17763")) return "Windows 10 (1809)";
            return version;
        }
#endif
    }
}
