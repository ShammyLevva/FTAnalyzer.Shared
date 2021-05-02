using FTAnalyzer.Filters;
using FTAnalyzer.Properties;
using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Numerics;
using System.Collections.Concurrent;

#if __PC__
using FTAnalyzer.Forms.Controls;
#elif __MACOS__ || __IOS__
using FTAnalyzer.ViewControllers;
#endif

namespace FTAnalyzer
{
    public class FamilyTree
    {
        #region Variables
        static FamilyTree instance;

        IList<FactSource> sources;
        IList<Individual> individuals;
        IList<Family> families;
        IList<Tuple<string, Fact>> sharedFacts;
        IDictionary<string, List<Individual>> occupations;
        IDictionary<StandardisedName, StandardisedName> names;
        IDictionary<string, List<Individual>> unknownFactTypes;
        SortableBindingList<IDisplayLocation>[] displayLocations;
        SortableBindingList<IDisplayLooseDeath> looseDeaths;
        SortableBindingList<IDisplayLooseBirth> looseBirths;
        SortableBindingList<IDisplayLooseInfo> looseInfo;
        SortableBindingList<DuplicateIndividual> duplicates;
        ConcurrentBag<DuplicateIndividual> buildDuplicates;
        const int DATA_ERROR_GROUPS = 32;
        static XmlNodeList noteNodes;
        BigInteger maxAhnentafel;
        Dictionary<string, Individual> individualLookup;
        string rootIndividualID = string.Empty;
        int SoloFamilies { get; set; }
        int PreMarriageFamilies { get; set; }
        public bool Geocoding { get; set; }
        public List<NonDuplicate> NonDuplicates { get; private set; }
        public string Version { get; set; }
        #endregion

        #region Static Functions

        FamilyTree() => ResetData();

        public static FamilyTree Instance
        {
            get
            {
                if (instance is null)
                    instance = new FamilyTree();
                return instance;
            }
        }

        public bool DocumentLoaded { get; set; }

        public static string GetText(XmlNode node, bool lookForText)
        {
            if (node is null)
                return string.Empty;
            if (node.Name.Equals("PAGE") || node.Name.Equals("TITL") || node.Name.Equals("NOTE") || node.Name.Equals("SOUR"))
                return node.InnerText.Trim();
            XmlNode text = node.SelectSingleNode(".//TEXT");
            if (text != null && lookForText && text.ChildNodes.Count > 0)
                return GetContinuationText(text.ChildNodes);
            if (node.FirstChild is null || node.FirstChild.Value is null)
                return string.Empty;
            if (node.FirstChild.NextSibling != null)
                return GetSiblingText(node.FirstChild, node.ChildNodes);
            return node.FirstChild.Value.Trim();
        }

        public static string GetText(XmlNode node, string tag, bool lookForText) => GetText(GetChildNode(node, tag), lookForText);

        public static XmlNode GetChildNode(XmlNode node, string tag) => node?.SelectSingleNode(tag);

        public static string GetNotes(XmlNode node)
        {
            if (node is null) return string.Empty;
            XmlNodeList notes = node.SelectNodes("NOTE");
            if (notes.Count == 0)
                notes = node.SelectNodes("DATA/TEXT");
            if (notes.Count == 0) return string.Empty;
            var result = new StringBuilder();
            try
            {
                foreach (XmlNode note in notes)
                {
                    if (note.ChildNodes.Count > 0)
                    {
                        result.AppendLine(GetContinuationText(note.ChildNodes));
                        result.AppendLine();
                    }
                    XmlAttribute ID = note.Attributes["REF"];
                    result.AppendLine(GetNoteRef(ID));
                    result.AppendLine();
                    result.AppendLine();
                }
            }
            catch
            { }
            return result.ToString().Trim();
        }

        static string GetNoteRef(XmlAttribute reference)
        {
            if (!instance.DocumentLoaded)
                Console.WriteLine("Looking up XML without document loaded");
            if (noteNodes is null || reference is null)
                return string.Empty;
            var result = new StringBuilder();
            foreach (XmlNode node in noteNodes)
            {
                if (node.Attributes["ID"] != null && node.Attributes["ID"].Value == reference.Value)
                {
                    result.AppendLine(GetContinuationText(node.ChildNodes));
                    return result.ToString().Trim();
                }
            }
            return string.Empty;
        }

        static string GetContinuationText(XmlNodeList nodeList)
        {
            var result = new StringBuilder();
            foreach (XmlNode child in nodeList)
            {
                if (child.Name.Equals("#text") || child.Name.Equals("CONT"))
                    result.AppendLine(); // We have a new continuation so start a new line
                if (!child.Name.Equals("SOUR"))
                    result.Append(child.InnerText);
            }
            result.AppendLine();
            return result.ToString().Trim();
        }

        static string GetSiblingText(XmlNode firstline, XmlNodeList nodeList)
        {
            var result = new StringBuilder();
            result.Append(firstline.Value.Trim());
            foreach (XmlNode child in nodeList)
            {
                if (child.Name.Equals("CONC"))
                    result.Append(child.InnerText);
            }
            result.AppendLine();
            return result.ToString().Trim();
        }

        public static string ValidFilename(string filename)
        {
            filename = filename ?? string.Empty;
            int pos = filename.IndexOfAny(Path.GetInvalidFileNameChars());
            if (pos == -1)
                return filename;
            var result = new StringBuilder();
            string remainder = filename;
            while (pos != -1)
            {
                result.Append(remainder.Substring(0, pos));
                if (pos == remainder.Length)
                {
                    remainder = string.Empty;
                    pos = -1;
                }
                else
                {
                    remainder = remainder.Substring(pos + 1);
                    pos = remainder.IndexOfAny(Path.GetInvalidFileNameChars());
                }
            }
            result.Append(remainder);
            return result.ToString();
        }
        #endregion

        #region Load Gedcom XML
        public void ResetData()
        {
            DataLoaded = false;
            sources = new List<FactSource>();
            individuals = new List<Individual>();
            families = new List<Family>();
            sharedFacts = new List<Tuple<string, Fact>>();
            occupations = new Dictionary<string, List<Individual>>();
            names = new Dictionary<StandardisedName, StandardisedName>();
            unknownFactTypes = new Dictionary<string, List<Individual>>();
            DataErrorTypes = new List<DataErrorGroup>();
            displayLocations = new SortableBindingList<IDisplayLocation>[5];
            rootIndividualID = string.Empty;
            SoloFamilies = 0;
            PreMarriageFamilies = 0;
            ResetLooseFacts();
            duplicates = null;
            buildDuplicates = null;
            ClearLocations();
#if __PC__
            TreeViewHandler.Instance.ResetData();
#endif
            noteNodes = null;
            maxAhnentafel = 0;
            FactLocation.ResetLocations();
            individualLookup = new Dictionary<string, Individual>();
        }

        public void ResetLooseFacts()
        {
            looseBirths = null;
            looseDeaths = null;
            looseInfo = null;
        }

        public void CheckUnknownFactTypes(string factType)
        {
            if (!unknownFactTypes.ContainsKey(factType))
            {
                unknownFactTypes.Add(factType, new List<Individual>());
            }
        }

        public XmlDocument LoadTreeHeader(string filename, FileStream stream, IProgress<string> outputText)
        {
            Loading = true;
            ResetData();
            rootIndividualID = string.Empty;
            outputText.Report($"Loading file {filename}\n");
            Encoding encoding = GedcomToXml.GetFileEncoding(stream);
            XmlDocument doc = GedcomToXml.LoadFile(stream, encoding, outputText, true);
            if (doc is null)
            {
                Loading = false;
                return null;
            }
            // First check if file has a valid header record ie: it is actually a GEDCOM file
            XmlNode header = doc.SelectSingleNode("GED/HEAD");
            if (header is null)
            {
                outputText.Report(string.Format($"\n\nUnable to find GEDCOM 'HEAD' record in first line of file aborting load.\nIs {filename} really a GEDCOM file"));
                Loading = false;
                return null;
            }
            XmlNode treeSoftware = doc.SelectSingleNode("GED/HEAD/SOUR");
            if (treeSoftware != null)
            {
                var softwareName = treeSoftware.SelectSingleNode("NAME")?.InnerText;
                var softwareVersion = treeSoftware.SelectSingleNode("VERS")?.InnerText;
                Task.Run(() => Analytics.TrackActionAsync(Analytics.GEDCOMAction, Analytics.SoftwareProvider, softwareName));
                Task.Run(() => Analytics.TrackActionAsync(Analytics.GEDCOMAction, Analytics.SoftwareVersion, softwareVersion));
            }
            XmlNode charset = doc.SelectSingleNode("GED/HEAD/CHAR");
            string fileCharset = charset.InnerText;
            XmlDocument tempDoc = null;
            if (charset != null)
            {
                switch (fileCharset)
                {
                    case "ANSEL":
                        tempDoc = GedcomToXml.LoadAnselFile(stream, outputText, false);
                        break;
                    case "UNICODE":
                        tempDoc = GedcomToXml.LoadFile(stream, Encoding.Unicode, outputText, false);
                        break;
                    case "ASCII":
                        tempDoc = GedcomToXml.LoadFile(stream, Encoding.ASCII, outputText, false);
                        break;
                    case "UTF-8":
                        tempDoc = GedcomToXml.LoadFile(stream, Encoding.UTF8, outputText, false);
                        break;
                }
            }
            if (tempDoc != null && tempDoc.SelectNodes("GED/INDI").Count > 0)
                doc = tempDoc;
            if (doc.SelectNodes("GED/INDI").Count == 0)
            {
                Loading = false;
                outputText.Report("Failed to load file no individuals found.");
                return null;
            }
            ReportOptions(outputText);
            SetRootIndividual(doc);
            // doc.Save(new FileStream(@"c:\temp\tim.xml", FileMode.Create));
            return doc;
        }

        void SetRootIndividual(XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("GED/HEAD/_ROOT");
            if (root != null)
            {
                // file has a root individual
                try
                {
                    rootIndividualID = root.Attributes["REF"].Value;
                }
                catch (Exception)
                { } // don't crash if can't set root individual
            }
        }

        public void LoadTreeSources(XmlDocument doc, IProgress<int> progress, IProgress<string> outputText)
        {
            // First iterate through attributes of root finding all sources
            XmlNodeList list = doc.SelectNodes("GED/SOUR");
            int sourceMax = list.Count == 0 ? 1 : list.Count;
            int counter = 0;
            foreach (XmlNode n in list)
            {
                var fs = new FactSource(n);
                sources.Add(fs);
                progress.Report(100 * counter++ / sourceMax);
            }
            outputText.Report($"Loaded {counter} sources.\n");
            progress.Report(100);
            // now get a list of all notes
            noteNodes = doc.SelectNodes("GED/NOTE");
        }

        public void LoadTreeIndividuals(XmlDocument doc, IProgress<int> progress, IProgress<string> outputText)
        {
            // now iterate through child elements of root
            // finding all individuals
            XmlNodeList list = doc.SelectNodes("GED/INDI");
            int individualMax = list.Count;
            int counter = 0;
            foreach (XmlNode n in list)
            {
                var individual = new Individual(n, outputText);
                if (individual.IndividualID is null)
                    outputText.Report("File has invalid GEDCOM data. Individual found with no ID. Search file for 0 @@ INDI\n");
                else
                {
                    // debugging of individuals - outputText.Report($"Loaded Individual: {individual.ToString()}\n");
                    individuals.Add(individual);
                    if (individualLookup.ContainsKey(individual.IndividualID))
                        outputText.Report($"More than one INDI record found with ID value {individual.IndividualID}\n");
                    else
                        individualLookup.Add(individual.IndividualID, individual);
                    AddOccupations(individual);
                    AddCustomFacts(individual);
                    progress.Report((100 * counter++) / individualMax);
                }
            }
            outputText.Report($"Loaded {counter} individuals.\n");
            progress.Report(100);
        }

        public void LoadTreeFamilies(XmlDocument doc, IProgress<int> progress, IProgress<string> outputText)
        {
            // now iterate through child elements of root
            // finding all families
            XmlNodeList list = doc.SelectNodes("GED/FAM");
            int familyMax = list.Count == 0 ? 1 : list.Count;
            int counter = 0;
            foreach (XmlNode n in list)
            {
                Family family = new Family(n, outputText);
                families.Add(family);
                progress.Report((100 * counter++) / familyMax);
            }
            outputText.Report($"Loaded {counter} families.\n");
            CheckAllIndividualsAreInAFamily(outputText);
            RemoveFamiliesWithNoIndividuals();
            progress.Report(100);
        }

        public void LoadTreeRelationships(XmlDocument doc, IProgress<int> progress, IProgress<string> outputText)
        {
            if (string.IsNullOrEmpty(rootIndividualID))
                rootIndividualID = individuals[0].IndividualID;
            UpdateRootIndividual(rootIndividualID, progress, outputText); //, true);
            CreateSharedFacts();
            CreateLostCousinsFacts(outputText);
            CountCensusFacts(outputText);
            FixIDs();
            SetDataErrorTypes(progress);
            CountUnknownFactTypes(outputText);
            FactLocation.LoadGoogleFixesXMLFile(outputText);
            LoadGEDCOM_PLAC_Locations(doc.SelectNodes("GED/_PLAC_DEFN/PLAC"), 60, progress, outputText); // Legacy Family Tree
            LoadGEDCOM_PLAC_Locations(doc.SelectNodes("GED/_PLAC"), 80, progress, outputText); // Family Historian PLAC format
            LoadGeoLocationsFromDataBase(outputText);
            progress.Report(100);
            DataLoaded = true;
            Loading = false;
        }

        public void CleanUpXML() => noteNodes = null;

        void LoadGEDCOM_PLAC_Locations(XmlNodeList list, int startval, IProgress<int> progress, IProgress<string> outputText)
        {
            int max = list.Count;
            int counter = 0;
            int value;
            foreach (XmlNode node in list)
            {
                string place = GetText(node, false);
                XmlNode lat_node = node.SelectSingleNode("MAP/LATI");
                XmlNode long_node = node.SelectSingleNode("MAP/LONG");
                if (place.Length > 0 && lat_node != null && long_node != null)
                {
                    string lat = lat_node.InnerText;
                    string lng = long_node.InnerText;
                    FactLocation loc = FactLocation.GetLocation(place, lat, lng, FactLocation.Geocode.GEDCOM_USER, true, true);
                    loc.FTAnalyzerCreated = false;
                    loc.GEDCOMLatLong = true;
                    if (!loc.IsValidLatLong)
                        outputText.Report($"'PLAC' record in GEDCOM has Location: {place} with invalid Lat/Long '{lat},{lng}'.");
                }
                value = startval + 15 * (counter++ / max);
                if (value > 100) value = 100;
                progress.Report(value);
            }
        }
        void CreateLostCousinsFacts(IProgress<string> outputText)
        {
            try
            {
                int count = DatabaseHelper.AddLostCousinsFacts();
                outputText.Report($"Created {count} Lost Cousins facts from records previously uploaded by FTAnalyzer");
            }
            catch (Exception ex)
            {
                outputText.Report($"Error loading previously submitted Lost Cousins data. {ex.Message}");
            }
        }

        public static bool LoadGeoLocationsFromDataBase(IProgress<string> outputText)
        {
            outputText.Report("");
#if __PC__
            try
            {
                DatabaseHelper.LoadGeoLocations();
                WriteGeocodeStatstoRTB(string.Empty, outputText);
            }
            catch (Exception ex)
            {
                outputText.Report($"Error loading previously geocoded data. {ex.Message}");
                return false;
            }
#endif
            return true;
        }

        public void UpdateRootIndividual(string rootIndividualID, IProgress<int> progress, IProgress<string> outputText) //, bool locationsToFollow = false)
        {
            outputText.Report($"\nCalculating Relationships using {rootIndividualID}: {GetIndividual(rootIndividualID)?.Name} as root person. Please wait\n\n");

            // When the user changes the root individual, no location processing is taking place
            //int locationCount = locationsToFollow ? FactLocation.AllLocations.Count() : 0;
            SetRelations(rootIndividualID, outputText);
            progress?.Report(10);
            SetRelationDescriptions(rootIndividualID);
            outputText.Report(PrintRelationCount());
            progress?.Report(20);
        }

        public void LoadStandardisedNames(string startPath)
        {
            try
            {
                string filename = Path.Combine(startPath, @"Resources\GINAP.txt");
                if (File.Exists(filename))
                    ReadStandardisedNameFile(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load Standardised names error was : {e.Message}");
            }
        }

        void ReadStandardisedNameFile(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    if (line.IndexOf(",", StringComparison.Ordinal) > 0 && (values[0] == "1" || values[0] == "2"))
                    {
                        StandardisedName original = new StandardisedName(values[0] == "2", values[2]);
                        StandardisedName standardised = new StandardisedName(values[1] == "2", values[3]);
                        names.Add(original, standardised);
                    }
                }
            }
        }

        public string GetStandardisedName(bool IsMale, string name)
        {
            StandardisedName gIn = new StandardisedName(IsMale, name);
            names.TryGetValue(gIn, out StandardisedName gOut);
            return gOut is null ? name : gOut.Name;
        }

        void ReportOptions(IProgress<string> outputText)
        {
            if (GeneralSettings.Default.ReportOptions)
            {
                outputText.Report($"\nThe current file handling options are set :");
                outputText.Report($"\n    Retry failed lines by looking for bad line breaks: {FileHandling.Default.RetryFailedLines}");
                outputText.Report($"\n    Convert Diacritics on load {FileHandling.Default.ConvertDiacritics}");

                outputText.Report($"\nThe current general options are set:");
                outputText.Report($"\n    Use Baptism/Christening Date If No Birth Date: {GeneralSettings.Default.UseBaptismDates}");
                outputText.Report($"\n    Use Burial/Cremation Date If No Death Date: {GeneralSettings.Default.UseBurialDates}");
                outputText.Report($"\n    Allow Empty Values In Locations: {GeneralSettings.Default.AllowEmptyLocations}");
                outputText.Report($"\n    Show Multiple Fact Forms When Viewing Duplicates: {GeneralSettings.Default.MultipleFactForms}");
                outputText.Report($"\n    Loose Birth Minimum Parental Age: {GeneralSettings.Default.MinParentalAge}");
                outputText.Report($"\n    Show Alias In Name Displays: {GeneralSettings.Default.ShowAliasInName}");
                outputText.Report($"\n    Files use Country First Locations: {GeneralSettings.Default.ReverseLocations}");
                outputText.Report($"\n    Show World Events on the 'On This Day' tab: {GeneralSettings.Default.ShowWorldEvents}");
                outputText.Report($"\n    Ignore Unknown/Custom Fact Type Warnings: {GeneralSettings.Default.IgnoreFactTypeWarnings}");
                outputText.Report($"\n    Treat Female Surnames as Unknown: {GeneralSettings.Default.TreatFemaleSurnamesAsUnknown}");
                outputText.Report($"\n    Show Ancestors that are muliple directs: {GeneralSettings.Default.ShowMultiAncestors}");
                outputText.Report($"\n    Hide Ignored Duplicates: {GeneralSettings.Default.HideIgnoredDuplicates}");
                outputText.Report($"\nThe current census options are set:");
                outputText.Report($"\n    Treat Residence Facts As Census Facts: {GeneralSettings.Default.UseResidenceAsCensus}");
                outputText.Report($"\n    Convert Residences with valid census reference to Census Facts: {GeneralSettings.Default.ConvertResidenceFacts}");
                outputText.Report($"\n    Tolerate Slightly Inaccurate Census Dates: {GeneralSettings.Default.TolerateInaccurateCensusDate}");
                outputText.Report($"\n    Family Census Facts Apply To Only Parents: {GeneralSettings.Default.OnlyCensusParents}");
                outputText.Report($"\n    Use Compact Census References: {GeneralSettings.Default.UseCompactCensusRef}");
                outputText.Report($"\n    Auto Create Census Events from Notes & Sources: {GeneralSettings.Default.AutoCreateCensusFacts}");
                outputText.Report($"\n    Add Auto Created Census Locations to Locations List: {GeneralSettings.Default.AddCreatedLocations}");
                outputText.Report($"\n    Hide People Tagged As Missing From Census: {GeneralSettings.Default.HidePeopleWithMissingTag}");
                outputText.Report($"\n    Skip Checking for Census References: {GeneralSettings.Default.SkipCensusReferences}");

#if __PC__
                outputText.Report($"\nThe current mapping options are set:");
                outputText.Report($"\n    Custom Maps Location: {MappingSettings.Default.CustomMapPath}");
                outputText.Report($"\n    Display British Parish Boundaries: {MappingSettings.Default.UseParishBoundaries}");
                outputText.Report($"\n    Hide Scale Bar: {MappingSettings.Default.HideScaleBar}");
                outputText.Report($"\n    Include Locations with Partial Match Status: {MappingSettings.Default.IncludePartials}");
#endif
                outputText.Report("\n\n");
            }
        }

        void RemoveFamiliesWithNoIndividuals() => (families as List<Family>).RemoveAll(x => x.FamilySize == 0);

        void CountUnknownFactTypes(IProgress<string> outputText)
        {
            if (unknownFactTypes.Count > 0 && !GeneralSettings.Default.IgnoreFactTypeWarnings)
            {
                outputText.Report("\nThe following unknown/custom fact types were reported.\nNB. This isn't an error if you deliberately created these fact types.\nThis is simply highlighting the types so you can check for any possible errors/duplicate types.\n");
                foreach (string tag in unknownFactTypes.Keys)
                {
                    int count = unknownFactTypes[tag].Count;
                    bool ignore = DatabaseHelper.IgnoreCustomFact(tag);
                    if (count > 0 && !ignore)
                        outputText.Report($"\nFound {count} facts of unknown/custom fact type {tag}");
                }
                outputText.Report("\n");
            }
        }

        void CreateSharedFacts()
        {
            foreach (Tuple<string, Fact> t in sharedFacts)
            {
                Individual ind = GetIndividual(t.Item1);
                Fact fact = t.Item2;
                if (ind != null && !ind.Facts.ContainsFact(fact))
                    ind.AddFact(fact);
            }
        }

        void CountCensusFacts(IProgress<string> outputText)
        {
            int censusFacts = 0;
            int censusFTAFacts = 0;
            int resiFacts = 0;
            int lostCousinsFacts = 0;
            int censusWarnAllow = 0;
            int resiCensus = 0;
            int resiWarnAllow = 0;
            int lostCousinsWarnAllow = 0;
            int censusWarnIgnore = 0;
            int lostCousinsWarnIgnore = 0;
            int censusErrors = 0;
            int lostCousinsErrors = 0;
            int censusReferences = 0;
            int blankCensusRefs = 0;
            int partialCensusRefs = 0;
            int unrecognisedCensusRefs = 0;
            foreach (Individual ind in individuals)
            {
                censusFacts += ind.FactCount(Fact.CENSUS);
                censusFTAFacts += ind.FactCount(Fact.CENSUS_FTA);
                censusWarnAllow += ind.ErrorFactCount(Fact.CENSUS, Fact.FactError.WARNINGALLOW);
                censusWarnIgnore += ind.ErrorFactCount(Fact.CENSUS, Fact.FactError.WARNINGIGNORE);
                censusErrors += ind.ErrorFactCount(Fact.CENSUS, Fact.FactError.ERROR);
                resiFacts += ind.FactCount(Fact.RESIDENCE);
                resiWarnAllow += ind.ErrorFactCount(Fact.RESIDENCE, Fact.FactError.WARNINGALLOW);
                resiCensus += ind.ResidenceCensusFactCount;
                lostCousinsFacts += ind.FactCount(Fact.LOSTCOUSINS) + ind.FactCount(Fact.LC_FTA);
                lostCousinsWarnAllow += ind.ErrorFactCount(Fact.LOSTCOUSINS, Fact.FactError.WARNINGALLOW) + ind.ErrorFactCount(Fact.LC_FTA, Fact.FactError.WARNINGALLOW);
                lostCousinsWarnIgnore += ind.ErrorFactCount(Fact.LOSTCOUSINS, Fact.FactError.WARNINGIGNORE) + ind.ErrorFactCount(Fact.LC_FTA, Fact.FactError.WARNINGIGNORE);
                lostCousinsErrors += ind.ErrorFactCount(Fact.LOSTCOUSINS, Fact.FactError.ERROR) + ind.ErrorFactCount(Fact.LC_FTA, Fact.FactError.ERROR);
                censusReferences += ind.CensusReferenceCount(CensusReference.ReferenceStatus.GOOD);
                blankCensusRefs += ind.CensusReferenceCount(CensusReference.ReferenceStatus.BLANK);
                partialCensusRefs += ind.CensusReferenceCount(CensusReference.ReferenceStatus.INCOMPLETE);
                unrecognisedCensusRefs += ind.CensusReferenceCount(CensusReference.ReferenceStatus.UNRECOGNISED);
            }
            int censusTotal = censusFacts + censusWarnAllow + censusWarnIgnore + censusErrors;
            int resiTotal = resiFacts + resiWarnAllow;
            int lostCousinsTotal = lostCousinsFacts + lostCousinsWarnAllow + lostCousinsWarnIgnore + lostCousinsErrors;

            outputText.Report($"\nFound {censusTotal} census facts in GEDCOM File ({censusFacts} good, ");
            if (censusWarnAllow > 0)
                outputText.Report($"{censusWarnAllow} warnings (data tolerated), ");
            if (censusWarnIgnore > 0)
                outputText.Report($"{censusWarnIgnore} warnings (data ignored in strict mode), ");
            if (censusErrors > 0)
                outputText.Report($"{censusErrors} errors (data discarded), ");
            outputText.Report($"{censusFacts + censusWarnAllow} usable facts loaded)");

            outputText.Report($"\nCreated {censusFTAFacts} census facts from individuals notes and source references in GEDCOM File");
            outputText.Report($"\nFound {resiTotal} residence facts in GEDCOM File ({resiCensus} treated as census facts) ");
            if (resiWarnAllow > 0)
            {
                if (GeneralSettings.Default.TolerateInaccurateCensusDate)
                    outputText.Report($"{resiWarnAllow} warnings (data tolerated), ");
                else
                    outputText.Report($"{resiWarnAllow} warnings (data ignored in strict mode), ");
            }
            if (GeneralSettings.Default.SkipCensusReferences)
                outputText.Report("No census references loaded as option to Skip Census Reference checking is turned on");
            else
            {
                outputText.Report($"\nFound {censusReferences} census references in file and {blankCensusRefs} facts missing a census reference");
                if (partialCensusRefs > 0)
                    outputText.Report($", with {partialCensusRefs} references with partial details");
                if (unrecognisedCensusRefs > 0)
                    outputText.Report($" and {unrecognisedCensusRefs} references that were unrecognised");
            }
            outputText.Report($"\nFound {lostCousinsTotal} Lost Cousins facts in GEDCOM File ({lostCousinsFacts} good, ");
            if (lostCousinsWarnAllow > 0)
                outputText.Report($"{lostCousinsWarnAllow} warnings (data tolerated), ");
            if (lostCousinsWarnIgnore > 0)
                outputText.Report($"{lostCousinsWarnIgnore} warnings (data ignored in strict mode), ");
            if (lostCousinsErrors > 0)
                outputText.Report($"{lostCousinsErrors} errors (data discarded), ");
            outputText.Report($"{lostCousinsFacts + lostCousinsWarnAllow} usable facts loaded)");
            if (censusFacts == 0 && resiCensus == 0 && censusWarnAllow == 0 && censusFTAFacts == 0)
            {
                outputText.Report("\nFound no census or suitable residence facts in GEDCOM File and no recognisable\n");
                outputText.Report("census references in notes or in source records stored against an individual.\n\n");
                outputText.Report("The most likely reason is that you have recorded census facts as notes and have\n");
                outputText.Report("not recorded any census references. This will mean that the census report will\n");
                outputText.Report("show everyone as not yet found on census and the Lost Cousins report will show\n");
                outputText.Report("no-one with a census needing to be entered onto your Lost Cousins My Ancestors page.");
            }
            outputText.Report("\n");
        }

        Dictionary<CensusDate, int> MissingLCEntries;
        int LCFound;
        int LCMissing;
        int LCUploadable;
        int LCInvalidRef;
#if __PC__
        readonly string separator = $"————————————————————————————————————————————————————\n";
#elif __MACOS__
        readonly string separator = $"————————————————————————————————\n";
#endif

        public string UpdateLostCousinsReport(Predicate<Individual> relationFilter)
        {
            StringBuilder output = new StringBuilder();
            output.Append("Lost Cousins facts recorded:\n\n");

            IEnumerable<Individual> listToCheck = AllIndividuals.Filter(relationFilter).ToList();
            MissingLCEntries = new Dictionary<CensusDate, int>();

            int countEW1841 = listToCheck.Count(ind => ind.IsLostCousinsEntered(CensusDate.EWCENSUS1841, false));
            int countEW1881 = listToCheck.Count(ind => ind.IsLostCousinsEntered(CensusDate.EWCENSUS1881, false));
            int countSco1881 = listToCheck.Count(ind => ind.IsLostCousinsEntered(CensusDate.SCOTCENSUS1881, false));
            int countCan1881 = listToCheck.Count(ind => ind.IsLostCousinsEntered(CensusDate.CANADACENSUS1881, false));
            int countEW1911 = listToCheck.Count(ind => ind.IsLostCousinsEntered(CensusDate.EWCENSUS1911, false));
            int countIre1911 = listToCheck.Count(ind => ind.IsLostCousinsEntered(CensusDate.IRELANDCENSUS1911, false));
            int countUS1880 = listToCheck.Count(ind => ind.IsLostCousinsEntered(CensusDate.USCENSUS1880, false));
            int countUS1940 = listToCheck.Count(ind => ind.IsLostCousinsEntered(CensusDate.USCENSUS1940, false));

            int missingEW1841 = listToCheck.Count(ind => ind.MissingLostCousins(CensusDate.EWCENSUS1841, false));
            int missingEW1881 = listToCheck.Count(ind => ind.MissingLostCousins(CensusDate.EWCENSUS1881, false));
            int missingSco1881 = listToCheck.Count(ind => ind.MissingLostCousins(CensusDate.SCOTCENSUS1881, false));
            int missingCan1881 = listToCheck.Count(ind => ind.MissingLostCousins(CensusDate.CANADACENSUS1881, false));
            int missingEW1911 = listToCheck.Count(ind => ind.MissingLostCousins(CensusDate.EWCENSUS1911, false));
            int missingIre1911 = listToCheck.Count(ind => ind.MissingLostCousins(CensusDate.IRELANDCENSUS1911, false));
            int missingUS1880 = listToCheck.Count(ind => ind.MissingLostCousins(CensusDate.USCENSUS1880, false));
            int missingUS1940 = listToCheck.Count(ind => ind.MissingLostCousins(CensusDate.USCENSUS1940, false));

            MissingLCEntries.Add(CensusDate.EWCENSUS1841, missingEW1841);
            MissingLCEntries.Add(CensusDate.EWCENSUS1881, missingEW1881);
            MissingLCEntries.Add(CensusDate.SCOTCENSUS1881, missingSco1881);
            MissingLCEntries.Add(CensusDate.CANADACENSUS1881, missingCan1881);
            MissingLCEntries.Add(CensusDate.EWCENSUS1911, missingEW1911);
            MissingLCEntries.Add(CensusDate.IRELANDCENSUS1911, missingIre1911);
            MissingLCEntries.Add(CensusDate.USCENSUS1880, missingUS1880);
            MissingLCEntries.Add(CensusDate.USCENSUS1940, missingUS1940);

            int LostCousinsCensusYearFacts = listToCheck.Sum(ind => ind.LostCousinsCensusFactCount);
            int LCnoCensus = listToCheck.Count(i => i.HasLostCousinsFactWithNoCensusFact);

            int moreThanOneLCfact = listToCheck.Sum(i => i.DuplicateLCFacts);
            int duplicateLCCensusFacts = listToCheck.Sum(i => i.DuplicateLCCensusFacts);
            int LCtotal = listToCheck.Sum(i => i.LostCousinsFacts);
            int total = countEW1841 + countEW1881 + countSco1881 + countCan1881 + countEW1911 + countIre1911 + countUS1880 + countUS1940 + moreThanOneLCfact;
            int missingTotal = missingEW1841 + missingEW1881 + missingSco1881 + missingCan1881 + missingEW1911 + missingIre1911 + missingUS1880 + missingUS1940;
            int noCountryTotal = LostCousinsCensusYearFacts - missingTotal - LCtotal - duplicateLCCensusFacts;

            output.Append($"1881 England & Wales Census: {countEW1881} LC Records, {missingEW1881} Missing LC Record\n");
            output.Append($"1841 England & Wales Census: {countEW1841} LC Records, {missingEW1841} Missing LC Record\n");
            output.Append($"1911 England & Wales Census: {countEW1911} LC Records, {missingEW1911} Missing LC Record\n");
            output.Append(separator);
            output.Append($"1881 Scotland Census: {countSco1881} LC Records, {missingSco1881} Missing LC Record\n");
            output.Append($"1911 Ireland Census: {countIre1911} LC Records, {missingIre1911} Missing LC Record\n");
            output.Append($"1881 Canada Census: {countCan1881} LC Records, {missingCan1881} Missing LC Record\n");
            output.Append(separator);
            output.Append($"1880 US Census: {countUS1880} LC Records, {missingUS1880} Missing LC Record\n");
            output.Append($"1940 US Census: {countUS1940} LC Records, {missingUS1940} Missing LC Record\n");
            output.Append(separator);
            if (moreThanOneLCfact > 0)
                output.Append($"Duplicate Lost Cousins facts: {moreThanOneLCfact}\n");
            if (LCtotal > total)
                output.Append($"Lost Cousins fact where Census fact has no country : {LCtotal - total}\n");
            //if (noCountryTotal > 0)
            //    rtbLostCousins.AppendText($"Census facts with no census country and no Lost Cousins fact : {noCountryTotal}\n");
            if (moreThanOneLCfact > 0 || LCtotal > total) // || noCountryTotal > 0)
                output.Append(separator);
            output.Append($"Totals: {LCtotal} Lost Cousin Records, {missingTotal} Missing Lost Cousins Record");

            if (LCnoCensus > 0 || missingTotal > 0)
                output.Append("\n\n");
            if (LCnoCensus > 0)
                output.Append($"Lost Cousins facts with bad/missing census fact: {LCnoCensus}\n\n");
            if (missingTotal > 0)
            {
                output.Append($"You have {missingTotal} Census facts with no Lost Cousins fact");
                output.Append("\nUse the Updates tab to automatically upload them today.");
                output.Append("\nUse the Lost Cousins Website Link to join if you aren't already a member.");
            }
            LCFound = LCtotal;
            LCMissing = missingTotal;
            return output.ToString();
        }

        public string LCOutput(List<CensusIndividual> LCUpdates, List<CensusIndividual> LCInvalidReferences, Predicate<CensusIndividual> relationFilter)
        {
            LCInvalidRef = 0;
            StringBuilder output = new StringBuilder();
            Tuple<List<CensusIndividual>, List<CensusIndividual>> result;
            result = GetMissingLCIndividuals(CensusDate.EWCENSUS1881, relationFilter, output);
            LCUpdates.AddRange(result.Item1);
            LCInvalidReferences.AddRange(result.Item2);
            result = GetMissingLCIndividuals(CensusDate.EWCENSUS1841, relationFilter, output);
            LCUpdates.AddRange(result.Item1);
            LCInvalidReferences.AddRange(result.Item2);
            result = GetMissingLCIndividuals(CensusDate.EWCENSUS1911, relationFilter, output);
            LCUpdates.AddRange(result.Item1);
            LCInvalidReferences.AddRange(result.Item2);
            output.Append(separator);
            result = GetMissingLCIndividuals(CensusDate.SCOTCENSUS1881, relationFilter, output);
            LCUpdates.AddRange(result.Item1);
            LCInvalidReferences.AddRange(result.Item2);
            //LCUpdates.AddRange(GetMissingLCIndividuals(CensusDate.IRELANDCENSUS1911, rtbLCUpdateData));
            //LCUpdates.AddRange(result.Item1);
            //LCInvalidReferences.AddRange(result.Item2);
            result = GetMissingLCIndividuals(CensusDate.CANADACENSUS1881, relationFilter, output);
            LCUpdates.AddRange(result.Item1);
            LCInvalidReferences.AddRange(result.Item2);
            output.Append(separator);
            result = GetMissingLCIndividuals(CensusDate.USCENSUS1880, relationFilter, output);
            LCUpdates.AddRange(result.Item1);
            LCInvalidReferences.AddRange(result.Item2);
            result = GetMissingLCIndividuals(CensusDate.USCENSUS1940, relationFilter, output);
            LCUpdates.AddRange(result.Item1);
            LCInvalidReferences.AddRange(result.Item2);
            output.Append(separator);
            output.Append($"{LCUpdates.Count} possible records to upload to Lost Cousins.");
            output.Append("\n\nUse the login form on the left to login to activate upload button");
            LCUploadable = LCUpdates.Count;
            LCInvalidRef = LCInvalidReferences.Count;
            string stats = $"{LCMissing} census records ({LCUploadable} uploadable, {LCInvalidRef} invalid LC refs), and {LCFound} already entered.";
            Task.Run(() => Analytics.TrackActionAsync(Analytics.LostCousinsAction, Analytics.LostCousinsStats, stats));
            return output.ToString();
        }

        Tuple<List<CensusIndividual>, List<CensusIndividual>> GetMissingLCIndividuals(CensusDate censusDate, Predicate<CensusIndividual> relationFilter, StringBuilder output)
        {
            IEnumerable<CensusFamily> censusFamilies = GetAllCensusFamilies(censusDate, true, false);

            bool missingLC(CensusIndividual x) => x.MissingLostCousins(censusDate, false) && x.CensusReference != null && x.CensusReference.IsGoodStatus;
            Predicate<CensusIndividual> missingFilter = FilterUtils.AndFilter(relationFilter, missingLC);
            List<CensusIndividual> missingIndiv = censusFamilies.SelectMany(f => f.Members).Filter(missingFilter).Distinct(new CensusIndividualComparer()).ToList();
            bool invalidRef(CensusIndividual x) => x.MissingLostCousins(censusDate, false) && x.CensusReference != null && !x.CensusReference.IsGoodStatus;
            //bool nameFilter(CensusIndividual x) => x.LCForename.Length > 0 && x.LCSurname.Length > 0 && x.Surname != Individual.UNKNOWN_NAME && x.LCSurname !=Individual.UNKNOWN_NAME;
            //bool ageFilter(CensusIndividual x) => x.Age != Age.Unknown;
            Predicate<CensusIndividual> invalidRefFilter = FilterUtils.AndFilter(relationFilter, invalidRef);
            //FilterUtils.AndFilter(FilterUtils.AndFilter(relationFilter, invalidRef), FilterUtils.AndFilter<CensusIndividual>(nameFilter, ageFilter));

            List<CensusIndividual> invalidRefIndiv = censusFamilies.SelectMany(f => f.Members).Filter(invalidRefFilter).Distinct(new CensusIndividualComparer()).ToList();
            int missing = MissingLCEntries[censusDate];
            output.Append($"{censusDate}: {missingIndiv.Count} possible {missing - missingIndiv.Count} without valid Lost Cousins details\n");
            return Tuple.Create(missingIndiv, invalidRefIndiv);
        }

        void AddOccupations(Individual individual)
        {
            List<string> jobs = new List<string>();
            foreach (Fact f in individual.GetFacts(Fact.OCCUPATION))
            {
                if (!jobs.ContainsString(f.Comment))
                {
                    if (!occupations.TryGetValue(f.Comment, out List<Individual> workers))
                    {
                        workers = new List<Individual>();
                        occupations.Add(f.Comment, workers);
                    }
                    workers.Add(individual);
                    jobs.Add(f.Comment);
                }
            }
        }
        void AddCustomFacts(Individual individual)
        {
            foreach (string factType in unknownFactTypes.Keys)
            {
                if (individual.AllFacts.Any(x => x.FactTypeDescription == factType))
                    unknownFactTypes[factType].Add(individual);
            }
        }

        void CheckAllIndividualsAreInAFamily(IProgress<string> outputText)
        {
            foreach (Family f in families)
            {
                if (f.Husband != null)
                {
                    f.Husband.Infamily = true;
                    f.Husband.ReferralFamilyID = f.FamilyID;
                }
                if (f.Wife != null)
                {
                    f.Wife.Infamily = true;
                    f.Wife.ReferralFamilyID = f.FamilyID;
                }
                foreach (Individual c in f.Children)
                {
                    c.Infamily = true;
                    c.ReferralFamilyID = f.FamilyID;
                    c.HasParents = f.Husband != null || f.Wife != null;
                    c.HasOnlyOneParent = (f.Husband != null && f.Wife is null) || (f.Husband is null && f.Wife != null);
                }
            }
            foreach (Individual ind in individuals)
            {
                if (!ind.IsInFamily)
                    families.Add(new Family(ind, NextSoloFamily));
            }
            if (SoloFamilies > 0)
                outputText.Report($"Added {SoloFamilies} lone individuals as single families.\n");
        }
        #endregion

        #region Properties

        public bool Loading { get; private set; }
        public bool DataLoaded { get; private set; }

        public List<ExportFact> AllExportFacts
        {
            get
            {
                List<ExportFact> result = new List<ExportFact>();
                foreach (Individual ind in individuals)
                {
                    foreach (Fact f in ind.PersonalFacts)
                        result.Add(new ExportFact(ind, f));
                    foreach (Family fam in ind.FamiliesAsSpouse)
                        foreach (Fact famfact in fam.Facts)
                            result.Add(new ExportFact(ind, famfact));
                }
                return result;
            }
        }

        public IEnumerable<Family> AllFamilies => families;

        public IEnumerable<Individual> AllIndividuals => individuals;

        public IEnumerable<FactSource> AllSources => sources;

        public IEnumerable<IDisplayDataError> AllDataErrors => DataErrorTypes.SelectMany(dg => dg.Errors);

        public int IndividualCount => individuals.Count;

        public List<Individual> DeadOrAlive => individuals.Filter(x => x.DeathDate.IsKnown && x.IsFlaggedAsLiving).ToList();

        public string NextSoloFamily { get { return $"SF{++SoloFamilies}"; } }

        public string NextPreMarriageFamily { get { return $"PM{++PreMarriageFamilies}"; } }
        #endregion

        #region Property Functions

        public IEnumerable<Individual> GetAllRelationsOfType(int relationType) => individuals.Filter(ind => ind.RelationType == relationType);

        public IEnumerable<Individual> GetUncertifiedFacts(string factType, int relationType)
        {
            return individuals.Filter(ind =>
            {
                if (ind.RelationType == relationType)
                {
                    Fact f = ind.GetPreferredFact(factType);
                    return (f != null && !f.CertificatePresent);
                }
                return false;
            });
        }

        public FactSource GetSource(string sourceID) => sources.FirstOrDefault(s => s.SourceID == sourceID);

        public Individual GetIndividual(string individualID)
        {
            //            return individuals.FirstOrDefault(i => i.IndividualID == individualID);
            if (string.IsNullOrEmpty(individualID))
                return null;
            individualLookup.TryGetValue(individualID, out Individual person);
            while (individualID.StartsWith("I0", StringComparison.Ordinal) && person is null)
            {
                if (individualID.Length >= 2) individualID = $"I{individualID.Substring(2)}";
                individualLookup.TryGetValue(individualID, out person);
            }
            return person;
        }

        public Family GetFamily(string familyID) => families.FirstOrDefault(f => f.FamilyID == familyID);

        public void AddSharedFact(string individual, Fact fact) => sharedFacts.Add(new Tuple<string, Fact>(individual, fact));

        public IEnumerable<Individual> GetIndividualsAtLocation(FactLocation loc, int level) => individuals.Filter(i => i.IsAtLocation(loc, level));

        public IEnumerable<Family> GetFamiliesAtLocation(FactLocation loc, int level) => families.Filter(f => f.IsAtLocation(loc, level));

        public int CountPeopleAtLocation(FactLocation loc, int level) => individuals.Filter(i => i.IsAtLocation(loc, level)).Count() + families.Filter(f => f.IsAtLocation(loc, level)).Count();

        public List<string> GetSurnamesAtLocation(FactLocation loc) { return GetSurnamesAtLocation(loc, FactLocation.SUBREGION); }
        public List<string> GetSurnamesAtLocation(FactLocation loc, int level)
        {
            List<string> result = new List<string>();
            foreach (Individual i in individuals)
            {
                if (!result.ContainsString(i.Surname) && i.IsAtLocation(loc, level))
                    result.Add(i.Surname);
            }
            result.Sort();
            return result;
        }

        void FixIDs()
        {
            int indLen = individuals.Count.ToString().Length;
            foreach (Individual ind in individuals)
            {
                ind.FixIndividualID(indLen);
                // If the individual id has been changed, the lookup needs to be updated
                if (!individualLookup.ContainsKey(ind.IndividualID))
                    individualLookup.Add(ind.IndividualID, ind);
            }
            int famLen = families.Count.ToString().Length;
            foreach (Family f in families)
                f.FixFamilyID(famLen);
            int sourceLen = sources.Count.ToString().Length;
            foreach (FactSource s in sources)
                s.FixSourceID(sourceLen);
        }

        public void SetFullNames()
        {
            foreach (Individual ind in individuals)
                ind.SetFullName();
        }
        #endregion

        #region Loose Info
        public SortableBindingList<IDisplayLooseInfo> LooseInfo()
        {
            if (looseInfo != null)
                return looseInfo;
            if (looseBirths is null)
                LooseBirths();
            if (looseDeaths is null)
                LooseDeaths();
            SortableBindingList<IDisplayLooseInfo> result = new SortableBindingList<IDisplayLooseInfo>();
            try
            {
                foreach (Individual ind in looseBirths)
                    result.Add(ind as IDisplayLooseInfo);
                foreach (Individual ind in looseDeaths)
                    result.Add(ind as IDisplayLooseInfo);
            }
            catch (Exception ex)
            {
                throw new LooseDataException($"Problem calculating Loose Info. Error was {ex.Message}");
            }
            looseInfo = result;
            return result;
        }
        #endregion

        #region Loose Births

        public SortableBindingList<IDisplayLooseBirth> LooseBirths()
        {
            if (looseBirths != null)
                return looseBirths;
            SortableBindingList<IDisplayLooseBirth> result = new SortableBindingList<IDisplayLooseBirth>();
            try
            {
                foreach (Individual ind in individuals)
                    CheckLooseBirth(ind, result);
            }
            catch (Exception ex)
            {
                throw new LooseDataException($"Problem calculating Loose Births. Error was {ex.Message}");
            }
            looseBirths = result;
            return result;
        }

        void CheckLooseBirth(Individual indiv, SortableBindingList<IDisplayLooseBirth> result = null)
        {
            FactDate birthDate = indiv.BirthDate;
            FactDate toAdd = null;
            if (birthDate.EndDate.Year - birthDate.StartDate.Year > 1)
            {
                FactDate baseDate = BaseLivingDate(indiv);
                DateTime minStart = baseDate.StartDate;
                DateTime minEnd = baseDate.EndDate;
                if (birthDate.EndDate != FactDate.MAXDATE && birthDate.EndDate > minEnd)
                {   // makes sure we use birth date end in event we have not enough facts
                    minEnd = birthDate.EndDate;
                    // don't think we should set the start date as max years before end as end may be wide range into future whereas start was calculated from facts
                    //if (minStart != FactDate.MINDATE && minEnd.Year > minStart.Year + FactDate.MAXYEARS)
                    //    minStart = CreateDate(minEnd.Year - FactDate.MAXYEARS, minStart.Month, minStart.Day); // min end mustn't be more than max years after start
                }
                foreach (Family fam in indiv.FamiliesAsSpouse)
                {
                    FactDate marriageDate = fam.GetPreferredFactDate(Fact.MARRIAGE);
                    if (marriageDate.StartDate.Year > GeneralSettings.Default.MinParentalAge && !marriageDate.IsLongYearSpan)
                    {  // set maximum birthdate as X years before earliest marriage
                        DateTime preMarriage = CreateDate(marriageDate.StartDate.Year - GeneralSettings.Default.MinParentalAge, 12, 31);
                        if (preMarriage < minEnd && preMarriage >= minStart)
                            minEnd = preMarriage;
                    }
                    if (fam.Children.Count > 0)
                    {   // must be at least X years old at birth of child
                        List<Individual> childrenNoAFT =
                            fam.Children.Filter(child => child.BirthDate.EndDate != FactDate.MAXDATE && !child.BirthDate.IsLongYearSpan).ToList();
                        if (childrenNoAFT.Count > 0)
                        {
                            int minChildYear = childrenNoAFT.Min(child => child.BirthDate.EndDate).Year;
                            DateTime minChild = CreateDate(minChildYear - GeneralSettings.Default.MinParentalAge, 12, 31);
                            if (minChild < minEnd && minChild >= minStart)
                                minEnd = minChild;
                        }
                        List<Individual> childrenNoBEF =
                            fam.Children.Filter(child => child.BirthDate.StartDate != FactDate.MINDATE && !child.BirthDate.IsLongYearSpan).ToList();
                        if (childrenNoBEF.Count > 0)
                        {
                            int maxChildYear = childrenNoBEF.Max(child => child.BirthDate.StartDate).Year;
                            DateTime maxChild;
                            if (indiv.IsMale) // for males check that not over 100 when oldest child is born
                                maxChild = CreateDate(maxChildYear - 100, 1, 1);
                            else // for females check that not over 60 when oldest child is born
                                maxChild = CreateDate(maxChildYear - 60, 1, 1);
                            if (maxChild > minStart)
                                minStart = maxChild;
                        }
                    }
                    Individual spouse = fam.Spouse(indiv);
                    if (spouse != null && spouse.DeathDate.IsKnown)
                    {
                        DateTime maxMarried = CreateDate(spouse.DeathEnd.Year - GeneralSettings.Default.MinParentalAge, 12, 31);
                        if (maxMarried < minEnd && maxMarried >= minStart)
                            minEnd = maxMarried;
                    }
                }
                foreach (ParentalRelationship parents in indiv.FamiliesAsChild)
                {  // check min date at least X years after parent born and no later than parent dies
                    Family fam = parents.Family;
                    if (fam.Husband != null)
                    {
                        if (fam.Husband.BirthDate.IsKnown && fam.Husband.BirthDate.StartDate != FactDate.MINDATE)
                            if (fam.Husband.BirthDate.StartDate.TryAddYears(GeneralSettings.Default.MinParentalAge) > minStart)
                                minStart = CreateDate(fam.Husband.BirthDate.StartDate.Year + GeneralSettings.Default.MinParentalAge, 1, 1);
                        if (fam.Husband.DeathDate.IsKnown && fam.Husband.DeathDate.EndDate != FactDate.MAXDATE)
                            if (fam.Husband.DeathDate.EndDate.Year != FactDate.MAXDATE.Year && fam.Husband.DeathDate.EndDate.AddMonths(9) < minEnd)
                                minEnd = CreateDate(fam.Husband.DeathDate.EndDate.AddMonths(9).Year, 1, 1);
                    }
                    if (fam.Wife != null)
                    {
                        if (fam.Wife.BirthDate.IsKnown && fam.Wife.BirthDate.StartDate != FactDate.MINDATE)
                            if (fam.Wife.BirthDate.StartDate.TryAddYears(GeneralSettings.Default.MinParentalAge) > minStart)
                                minStart = CreateDate(fam.Wife.BirthDate.StartDate.Year + GeneralSettings.Default.MinParentalAge, 1, 1);
                        if (fam.Wife.DeathDate.IsKnown && fam.Wife.DeathDate.EndDate != FactDate.MAXDATE)
                            if (fam.Wife.DeathDate.EndDate.Year != FactDate.MAXDATE.Year && fam.Wife.DeathDate.EndDate < minEnd)
                                minEnd = CreateDate(fam.Wife.DeathDate.EndDate.Year, 1, 1);
                    }
                }
                if (birthDate.EndDate <= minEnd && birthDate.EndDate != FactDate.MAXDATE)
                {  // check for BEF XXXX types that are prevalent in my tree
                    if (birthDate.StartDate == FactDate.MINDATE && birthDate.EndDate.TryAddYears(1) <= minEnd)
                        minEnd = birthDate.EndDate.TryAddYears(1);
                    else
                        minEnd = birthDate.EndDate;
                }
                if (birthDate.StartDate > minStart)
                    minStart = birthDate.StartDate;
                // force min & max years with odd dates to be min & max dates
                if (minEnd.Year == FactDate.MAXDATE.Year && minEnd != FactDate.MAXDATE)
                    minEnd = FactDate.MAXDATE;
                if (minStart.Year == 1 && minStart != FactDate.MINDATE)
                    minStart = FactDate.MINDATE;
                if (minEnd.Month == 1 && minEnd.Day == 1 && birthDate.EndDate.Month == 12 && birthDate.EndDate.Day == 31)
                    minEnd = minEnd.TryAddYears(1).AddDays(-1); // year has rounded to 1st Jan when was upper year.
                baseDate = new FactDate(minStart, minEnd);
                if (birthDate != baseDate)
                    toAdd = baseDate;
            }
            if (toAdd != null && toAdd != birthDate && toAdd.DistanceSquared(birthDate) > 1)
            {
                // we have a date to change and its not the same 
                // range as the existing death date
                Fact looseBirth = new Fact(indiv.IndividualID, Fact.LOOSEBIRTH, toAdd, FactLocation.UNKNOWN_LOCATION);
                indiv.AddFact(looseBirth);
                if (result != null)
                    result.Add(indiv);
            }
        }

        DateTime CreateDate(int year, int month, int day)
        {
            if (year > DateTime.MaxValue.Year)
                year = DateTime.MaxValue.Year;
            if (year < 1)
                year = 1;
            if (month > 12)
                month = 12;
            if (month < 1)
                month = 1;
            if (month == 2 && day == 29 & !DateTime.IsLeapYear(year))
                day = 28;
            return new DateTime(year, month, day);
        }

        FactDate BaseLivingDate(Individual indiv)
        {
            DateTime mindate = FactDate.MAXDATE;
            DateTime maxdate = GetMaxLivingDate(indiv, Fact.LOOSE_BIRTH_FACTS);
            DateTime startdate = maxdate.Year < FactDate.MAXYEARS ? FactDate.MINDATE : CreateDate(maxdate.Year - FactDate.MAXYEARS, 1, 1);
            foreach (Fact f in indiv.AllFacts)
            {
                if (Fact.LOOSE_BIRTH_FACTS.Contains(f.FactType))
                {
                    if (f.FactDate.IsKnown && (!Fact.IGNORE_LONG_RANGE.Contains(f.FactType) || !f.FactDate.IsLongYearSpan))
                    {  // don't consider long year span marriage or children facts
                        if (f.FactDate.StartDate != FactDate.MINDATE && f.FactDate.StartDate < mindate)
                            mindate = f.FactDate.StartDate;
                        if (f.FactDate.EndDate != FactDate.MAXDATE && f.FactDate.EndDate < mindate) //copes with BEF dates
                            mindate = f.FactDate.EndDate;
                    }
                }
            }
            if (startdate.Year != 1 && startdate.Year != FactDate.MAXDATE.Year && startdate < mindate)
                return new FactDate(startdate, mindate);
            if (mindate.Year != 1 && mindate.Year != FactDate.MAXDATE.Year && mindate <= maxdate)
                return new FactDate(mindate, maxdate);
            return FactDate.UNKNOWN_DATE;
        }

        #endregion

        #region Loose Deaths

        public SortableBindingList<IDisplayLooseDeath> LooseDeaths()
        {
            if (looseDeaths != null)
                return looseDeaths;
            SortableBindingList<IDisplayLooseDeath> result = new SortableBindingList<IDisplayLooseDeath>();
            try
            {
                foreach (Individual ind in individuals)
                    CheckLooseDeath(ind, result);
            }
            catch (Exception ex)
            {
                throw new LooseDataException($"Problem calculating Loose Deaths. Error was {ex.Message}");
            }
            looseDeaths = result;
            return result;
        }

        void CheckLooseDeath(Individual indiv, SortableBindingList<IDisplayLooseDeath> result = null)
        {
            FactDate deathDate = indiv.DeathDate;
            FactDate toAdd = null;
            if (deathDate.EndDate.Year - deathDate.StartDate.Year > 1)
            {
                DateTime maxLiving = GetMaxLivingDate(indiv, Fact.LOOSE_DEATH_FACTS);
                DateTime minDeath = GetMinDeathDate(indiv);
                if (minDeath != FactDate.MAXDATE)
                {   // we don't have a minimum death date so can't proceed - individual may still be alive
                    if (maxLiving > deathDate.StartDate)
                    {
                        // the starting death date is before the last alive date
                        // so add to the list of loose deaths
                        if (minDeath < deathDate.EndDate)
                            toAdd = new FactDate(maxLiving, minDeath);
                        else if (deathDate.DateType == FactDate.FactDateType.BEF && minDeath != FactDate.MAXDATE
                              && deathDate.EndDate != FactDate.MAXDATE
                              && deathDate.EndDate.TryAddYears(1) == minDeath)
                            toAdd = new FactDate(maxLiving, minDeath);
                        else
                            toAdd = new FactDate(maxLiving, deathDate.EndDate);
                    }
                    else if (minDeath < deathDate.EndDate)
                    {
                        // earliest death date before current latest death
                        // or they were two BEF dates 
                        // so add to the list of loose deaths
                        toAdd = new FactDate(deathDate.StartDate, minDeath);
                    }
                }
            }
            if (toAdd != null && toAdd != deathDate && toAdd.DistanceSquared(deathDate) > 1)
            {
                // we have a date to change and its not the same 
                // range as the existing death date
                Fact looseDeath = new Fact(indiv.IndividualID, Fact.LOOSEDEATH, toAdd, FactLocation.UNKNOWN_LOCATION);
                indiv.AddFact(looseDeath);
                if (result != null)
                    result.Add(indiv);
            }
        }

        DateTime GetMaxLivingDate(Individual indiv, ISet<string> factTypes)
        {
            DateTime maxdate = FactDate.MINDATE;
            // having got the families the individual is a parent of
            // get the max startdate of the birthdate of the youngest child
            // this then is the minimum point they were alive
            // subtract 9 months for a male
            bool childDate = false;
            foreach (Family fam in indiv.FamiliesAsSpouse)
            {
                FactDate marriageDate = fam.GetPreferredFactDate(Fact.MARRIAGE);
                if (marriageDate.StartDate > maxdate && !marriageDate.IsLongYearSpan)
                    maxdate = marriageDate.StartDate;
                List<Individual> childrenNoLongSpan = fam.Children.Filter(child => !child.BirthDate.IsLongYearSpan).ToList<Individual>();
                if (childrenNoLongSpan.Count > 0)
                {
                    DateTime maxChildBirthDate = childrenNoLongSpan.Max(child => child.BirthDate.StartDate);
                    if (maxChildBirthDate > maxdate)
                    {
                        maxdate = maxChildBirthDate;
                        childDate = true;
                    }
                }
            }
            if (childDate && indiv.IsMale && maxdate > FactDate.MINDATE.AddMonths(9))
            {
                // set to 9 months before birth if indiv is a father 
                // and we have changed maxdate from the MINDATE default
                // and the date is derived from a child not a marriage
                maxdate = maxdate.AddMonths(-9);
                // now set to Jan 1 of that year 9 months before birth to prevent 
                // very exact 9 months before dates
                maxdate = CreateDate(maxdate.Year, 1, 1);
            }
            // Check max date on all facts of facttype but don't consider long year span marriage or children facts
            foreach (Fact f in indiv.AllFacts)
                if (factTypes.Contains(f.FactType) && f.FactDate.StartDate > maxdate && (!Fact.IGNORE_LONG_RANGE.Contains(f.FactType) || !f.FactDate.IsLongYearSpan))
                    maxdate = f.FactDate.StartDate;
            // at this point we have the maximum point a person was alive
            // based on their oldest child and last living fact record and marriage date
            return maxdate;
        }

        DateTime GetMinDeathDate(Individual indiv)
        {
            FactDate deathDate = indiv.DeathDate;
            FactDate.FactDateType deathDateType = deathDate.DateType;
            FactDate.FactDateType birthDateType = indiv.BirthDate.DateType;
            DateTime minDeath = FactDate.MAXDATE;
            if (indiv.BirthDate.IsKnown && indiv.BirthDate.EndDate.Year < 9999) // filter out births where no year specified
            {
                minDeath = CreateDate(indiv.BirthDate.EndDate.Year + FactDate.MAXYEARS, 12, 31);
                if (birthDateType == FactDate.FactDateType.BEF)
                    minDeath = minDeath.TryAddYears(1);
                if (minDeath > FactDate.NOW) // 110 years after birth is after todays date so we set to ignore
                    minDeath = FactDate.MAXDATE;
            }
            FactDate burialDate = indiv.GetPreferredFactDate(Fact.BURIAL);
            if (burialDate.EndDate < minDeath)
                minDeath = burialDate.EndDate;
            if (minDeath <= deathDate.EndDate)
                return minDeath;
            if (deathDateType == FactDate.FactDateType.BEF && minDeath != FactDate.MAXDATE)
                return minDeath;
            return deathDate.EndDate;
        }

        //TODO Check Loose Marriage
        //void CheckLooseMarriage(Individual ind)
        //{

        //}

        #endregion

        #region Relationship Functions
        void ClearRelations()
        {
            foreach (Individual i in individuals)
            {
                i.RelationType = Individual.UNKNOWN;
                i.BudgieCode = string.Empty;
                i.Ahnentafel = 0;
                i.CommonAncestor = null;
                i.RelationToRoot = string.Empty;
            }
        }

        void AddToQueue(Queue<Individual> queue, IEnumerable<Individual> list)
        {
            foreach (Individual i in list)
                queue.Enqueue(i);
        }

        void AddDirectParentsToQueue(Individual indiv, Queue<Individual> queue, IProgress<string> outputText)
        {
            foreach (ParentalRelationship parents in indiv.FamiliesAsChild)
            {
                Family family = parents.Family;
                // add parents to queue
                if (family.Husband != null && parents.IsNaturalFather && indiv.RelationType == Individual.DIRECT)
                {
                    BigInteger newAhnentafel = indiv.Ahnentafel * 2;
                    if (family.Husband.RelationType != Individual.UNKNOWN && family.Husband.Ahnentafel != newAhnentafel)
                        AlreadyDirect(family.Husband, newAhnentafel, outputText);
                    else
                    {
                        family.Husband.Ahnentafel = newAhnentafel;
                        if (family.Husband.Ahnentafel > maxAhnentafel)
                            maxAhnentafel = family.Husband.Ahnentafel;
                        queue.Enqueue(family.Husband); // add to directs queue only if natural father of direct
                    }
                }

                if (family.Wife != null && parents.IsNaturalMother && indiv.RelationType == Individual.DIRECT)
                {
                    BigInteger newAhnentafel = indiv.Ahnentafel * 2 + 1;
                    if (family.Wife.RelationType != Individual.UNKNOWN && family.Wife.Ahnentafel != newAhnentafel)
                        AlreadyDirect(family.Wife, newAhnentafel, outputText);
                    else
                    {
                        family.Wife.Ahnentafel = newAhnentafel;
                        if (family.Wife.Ahnentafel > maxAhnentafel)
                            maxAhnentafel = family.Wife.Ahnentafel;
                        queue.Enqueue(family.Wife); // add to directs queue only if natural mother of direct
                    }
                }
            }
        }

        void AlreadyDirect(Individual parent, BigInteger newAhnentafel, IProgress<string> outputText)
        {
            if (GeneralSettings.Default.ShowMultiAncestors)
            {
                // Hmm interesting a direct parent who is already a direct
                //string currentRelationship = Relationship.CalculateRelationship(RootPerson, parent);
                string currentLine = Relationship.AhnentafelToString(parent.Ahnentafel);
                string newLine = Relationship.AhnentafelToString(newAhnentafel);
                if (parent.Ahnentafel > newAhnentafel)
                    parent.Ahnentafel = newAhnentafel; // set to lower line if new direct
                if (outputText != null)
                {
                    outputText.Report($"{parent.Name} detected as a direct ancestor more than once as:\n");
                    outputText.Report($"{currentLine} and as:\n");
                    outputText.Report($"{newLine}\n\n");
                }
            }
        }

        void AddParentsToQueue(Individual indiv, Queue<Individual> queue)
        {
            foreach (ParentalRelationship parents in indiv.FamiliesAsChild)
            {
                Family family = parents.Family;
                // add parents to queue
                if (family?.Husband?.RelationType == Individual.UNKNOWN)
                    queue.Enqueue(family.Husband);
                if (family?.Wife?.RelationType == Individual.UNKNOWN)
                    queue.Enqueue(family.Wife);
            }
        }

        //void AddChildrenToQueue(Individual indiv, Queue<Individual> queue, bool isRootPerson)
        //{
        //    IEnumerable<Family> parentFamilies = indiv.FamiliesAsSpouse;
        //    foreach (Family family in parentFamilies)
        //    {
        //        foreach (Individual child in family.Children)
        //        {
        //            // add child to queue
        //            if (child.RelationType == Individual.BLOOD || child.RelationType == Individual.UNKNOWN)
        //            {
        //                child.RelationType = Individual.BLOOD;
        //                child.Ahnentafel = isRootPerson ? indiv.Ahnentafel - 2 : indiv.Ahnentafel - 1;
        //                child.BudgieCode = $"-{child.Ahnentafel.ToString().PadLeft(2, '0')}c";
        //                queue.Enqueue(child);
        //            }
        //        }
        //        family.SetBudgieCode(indiv, 2);
        //    }
        //}

        public Individual RootPerson { get; set; }

        public void SetRelations(string startID, IProgress<string> outputText)
        {
            ClearRelations();
            RootPerson = GetIndividual(startID);
            if (RootPerson is null)
            {
                startID = individuals[0].IndividualID;
                RootPerson = GetIndividual(startID);
                if (RootPerson is null)
                    throw new NotFoundException("Unable to find a Root Person in the file");
            }
            Individual ind = RootPerson;
            ind.RelationType = Individual.DIRECT;
            ind.Ahnentafel = 1;
            maxAhnentafel = 1;
            var queue = new Queue<Individual>();
            queue.Enqueue(RootPerson);
            while (queue.Count > 0)
            {
                // now take an item from the queue
                ind = queue.Dequeue();
                // set them as a direct relation
                ind.RelationType = Individual.DIRECT;
                ind.CommonAncestor = new CommonAncestor(ind, 0, false); // set direct as common ancestor
                AddDirectParentsToQueue(ind, queue, outputText);
            }
            int lenAhnentafel = maxAhnentafel.ToString().Length;
            // we have now added all direct ancestors
            IEnumerable<Individual> directs = GetAllRelationsOfType(Individual.DIRECT);
            // add all direct ancestors budgie codes
            foreach (Individual i in directs)
                i.BudgieCode = (i.Ahnentafel).ToString().PadLeft(lenAhnentafel, '0') + "d";
            AddToQueue(queue, directs);
            while (queue.Count > 0)
            {
                // get the next person
                ind = queue.Dequeue();
                var parentFamilies = ind.FamiliesAsSpouse;
                foreach (Family family in parentFamilies)
                {
                    // if the spouse of a direct ancestor is not a direct
                    // ancestor then they are only related by marriage
                    family.SetSpouseRelation(ind, Individual.MARRIEDTODB);
                    // all children of direct ancestors and blood relations
                    // are blood relations
                    family.SetChildRelation(queue, Individual.BLOOD);
                    family.SetChildrenCommonRelation(ind, ind.CommonAncestor);
                    family.SetBudgieCode(ind, lenAhnentafel);
                }
            }
            // we have now set all direct ancestors and all blood relations
            // now is to loop through the marriage relations
            IEnumerable<Individual> marriedDBs = GetAllRelationsOfType(Individual.MARRIEDTODB);
            AddToQueue(queue, marriedDBs);
            while (queue.Count > 0)
            {
                // get the next person
                ind = queue.Dequeue();
                // first only process this individual if they are related by marriage or still unknown
                int relationship = ind.RelationType;
                if (relationship == Individual.MARRIAGE ||
                    relationship == Individual.MARRIEDTODB ||
                    relationship == Individual.UNKNOWN)
                {
                    // set this individual to be related by marriage
                    if (relationship == Individual.UNKNOWN)
                        ind.RelationType = Individual.MARRIAGE;
                    AddParentsToQueue(ind, queue);
                    IEnumerable<Family> parentFamilies = ind.FamiliesAsSpouse;
                    foreach (Family family in parentFamilies)
                    {
                        family.SetSpouseRelation(ind, Individual.MARRIAGE);
                        // children of relatives by marriage that we haven't previously 
                        // identified are also relatives by marriage
                        family.SetChildRelation(queue, Individual.MARRIAGE);
                    }
                }
            }
            // now anyone linked is set
            bool keepLooping = true;
            while (keepLooping)
            {
                keepLooping = false;
                IEnumerable<Family> families = AllFamilies.Where(f => f.HasUnknownRelations && f.HasLinkedRelations);
                foreach (Family f in families)
                {
                    foreach (Individual i in f.Members)
                    {
                        if (i.RelationType == Individual.UNKNOWN)
                        {
                            i.RelationType = Individual.LINKED;
                            keepLooping = true; // keep going if we set an individual
                        }
                    }
                }
            }
        }

        void SetRelationDescriptions(string startID)
        {
            IEnumerable<Individual> directs = GetAllRelationsOfType(Individual.DIRECT);
            IEnumerable<Individual> blood = GetAllRelationsOfType(Individual.BLOOD);
            IEnumerable<Individual> married = GetAllRelationsOfType(Individual.MARRIEDTODB);
            Individual rootPerson = GetIndividual(startID);
            foreach (Individual i in directs)
                i.RelationToRoot = Relationship.CalculateRelationship(rootPerson, i);
            foreach (Individual i in blood)
                i.RelationToRoot = Relationship.CalculateRelationship(rootPerson, i);
            foreach (Individual i in married)
            {
                foreach (Family f in i.FamiliesAsSpouse)
                {
                    if (i.RelationToRoot is null && f.Spouse(i) != null && f.Spouse(i).IsBloodDirect)
                    {
                        string relation = f.MaritalStatus != Family.MARRIED ? "partner" : i.IsMale ? "husband" : "wife";
                        i.RelationToRoot = $"{relation} of {f.Spouse(i).RelationToRoot}";
                        break;
                    }
                }
            }
        }

        public string PrintRelationCount()
        {
            StringBuilder sb = new StringBuilder();
            int[] relations = new int[Individual.UNSET + 1];
            foreach (Individual i in individuals)
                relations[i.RelationType]++;
            sb.Append($"Direct Ancestors: {relations[Individual.DIRECT]}\n");
            sb.Append($"Descendants: {relations[Individual.DESCENDANT]}\n");
            sb.Append($"Blood Relations: {relations[Individual.BLOOD]}\n");
            sb.Append($"Married to Blood or Direct Relation: {relations[Individual.MARRIEDTODB]}\n");
            sb.Append($"Related by Marriage: {relations[Individual.MARRIAGE]}\n");
            sb.Append($"Linked through Marriages: {relations[Individual.LINKED]}\n");
            sb.Append($"Unknown relation: {relations[Individual.UNKNOWN]}\n");
            if (relations[Individual.UNSET] > 0)
                sb.Append($"Failed to set relationship: {relations[Individual.UNSET]}\n");
            sb.Append('\n');
            return sb.ToString();
        }

        public IEnumerable<Individual> DirectLineIndividuals => AllIndividuals.Filter(i => i.RelationType == Individual.DIRECT || i.RelationType == Individual.DESCENDANT);

        #endregion

        #region Displays
        public IEnumerable<CensusFamily> GetAllCensusFamilies(CensusDate censusDate, bool censusDone, bool checkCensus)
        {
            if (censusDate != null)
            {
                HashSet<string> individualIDs = new HashSet<string>();
                foreach (Family f in families)
                {
                    CensusFamily cf = new CensusFamily(f, censusDate);
                    if (cf.Process(censusDate, censusDone, checkCensus))
                    {
                        individualIDs.UnionWith(cf.Members.Select(x => x.IndividualID));
                        yield return cf;
                    }
                }
                // also add all individuals that don't ever appear as a child as they may have census facts for when they are children
                foreach (Individual ind in individuals.Filter(x => x.FamiliesAsChild.Count == 0))
                {
                    CensusFamily cf = new CensusFamily(new Family(ind, Instance.NextPreMarriageFamily), censusDate);
                    if (!individualIDs.Contains(ind.IndividualID) && cf.Process(censusDate, censusDone, checkCensus))
                    {
                        individualIDs.Add(ind.IndividualID);
                        yield return cf;
                    }
                }
            }
        }

        public void ClearLocations()
        {
            for (int i = 0; i < 5; i++)
                displayLocations[i] = null;
        }

        SortableBindingList<IDisplayLocation> GetDisplayLocations(int level)
        {
            List<IDisplayLocation> result = new List<IDisplayLocation>();
            //copy to list so that any GetLocation(level) that creates a new location 
            //won't cause an error due to collection changing
            List<FactLocation> allLocations = FactLocation.AllLocations.ToList();
            foreach (FactLocation loc in allLocations)
            {
                FactLocation c = loc.GetLocation(level);
                if (!c.IsBlank && !result.ContainsLocation(c))
                    result.Add(c);
            }
            result.Sort(new FactLocationComparer(level));
            displayLocations[level] = new SortableBindingList<IDisplayLocation>(result);
            return displayLocations[level];
        }

        public SortableBindingList<IDisplayLocation> AllDisplayCountries => displayLocations[FactLocation.COUNTRY] ?? GetDisplayLocations(FactLocation.COUNTRY);

        public SortableBindingList<IDisplayLocation> AllDisplayRegions => displayLocations[FactLocation.REGION] ?? GetDisplayLocations(FactLocation.REGION);

        public SortableBindingList<IDisplayLocation> AllDisplaySubRegions => displayLocations[FactLocation.SUBREGION] ?? GetDisplayLocations(FactLocation.SUBREGION);

        public SortableBindingList<IDisplayLocation> AllDisplayAddresses => displayLocations[FactLocation.ADDRESS] ?? GetDisplayLocations(FactLocation.ADDRESS);

        public SortableBindingList<IDisplayLocation> AllDisplayPlaces => displayLocations[FactLocation.PLACE] ?? GetDisplayLocations(FactLocation.PLACE);

        public List<IDisplayGeocodedLocation> AllGeocodingLocations
        {
            get
            {
                List<IDisplayGeocodedLocation> result = new List<IDisplayGeocodedLocation>();
                foreach (IDisplayGeocodedLocation loc in FactLocation.AllLocations)
                    if ((loc as FactLocation).IsKnown)
                        result.Add(loc);
                return result;
            }
        }

        public SortableBindingList<IDisplayIndividual> AllDisplayIndividuals
        {
            get
            {
                SortableBindingList<IDisplayIndividual> result = new SortableBindingList<IDisplayIndividual>();
                foreach (IDisplayIndividual i in individuals)
                    result.Add(i);
                return result;
            }
        }

        public SortableBindingList<IDisplayFamily> AllDisplayFamilies
        {
            get
            {
                SortableBindingList<IDisplayFamily> result = new SortableBindingList<IDisplayFamily>();
                foreach (IDisplayFamily f in families)
                    result.Add(f);
                return result;
            }
        }

        public static SortableBindingList<IDisplayFact> GetSourceDisplayFacts(FactSource source)
        {
            SortableBindingList<IDisplayFact> result = new SortableBindingList<IDisplayFact>();
            foreach (Fact f in source.Facts)
            {
                if (f.Individual != null)
                {
                    DisplayFact df = new DisplayFact(f.Individual, f);
                    if (!result.ContainsFact(df))
                        result.Add(df);
                }
                else
                {
                    if (f.Family != null && f.Family.Husband != null)
                    {
                        DisplayFact df = new DisplayFact(f.Family.Husband, f);
                        if (!result.ContainsFact(df))
                            result.Add(df);
                    }
                    if (f.Family != null && f.Family.Wife != null)
                    {
                        DisplayFact df = new DisplayFact(f.Family.Wife, f);
                        if (!result.ContainsFact(df))
                            result.Add(df);
                    }
                }
            }
            return result;
        }

        public SortableBindingList<IDisplaySource> AllDisplaySources
        {
            get
            {
                var result = new SortableBindingList<IDisplaySource>();
                foreach (IDisplaySource s in sources)
                    result.Add(s);
                return result;
            }
        }

        public SortableBindingList<IDisplayOccupation> AllDisplayOccupations
        {
            get
            {
                var result = new SortableBindingList<IDisplayOccupation>();
                foreach (string occ in occupations.Keys)
                    result.Add(new DisplayOccupation(occ, occupations[occ].Count));
                return result;
            }
        }
        public SortableBindingList<IDisplayCustomFact> AllCustomFacts
        {
            get
            {
                var result = new SortableBindingList<IDisplayCustomFact>();
                foreach (string facttype in unknownFactTypes.Keys)
                {
                    bool ignore = DatabaseHelper.IgnoreCustomFact(facttype);
                    var customFact = new DisplayCustomFact(facttype, unknownFactTypes[facttype].Count, ignore);
                    result.Add(customFact);
                }
                return result;
            }
        }

        public SortableBindingList<IDisplayFact> AllDisplayFacts
        {
            get
            {
                SortableBindingList<IDisplayFact> result = new SortableBindingList<IDisplayFact>();

                foreach (Individual ind in individuals)
                {
                    foreach (Fact f in ind.PersonalFacts)
                        result.Add(new DisplayFact(ind, f));
                    foreach (Family fam in ind.FamiliesAsSpouse)
                        foreach (Fact famfact in fam.Facts)
                            result.Add(new DisplayFact(ind, famfact));
                }
                return result;
            }
        }

        public SortableBindingList<Individual> AllWorkers(string job) => new SortableBindingList<Individual>(occupations[job]);

        public SortableBindingList<Individual> AllCustomFactIndividuals(string factType) =>
            new SortableBindingList<Individual>(unknownFactTypes[factType]);

        public SortableBindingList<IDisplayFamily> PossiblyMissingChildFamilies
        {
            get
            {
                SortableBindingList<IDisplayFamily> result = new SortableBindingList<IDisplayFamily>();
                foreach (Family fam in families)
                    if (fam.EldestChild != null && fam.MarriageDate.IsKnown && fam.EldestChild.BirthDate.IsKnown &&
                      !fam.EldestChild.BirthDate.IsLongYearSpan && fam.EldestChild.BirthDate.BestYear > fam.MarriageDate.BestYear + 3)
                        result.Add(fam);
                return result;
            }
        }

        public SortableBindingList<IDisplayFamily> SingleFamilies
        {
            get
            {
                SortableBindingList<IDisplayFamily> result = new SortableBindingList<IDisplayFamily>();
                foreach (Family fam in families)
                    if (fam.FamilyType != Family.SOLOINDIVIDUAL && (fam.Husband is null || fam.Wife is null))
                        result.Add(fam);
                return result;
            }
        }

        public SortableBindingList<IDisplayIndividual> AgedOver99
        {
            get
            {
                SortableBindingList<IDisplayIndividual> result = new SortableBindingList<IDisplayIndividual>();
                foreach (Individual ind in individuals)
                {
                    int age = ind.GetMaxAge(FactDate.TODAY);
                    Console.WriteLine($"\nName: {ind.Name}: b.{ind.BirthDate} d.{ind.DeathDate} max age={age}");
                    if (ind.DeathDate.IsUnknown && age >= 99)
                        result.Add(ind);
                }
                return result;
            }
        }

        public List<IDisplayColourCensus> ColourCensus(string country, RelationTypes relType, string surname,
                                                       ComboBoxFamily family, bool IgnoreMissingBirthDates, bool IgnoreMissingDeathDates)
        {
            Predicate<Individual> filter;
            if (family is null)
            {
                filter = relType.BuildFilter<Individual>(x => x.RelationType);
                if (surname.Length > 0)
                {
                    Predicate<Individual> surnameFilter = FilterUtils.StringFilter<Individual>(x => x.Surname, surname);
                    filter = FilterUtils.AndFilter(filter, surnameFilter);
                }
                Predicate<Individual> dateFilter;
                if (country.Equals(Countries.UNITED_STATES))
                    dateFilter = i => (i.BirthDate.StartsBefore(CensusDate.USCENSUS1950) || i.BirthDate.IsUnknown) &&
                                      (i.DeathDate.EndsAfter(CensusDate.USCENSUS1790) || i.DeathDate.IsUnknown) &&
                                      (i.BirthDate.IsKnown || !IgnoreMissingBirthDates) &&
                                      (i.DeathDate.IsKnown || !IgnoreMissingDeathDates);
                else if (country.Equals(Countries.CANADA))
                    dateFilter = i => (i.BirthDate.StartsBefore(CensusDate.CANADACENSUS1921) || i.BirthDate.IsUnknown) &&
                                      (i.DeathDate.EndsAfter(CensusDate.CANADACENSUS1851) || i.DeathDate.IsUnknown) &&
                                      (i.BirthDate.IsKnown || !IgnoreMissingBirthDates) &&
                                      (i.DeathDate.IsKnown || !IgnoreMissingDeathDates);
                else if (country.Equals(Countries.IRELAND))
                    dateFilter = i => (i.BirthDate.StartsBefore(CensusDate.IRELANDCENSUS1911) || i.BirthDate.IsUnknown) &&
                                      (i.DeathDate.EndsAfter(CensusDate.IRELANDCENSUS1901) || i.DeathDate.IsUnknown) &&
                                      (i.BirthDate.IsKnown || !IgnoreMissingBirthDates) &&
                                      (i.DeathDate.IsKnown || !IgnoreMissingDeathDates);
                else
                    dateFilter = i => (i.BirthDate.StartsBefore(CensusDate.UKCENSUS1939) || i.BirthDate.IsUnknown) &&
                                      (i.DeathDate.EndsAfter(CensusDate.UKCENSUS1841) || i.DeathDate.IsUnknown) &&
                                      (i.BirthDate.IsKnown || !IgnoreMissingBirthDates) &&
                                      (i.DeathDate.IsKnown || !IgnoreMissingDeathDates);
                filter = FilterUtils.AndFilter(filter, dateFilter);
                filter = FilterUtils.AndFilter(filter, x => x.AliveOnAnyCensus(country) && !x.OutOfCountryOnAllCensus(country));
            }
            else
                filter = x => family.Members.Contains(x);
            return individuals.Filter(filter).ToList<IDisplayColourCensus>();
        }

        public List<IDisplayColourBMD> ColourBMD(RelationTypes relType, string surname, ComboBoxFamily family)
        {
            Predicate<Individual> filter;
            if (family is null)
            {
                filter = relType.BuildFilter<Individual>(x => x.RelationType);
                if (surname.Length > 0)
                {
                    Predicate<Individual> surnameFilter = FilterUtils.StringFilter<Individual>(x => x.Surname, surname);
                    filter = FilterUtils.AndFilter(filter, surnameFilter);
                }
            }
            else
                filter = x => family.Members.Contains(x);
            return individuals.Filter(filter).ToList<IDisplayColourBMD>();
        }

        public List<IDisplayMissingData> MissingData(RelationTypes relType, string surname, ComboBoxFamily family)
        {
            Predicate<Individual> filter;
            if (family is null)
            {
                filter = relType.BuildFilter<Individual>(x => x.RelationType);
                if (surname.Length > 0)
                {
                    Predicate<Individual> surnameFilter = FilterUtils.StringFilter<Individual>(x => x.Surname, surname);
                    filter = FilterUtils.AndFilter(filter, surnameFilter);
                }
            }
            else
                filter = x => family.Members.Contains(x);
            return individuals.Filter(filter).ToList<IDisplayMissingData>();
        }
        #endregion

        #region Data Errors

        void SetDataErrorTypes(IProgress<int> progress)
        {
            int catchCount = 0;
            int totalRecords = (individuals.Count + families.Count) / 40 + 1; //only count for 40% of progressbar
            int record = 0;
            DataErrorTypes = new List<DataErrorGroup>();
            List<DataError>[] errors = new List<DataError>[DATA_ERROR_GROUPS];
            for (int i = 0; i < DATA_ERROR_GROUPS; i++)
                errors[i] = new List<DataError>();
            // calculate error lists
            #region Individual Fact Errors
            foreach (Individual ind in AllIndividuals)
            {
                progress.Report(20 + (record++ / totalRecords));
                try
                {
                    if (ind.BaptismDate is object && ind.BaptismDate.IsKnown)
                    {
                        if (ind.BirthDate.IsAfter(ind.BaptismDate))
                        {   // if birthdate after baptism and not an approx date
                            if (ind.Birth != ColourValues.BMDColours.APPROX_DATE)
                                errors[(int)Dataerror.BIRTH_AFTER_BAPTISM].Add(new DataError((int)Dataerror.BIRTH_AFTER_BAPTISM, ind, $"Baptised/Christened {ind.BaptismDate} before born {ind.BirthDate}"));
                            else
                            {   // if it is an approx birthdate only show as error if 4 months after birthdate to fudge for quarter days
                                if (ind.BirthDate.SubtractMonths(4).IsAfter(ind.BaptismDate))
                                    errors[(int)Dataerror.BIRTH_AFTER_BAPTISM].Add(new DataError((int)Dataerror.BIRTH_AFTER_BAPTISM, ind, $"Baptised/Christened {ind.BaptismDate} before born {ind.BirthDate}"));
                            }
                        }
                    }
                    #region Death facts
                    if (ind.DeathDate.IsKnown)
                    {
                        if (ind.BirthDate.IsAfter(ind.DeathDate))
                            errors[(int)Dataerror.BIRTH_AFTER_DEATH].Add(new DataError((int)Dataerror.BIRTH_AFTER_DEATH, ind, $"Died {ind.DeathDate} before born {ind.BirthDate}"));
                        if (ind.BurialDate != null && ind.BirthDate.IsAfter(ind.BurialDate))
                            errors[(int)Dataerror.BIRTH_AFTER_DEATH].Add(new DataError((int)Dataerror.BIRTH_AFTER_DEATH, ind, $"Buried {ind.BurialDate} before born {ind.BirthDate}"));
                        if (ind.BurialDate != null && ind.BurialDate.IsBefore(ind.DeathDate) && !ind.BurialDate.Overlaps(ind.DeathDate))
                            errors[(int)Dataerror.BURIAL_BEFORE_DEATH].Add(new DataError((int)Dataerror.BURIAL_BEFORE_DEATH, ind, $"Buried {ind.BurialDate} before died {ind.DeathDate}"));
                        int minAge = ind.GetMinAge(ind.DeathDate);
                        if (minAge > FactDate.MAXYEARS)
                            errors[(int)Dataerror.AGED_MORE_THAN_110].Add(new DataError((int)Dataerror.AGED_MORE_THAN_110, ind, $"Aged over {FactDate.MAXYEARS} before died {ind.DeathDate}"));
                        if (ind.IsFlaggedAsLiving)
                            errors[(int)Dataerror.LIVING_WITH_DEATH_DATE].Add(new DataError((int)Dataerror.LIVING_WITH_DEATH_DATE, ind, $"Flagged as living but has death date of {ind.DeathDate}"));
                    }
                    #endregion
                    #region Error facts
                    foreach (Fact f in ind.ErrorFacts)
                    {
                        bool added = false;
                        if (f.FactErrorNumber != 0)
                        {
                            errors[f.FactErrorNumber].Add(
                                new DataError(f.FactErrorNumber, ind, f.FactErrorMessage));
                            added = true;
                        }
                        else if (f.FactType == Fact.LOSTCOUSINS || f.FactType == Fact.LC_FTA)
                        {
                            if (!CensusDate.IsCensusYear(f.FactDate, f.Country, false))
                            {
                                errors[(int)Dataerror.LOST_COUSINS_NON_CENSUS].Add(
                                    new DataError((int)Dataerror.LOST_COUSINS_NON_CENSUS, ind, $"Lost Cousins event for {f.FactDate} which isn't a census year"));
                                added = true;
                            }
                            else if (!CensusDate.IsLostCousinsCensusYear(f.FactDate, false))
                            {
                                errors[(int)Dataerror.LOST_COUSINS_NOT_SUPPORTED_YEAR].Add(
                                    new DataError((int)Dataerror.LOST_COUSINS_NOT_SUPPORTED_YEAR, ind, $"Lost Cousins event for {f.FactDate} which isn't a Lost Cousins census year"));
                                added = true;
                            }
                        }
                        else if (f.IsCensusFact)
                        {
                            string comment = f.FactType == Fact.CENSUS ? "Census date " : "Residence date ";
                            if (f.FactDate.IsUnknown)
                            {
                                errors[(int)Dataerror.CENSUS_COVERAGE].Add(
                                        new DataError((int)Dataerror.CENSUS_COVERAGE, ind, $"{comment} is blank."));
                                added = true;
                            }
                            else
                            {
                                TimeSpan ts = f.FactDate.EndDate - f.FactDate.StartDate;
                                if (ts.Days > 3650)
                                {
                                    errors[(int)Dataerror.CENSUS_COVERAGE].Add(
                                        new DataError((int)Dataerror.CENSUS_COVERAGE, ind, $"{comment} {f.FactDate} covers more than one census event."));
                                    added = true;
                                }
                            }
                        }
                        if (f.FactErrorLevel == Fact.FactError.WARNINGALLOW && f.FactType == Fact.RESIDENCE)
                        {
                            errors[(int)Dataerror.RESIDENCE_CENSUS_DATE].Add(
                                    new DataError((int)Dataerror.RESIDENCE_CENSUS_DATE, f.FactErrorLevel, ind, f.FactErrorMessage));
                            added = true;
                        }
                        if (!added)
                            errors[(int)Dataerror.FACT_ERROR].Add(new DataError((int)Dataerror.FACT_ERROR, f.FactErrorLevel, ind, f.FactErrorMessage));
                    }
                    #endregion
                    #region All Facts
                    foreach (Fact f in ind.AllFacts)
                    {
                        if (f.FactDate.IsAfter(FactDate.TODAY))
                            errors[(int)Dataerror.FACT_IN_FUTURE].Add(
                                new DataError((int)Dataerror.FACT_IN_FUTURE, ind, $"{f} is in the future."));
                        if (FactBeforeBirth(ind, f))
                            errors[(int)Dataerror.FACTS_BEFORE_BIRTH].Add(
                                new DataError((int)Dataerror.FACTS_BEFORE_BIRTH, ind, f.FactErrorMessage));
                        if (FactAfterDeath(ind, f))
                            errors[(int)Dataerror.FACTS_AFTER_DEATH].Add(
                                new DataError((int)Dataerror.FACTS_AFTER_DEATH, ind, f.FactErrorMessage));
                        if (!GeneralSettings.Default.IgnoreFactTypeWarnings)
                        {
                            foreach (string tag in unknownFactTypes.Keys)
                            {
                                if (f.FactTypeDescription == tag)
                                {
                                    errors[(int)Dataerror.UNKNOWN_FACT_TYPE].Add(
                                        new DataError((int)Dataerror.UNKNOWN_FACT_TYPE, Fact.FactError.QUESTIONABLE,
                                            ind, $"Unknown/Custom fact type {f.FactTypeDescription} recorded"));
                                }
                            }
                        }
                        if (f.IsCensusFact && f.FactDate.FactYearMatches(CensusDate.UKCENSUS1939) && !ind.BirthDate.IsExact && f.FactType != Fact.RESIDENCE && !f.Created)
                        {  //only warn if not a residence fact assumed to be a census fact
                            errors[(int)Dataerror.NATREG1939_INEXACT_BIRTHDATE].Add(
                                        new DataError((int)Dataerror.NATREG1939_INEXACT_BIRTHDATE, Fact.FactError.QUESTIONABLE,
                                            ind, "On the 1939 National Register but birth date is not exact"));
                        }
                    }
                    #region Duplicate Fact Check
                    var dup = ind.AllFileFacts.GroupBy(x => x.EqualHash).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                    var dupList = new List<Fact>();
                    foreach (string dfs in dup)
                    {
                        var df = ind.AllFacts.First(x => x.EqualHash.Equals(dfs));
                        if (df != null)
                        {
                            dupList.Add(df);
                            errors[(int)Dataerror.DUPLICATE_FACT].Add(
                                            new DataError((int)Dataerror.DUPLICATE_FACT, Fact.FactError.ERROR,
                                                ind, $"Duplicated {df.FactTypeDescription} fact recorded"));
                        }
                    }
                    var possDuplicates = ind.AllFileFacts.GroupBy(x => x.PossiblyEqualHash).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                    foreach (string pd in possDuplicates)
                    {
                        var pdf = ind.AllFacts.First(x => x.PossiblyEqualHash.Equals(pd));
                        if (pdf != null && !dupList.ContainsFact(pdf))
                        {
                            errors[(int)Dataerror.POSSIBLE_DUPLICATE_FACT].Add(
                                            new DataError((int)Dataerror.POSSIBLE_DUPLICATE_FACT, Fact.FactError.QUESTIONABLE,
                                                ind, $"Possibly duplicated {pdf.FactTypeDescription} fact recorded"));
                        }
                    }
                    #endregion
                    #endregion
                    #region Parents Facts
                    foreach (ParentalRelationship parents in ind.FamiliesAsChild)
                    {
                        Family asChild = parents.Family;
                        Individual father = asChild.Husband;
                        if (father != null && ind.BirthDate.StartDate.Year != 1 && parents.IsNaturalFather)
                        {
                            int minAge = father.GetMinAge(ind.BirthDate);
                            int maxAge = father.GetMaxAge(ind.BirthDate);
                            if (minAge > 90)
                                errors[(int)Dataerror.BIRTH_AFTER_FATHER_90].Add(new DataError((int)Dataerror.BIRTH_AFTER_FATHER_90, ind, $"Father {father.Name} born {father.BirthDate} is more than 90 yrs old when individual was born"));
                            if (maxAge < 13)
                                errors[(int)Dataerror.BIRTH_BEFORE_FATHER_13].Add(new DataError((int)Dataerror.BIRTH_BEFORE_FATHER_13, ind, $"Father {father.Name} born {father.BirthDate} is less than 13 yrs old when individual was born"));
                            if (father.DeathDate.IsKnown && ind.BirthDate.IsKnown)
                            {
                                FactDate conception = ind.BirthDate.SubtractMonths(9);
                                if (father.DeathDate.IsBefore(conception))
                                    errors[(int)Dataerror.BIRTH_AFTER_FATHER_DEATH].Add(new DataError((int)Dataerror.BIRTH_AFTER_FATHER_DEATH, ind, $"Father {father.Name} died {father.DeathDate} more than 9 months before individual was born"));
                            }
                        }
                        Individual mother = asChild.Wife;
                        if (mother != null && ind.BirthDate.StartDate.Year != 1 && parents.IsNaturalMother)
                        {
                            int minAge = mother.GetMinAge(ind.BirthDate);
                            int maxAge = mother.GetMaxAge(ind.BirthDate);
                            if (minAge > 60)
                                errors[(int)Dataerror.BIRTH_AFTER_MOTHER_60].Add(new DataError((int)Dataerror.BIRTH_AFTER_MOTHER_60, ind, $"Mother {mother.Name} born {mother.BirthDate} is more than 60 yrs old when individual was born"));
                            if (maxAge < 13)
                                errors[(int)Dataerror.BIRTH_BEFORE_MOTHER_13].Add(new DataError((int)Dataerror.BIRTH_BEFORE_MOTHER_13, ind, $"Mother {mother.Name} born {mother.BirthDate} is less than 13 yrs old when individual was born"));
                            if (mother.DeathDate.IsKnown && mother.DeathDate.IsBefore(ind.BirthDate))
                                errors[(int)Dataerror.BIRTH_AFTER_MOTHER_DEATH].Add(new DataError((int)Dataerror.BIRTH_AFTER_MOTHER_DEATH, ind, $"Mother {mother.Name} died {mother.DeathDate} which is before individual was born"));
                        }
                    }
                    List<Individual> womansChildren = new List<Individual>();
                    foreach (Family asParent in ind.FamiliesAsSpouse)
                    {
                        Individual spouse = asParent.Spouse(ind);
                        if (asParent.MarriageDate != null && spouse != null)
                        {
                            if (ind.DeathDate != null && asParent.MarriageDate.IsAfter(ind.DeathDate))
                                errors[(int)Dataerror.MARRIAGE_AFTER_DEATH].Add(new DataError((int)Dataerror.MARRIAGE_AFTER_DEATH, ind, $"Marriage to {spouse.Name} in {asParent.MarriageDate} is after individual died on {ind.DeathDate}"));
                            if (spouse.DeathDate != null && asParent.MarriageDate.IsAfter(spouse.DeathDate))
                                errors[(int)Dataerror.MARRIAGE_AFTER_SPOUSE_DEAD].Add(new DataError((int)Dataerror.MARRIAGE_AFTER_SPOUSE_DEAD, ind, $"Marriage to {spouse.Name} in {asParent.MarriageDate} is after spouse died {spouse.DeathDate}"));
                            int maxAge = ind.GetMaxAge(asParent.MarriageDate);
                            if (maxAge < 13 && ind.BirthDate.IsAfter(FactDate.MARRIAGE_LESS_THAN_13))
                                errors[(int)Dataerror.MARRIAGE_BEFORE_13].Add(new DataError((int)Dataerror.MARRIAGE_BEFORE_13, ind, $"Marriage to {spouse.Name} in {asParent.MarriageDate} is before individual was 13 years old"));
                            maxAge = spouse.GetMaxAge(asParent.MarriageDate);
                            if (maxAge < 13 && spouse.BirthDate.IsAfter(FactDate.MARRIAGE_LESS_THAN_13))
                                errors[(int)Dataerror.MARRIAGE_BEFORE_SPOUSE_13].Add(new DataError((int)Dataerror.MARRIAGE_BEFORE_SPOUSE_13, ind, $"Marriage to {spouse.Name} in {asParent.MarriageDate} is before spouse born {spouse.BirthDate} was 13 years old"));
                            if (ind.Surname == spouse.Surname)
                            {
                                Individual wifesFather = ind.IsMale ? spouse.NaturalFather : ind.NaturalFather;
                                Individual husband = ind.IsMale ? ind : spouse;
                                if (husband.Surname != wifesFather?.Surname) // if couple have same surname and wife is different from her natural father then likely error
                                    errors[(int)Dataerror.SAME_SURNAME_COUPLE].Add(new DataError((int)Dataerror.SAME_SURNAME_COUPLE, ind, $"Spouse {spouse.Name} has same surname. Usually due to wife incorrectly recorded with married instead of maiden name."));
                            }
                            //if (ind.FirstMarriage != null && ind.FirstMarriage.MarriageDate != null)
                            //{
                            //    if (asParent.MarriageDate.isAfter(ind.FirstMarriage.MarriageDate))
                            //    {  // we have a later marriage now see if first marriage spouse is still alive

                            //    }
                            //}
                        }
                        if (!ind.IsMale) // for females as parent in family check children
                        {
                            womansChildren.AddRange(asParent.Children.Where(c => c.IsNaturalChildOf(ind)));
                        }
                    }
                    womansChildren = womansChildren.Distinct().ToList(); // eliminate duplicate children
                    if (womansChildren.Count > 1) // only bother checking if we have two or more children.
                    {
                        womansChildren.Sort(new BirthDateComparer());
                        FactDate previousBirth = ind.BirthDate;  // set start date to womans birth date.
                        foreach (Individual child in womansChildren)
                        {
                            if (child.IsBirthKnown)
                            {
                                double daysDiff = child.BirthDate.DaysDifference(previousBirth);
                                if (daysDiff >= 10 && daysDiff <= 168)
                                    errors[(int)Dataerror.SIBLING_TOO_SOON].Add(new DataError((int)Dataerror.SIBLING_TOO_SOON, Fact.FactError.ERROR, child, $"Child {child.Name} of {ind.Name} born too soon, only {daysDiff} days after sibling."));
                                if (daysDiff > 168 && daysDiff < 365)
                                    errors[(int)Dataerror.SIBLING_PROB_TOO_SOON].Add(new DataError((int)Dataerror.SIBLING_PROB_TOO_SOON, Fact.FactError.QUESTIONABLE, ind, $"Child {child.Name} of {ind.Name} born very soon after sibling, only {daysDiff} days later."));
                            }
                        }
                    }
                    #endregion
                }
#if __MACOS__ || __IOS__
                catch (Exception)
                {
                    catchCount++;
                }
#else
                catch (Exception e)
                {
                    if (catchCount == 0) // prevent multiple displays of the same error - usually resource icon load failures
                    {
                        ErrorHandler.Show("FTA_0001", e);
                        catchCount++;
                    }
                }
#endif
            }
            #endregion
            #region Family Fact Errors
            catchCount = 0;
            foreach (Family fam in AllFamilies)
            {
                progress.Report(20 + (record++ / totalRecords));
                try
                {
                    foreach (Fact f in fam.Facts)
                    {
                        if (f.FactErrorLevel == Fact.FactError.ERROR)
                        {
                            if (f.FactType == Fact.CHILDREN1911)
                                errors[(int)Dataerror.CHILDRENSTATUS_TOTAL_MISMATCH].Add(
                                    new DataError((int)Dataerror.CHILDRENSTATUS_TOTAL_MISMATCH, fam, f.FactErrorMessage));
                            else
                                errors[(int)Dataerror.FACT_ERROR].Add(
                                    new DataError((int)Dataerror.FACT_ERROR, fam, f.FactErrorMessage));
                        }
                    }
                }
                catch (Exception)
                {
                    if (catchCount == 0) // prevent multiple displays of the same error - usually resource icon load failures
                        catchCount++;
                }
            }
            #endregion

            for (int i = 0; i < DATA_ERROR_GROUPS; i++)
                DataErrorTypes.Add(new DataErrorGroup(i, errors[i]));
        }

        public IList<DataErrorGroup> DataErrorTypes { get; private set; }

        public static bool FactBeforeBirth(Individual ind, Fact f)
        {
            if (ind is null || f is null) return false;
            if (f.FactType != Fact.BIRTH & f.FactType != Fact.BIRTH_CALC && Fact.LOOSE_BIRTH_FACTS.Contains(f.FactType) && f.FactDate.IsBefore(ind.BirthDate))
            {
                if (f.FactType == Fact.CHRISTENING || f.FactType == Fact.BAPTISM)
                {  //due to possible late birth abt qtr reporting use 3 month fudge factor for bapm/chr
                    if (f.FactDate.IsBefore(ind.BirthDate.SubtractMonths(4)))
                        return true;
                }
                else
                    return true;
            }
            return false;
        }

        public static bool FactAfterDeath(Individual ind, Fact f) => Fact.LOOSE_DEATH_FACTS.Contains(f.FactType) && f.FactDate.IsAfter(ind.DeathDate);
        public enum Dataerror
        {
            BIRTH_AFTER_BAPTISM = 0, BIRTH_AFTER_DEATH = 1, BIRTH_AFTER_FATHER_90 = 2, BIRTH_AFTER_MOTHER_60 = 3, BIRTH_AFTER_MOTHER_DEATH = 4,
            BIRTH_AFTER_FATHER_DEATH = 5, BIRTH_BEFORE_FATHER_13 = 6, BIRTH_BEFORE_MOTHER_13 = 7, BURIAL_BEFORE_DEATH = 8,
            AGED_MORE_THAN_110 = 9, FACTS_BEFORE_BIRTH = 10, FACTS_AFTER_DEATH = 11, MARRIAGE_AFTER_DEATH = 12,
            MARRIAGE_AFTER_SPOUSE_DEAD = 13, MARRIAGE_BEFORE_13 = 14, MARRIAGE_BEFORE_SPOUSE_13 = 15, LOST_COUSINS_NON_CENSUS = 16,
            LOST_COUSINS_NOT_SUPPORTED_YEAR = 17, RESIDENCE_CENSUS_DATE = 18, CENSUS_COVERAGE = 19, FACT_ERROR = 20,
            UNKNOWN_FACT_TYPE = 21, LIVING_WITH_DEATH_DATE = 22, CHILDRENSTATUS_TOTAL_MISMATCH = 23, DUPLICATE_FACT = 24,
            POSSIBLE_DUPLICATE_FACT = 25, NATREG1939_INEXACT_BIRTHDATE = 26, MALE_WIFE_FEMALE_HUSBAND = 27,
            SAME_SURNAME_COUPLE = 28, SIBLING_TOO_SOON = 29, SIBLING_PROB_TOO_SOON = 30, FACT_IN_FUTURE = 31
        };

        #endregion

        #region Census Searching

        public static string ProviderName(int censusProvider)
        {
            switch (censusProvider)
            {
                case 0:
                    return "Ancestry";
                case 1:
                    return "FindMyPast";
                case 2:
                    return "FreeCen";
                case 3:
                    return "FamilySearch";
                case 4:
                    return "ScotlandsPeople";
                default:
                    return "FamilySearch";
            }
        }

        public void SearchCensus(string censusCountry, int censusYear, Individual person, int censusProvider, string censusRegion)
        {
            string uri = null;
            string provider = ProviderName(censusProvider);
            if (censusYear == 1950 && censusCountry.Equals(Countries.UNITED_STATES))
            {
                UIHelpers.ShowMessage("Automated Searching for 1950 US Census not yet implemented");
                return;
            }
            if (censusYear == 1921 && Countries.IsUnitedKingdom(censusCountry))
            {
                UIHelpers.ShowMessage("Automated Searching for 1921 census not yet implemented");
                return;
            }
            switch (censusProvider)
            {
                case 0:
                    uri = BuildAncestryCensusQuery(censusCountry, censusYear, person, censusRegion);
                    break;
                case 1:
                    uri = censusYear == 1939 && censusCountry.Equals(Countries.UNITED_KINGDOM) ? BuildFindMyPast1939Query(person, censusRegion) : BuildFindMyPastCensusQuery(censusCountry, censusYear, person, censusRegion);
                    break;
                case 2:
                    uri = BuildFreeCenCensusQuery(censusCountry, censusYear, person);
                    break;
                case 3:
                    uri = BuildFamilySearchCensusQuery(censusCountry, censusYear, person);
                    break;
                case 4:
                    uri = BuildScotlandsPeopleCensusQuery(censusYear, person);
                    break;
            }
            if (uri != null)
            {
                SpecialMethods.VisitWebsite(uri);
                Analytics.TrackAction(Analytics.CensusSearchAction, $"Searching {provider} {censusYear}");
            }
        }

        string BuildScotlandsPeopleCensusQuery(int censusYear, Individual person)
        {
            // &surname=Bisset&surname_so=fuzzy&forename=Alexander&forename_so=syn&second_person_forename_so=exact&age_from=10&age_to=16&record_type=census&year%5B0%5D=1841
            FactDate censusFactDate = new FactDate(censusYear.ToString());
            StringBuilder path = new StringBuilder();
            path.Append("https://www.scotlandspeople.gov.uk/record-results?search_type=people&dl_cat=census");
            string surname = person.SurnameAtDate(censusFactDate);
            if (surname != "?" && surname.ToUpper() != Individual.UNKNOWN_NAME)
                path.Append($"&surname={HttpUtility.UrlEncode(surname)}&surname_so=fuzzy");
            if (person.Forename != "?" && person.Forename.ToUpper() != Individual.UNKNOWN_NAME)
                path.Append($"&forename={HttpUtility.UrlEncode(person.Forenames)}&forename_so=syn");
            Age age = person.GetAge(censusFactDate);
            if (censusYear == 1841 && age.MaxAge > 15)
                path.Append($"&age_from={age.MinAge - 1}&age_to={age.MaxAge + 5}");
            else
                path.Append($"&age_from={age.MinAge - 1}&age_to={age.MaxAge + 1}");
            path.Append($"&record_type=census&year%5B0%5D={censusYear}");
            return path.ToString();
        }

        string BuildFamilySearchCensusQuery(string country, int censusYear, Individual person)
        {
            FactDate censusFactDate = new FactDate(censusYear.ToString());

            //updated https://www.familysearch.org/search/record/results?givenname=bisset&surname=william&record_type=3&offset=0&count=20
            StringBuilder path = new StringBuilder();
            path.Append("https://www.familysearch.org/search/record/results?");
            if (person.Forename != "?" && person.Forename.ToUpper() != Individual.UNKNOWN_NAME)
                path.Append($"{FamilySearch.GIVENNAME}={HttpUtility.UrlEncode(person.Forenames)}");
            string surname = person.SurnameAtDate(censusFactDate);
            if (surname != "?" && surname.ToUpper() != Individual.UNKNOWN_NAME)
                path.Append($"&{FamilySearch.SURNAME}={HttpUtility.UrlEncode(surname)}");
            path.Append($"&{FamilySearch.RECORD_TYPE}=3");
            if (person.BirthDate.IsKnown)
            {
                int startYear = person.BirthDate.StartDate.Year - 1;
                int endYear = person.BirthDate.EndDate.Year + 1;
                path.Append($"&{FamilySearch.BIRTH_YEAR}={startYear}-{endYear}");
            }
            string location = Countries.UNKNOWN_COUNTRY;
            if (person.BirthLocation.IsKnown)
            {
                location = person.BirthLocation.Country != country
                    ? person.BirthLocation.Country
                    : person.BirthLocation.GetLocation(FactLocation.REGION).ToString().Replace(",", "");
                path.Append($"&{FamilySearch.BIRTH_LOCATION}={HttpUtility.UrlEncode(location)}");
            }
            int collection = FamilySearch.CensusCollectionID(country, censusYear);
            if (collection > 0)
                path.Append($"&collection_id={collection}");
            else
            {
                collection = FamilySearch.CensusCollectionID(location, censusYear);
                if (collection > 0)
                    path.Append($"&collection_id={collection}");
                else if (Countries.IsUnitedKingdom(country))
                {
                    collection = FamilySearch.CensusCollectionID(Countries.ENGLAND, censusYear);
                    path.Append($"&collection_id={collection}");
                }
                else if (Countries.IsKnownCountry(country))
                { // TODO
                    throw new CensusSearchException($"Sorry searching the {country} census on FamilySearch for {censusYear} is not supported by FTAnalyzer at this time");
                }
            }
            return path.Replace("+", "%20").ToString();
        }

        string BuildAncestryCensusQuery(string censusCountry, int censusYear, Individual person, string censusRegion = ".com")
        {
            if (censusYear == 1939 && censusCountry.Equals(Countries.UNITED_KINGDOM))
                return BuildAncestry1939Query(person, censusRegion);
            if (censusYear == 1940 && censusCountry.Equals(Countries.UNITED_STATES))
                return BuildAncestry1940Query(person, censusRegion);
            UriBuilder uri = new UriBuilder
            {
                Host = $"search.ancestry{censusRegion}",
                Path = "cgi-bin/sse.dll"
            };
            StringBuilder query = new StringBuilder();
            if (censusCountry.Equals(Countries.UNITED_KINGDOM))
            {
                query.Append($"gl={censusYear}uki&");
                query.Append("gss=ms_f-68&");
            }
            else if (censusCountry.Equals(Countries.IRELAND))
            {
                if (censusYear == 1901)
                    query.Append("db=websearch-4150&");
                if (censusYear == 1911)
                    query.Append("db=websearch-4050&");
            }
            else if (censusCountry.Equals(Countries.UNITED_STATES))
            {
                CensusDate cd = CensusDate.US_FEDERAL_CENSUS.First(x => x.BestYear == censusYear);
                uri.Path = $"search/collections/{cd.AncestryCatalog}/";
            }
            else if (censusCountry.Equals(Countries.CANADA))
            {
                if (censusYear == 1921)
                    query.Append("db=cancen1921&");
                else
                    query.Append($"db={censusYear}canada&");
            }
            if (censusCountry.Equals(Countries.UNITED_STATES))
            {
                query.Append("name=");
                if (person.Forenames != "?" && person.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
                    query.Append($"{HttpUtility.UrlEncode(person.Forenames)}_");
                string surname = string.Empty;
                if (person.Surname != "?" && person.Surname.ToUpper() != Individual.UNKNOWN_NAME)
                    surname = person.Surname;
                if (person.MarriedName != "?" && person.MarriedName.ToUpper() != Individual.UNKNOWN_NAME && person.MarriedName != person.Surname)
                    surname += $" {person.MarriedName}";
                surname = surname.Trim();
                query.Append($"{HttpUtility.UrlEncode(surname)}&");
                if (person.BirthDate.IsKnown)
                {
                    int startYear = person.BirthDate.StartDate.Year;
                    int endYear = person.BirthDate.EndDate.Year;
                    int year, range;
                    if (startYear == FactDate.MINDATE.Year)
                    {
                        year = endYear - 9;
                        range = 10;
                    }
                    else if (endYear == FactDate.MAXDATE.Year)
                    {
                        year = startYear + 9;
                        range = 10;
                    }
                    else
                    {
                        year = (endYear + startYear + 1) / 2;
                        range = (endYear - startYear + 1) / 2;
                        if (range < 2) range = 2;
                        if (2 < range && range < 5) range = 5;
                        if (range > 5)
                        {
                            year = startYear + 5;
                            range = 10;
                        }
                        if (year > censusYear) year = censusYear;
                    }
                    query.Append($"&birth={year}");
                    if (person.BirthLocation.IsKnown)
                    {
                        string location = person.BirthLocation.GetLocation(FactLocation.SUBREGION).ToString();
                        query.Append($"_{HttpUtility.UrlEncode(location)}");
                    }
                    query.Append($"&birth_x={range}-0-0");
                }

            }
            else
            {
                query.Append("rank=1&");
                query.Append("new=1&");
                query.Append("so=3&");
                query.Append("MSAV=1&");
                query.Append("msT=1&");
                if (person.Forenames != "?" && person.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
                    query.Append($"gsfn={HttpUtility.UrlEncode(person.Forenames)}&");
                string surname = string.Empty;
                if (person.Surname != "?" && person.Surname.ToUpper() != Individual.UNKNOWN_NAME)
                    surname = person.Surname;
                if (person.MarriedName != "?" && person.MarriedName.ToUpper() != Individual.UNKNOWN_NAME && person.MarriedName != person.Surname)
                    surname += $" {person.MarriedName}";
                surname = surname.Trim();
                query.Append($"gsln={HttpUtility.UrlEncode(surname)}&");
                if (person.BirthDate.IsKnown)
                {
                    int startYear = person.BirthDate.StartDate.Year;
                    int endYear = person.BirthDate.EndDate.Year;
                    int year, range;
                    if (startYear == FactDate.MINDATE.Year)
                    {
                        year = endYear - 9;
                        range = 10;
                    }
                    else if (endYear == FactDate.MAXDATE.Year)
                    {
                        year = startYear + 9;
                        range = 10;
                    }
                    else
                    {
                        year = (endYear + startYear + 1) / 2;
                        range = (endYear - startYear + 1) / 2;
                        if (2 < range && range < 5) range = 5;
                        if (range > 5)
                        {
                            year = startYear + 5;
                            range = 10;
                        }
                        if (year > censusYear) year = censusYear;
                    }
                    query.Append($"msbdy={year}&");
                    query.Append($"msbdp={range}&");
                }
                if (person.BirthLocation.IsKnown)
                {
                    string location = person.BirthLocation.GetLocation(FactLocation.SUBREGION).ToString();
                    query.Append($"msbpn__ftp={HttpUtility.UrlEncode(location)}&");
                }
                query.Append("uidh=2t2");
            }
            uri.Query = query.ToString();
            return uri.ToString();
        }
        string BuildAncestry1939Query(Individual person, string censusRegion = ".co.uk")
        {
            UriBuilder uri = new UriBuilder
            {
                Host = $"search.ancestry{censusRegion}",
                Path = "search/collections/1939ukregister/"
            };
            StringBuilder query = new StringBuilder();
            string forename = string.Empty;
            string surname = string.Empty;
            if (person.Forenames != "?" && person.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
                forename = HttpUtility.UrlEncode(person.Forenames);
            if (person.Surname != "?" && person.Surname.ToUpper() != Individual.UNKNOWN_NAME)
                surname = person.Surname;
            if (person.MarriedName != "?" && person.MarriedName.ToUpper() != Individual.UNKNOWN_NAME && person.MarriedName != person.Surname)
                surname += $" {person.MarriedName}";
            surname = HttpUtility.UrlEncode(surname.Trim());
            query.Append($"name={forename}_{surname}&name_x=ps_ps&");
            if (person.BirthDate.IsKnown)
            {
                int startYear = person.BirthDate.StartDate.Year;
                int endYear = person.BirthDate.EndDate.Year;
                int year, range;
                if (startYear == FactDate.MINDATE.Year)
                {
                    year = endYear - 9;
                    range = 10;
                }
                else if (endYear == FactDate.MAXDATE.Year)
                {
                    year = startYear + 9;
                    range = 10;
                }
                else
                {
                    year = (endYear + startYear + 1) / 2;
                    range = (endYear - startYear + 1) / 2;
                    if (2 < range && range < 5) range = 5;
                    if (range > 5)
                    {
                        year = startYear + 5;
                        range = 10;
                    }
                    if (year > 1939) year = 1939;
                }
                query.Append($"&birth={year}&");
                query.Append($"&birth_x={range}-0-0&");
            }
            FactLocation bestLocation = person.BestLocation(CensusDate.UKCENSUS1939);
            if (bestLocation.IsKnown)
            {
                string location = HttpUtility.UrlEncode(bestLocation.ToString());
                query.Append($"residence={location}");
            }
            uri.Query = query.ToString();
            return uri.ToString();
        }

        string BuildAncestry1940Query(Individual person, string censusRegion = ".com")
        {
            // ?name=Sylvia+Esther_Buck+Sweitzer&birth=1932_ohio-usa_38&birth_x=1-0-0&name_x=ps_ps
            UriBuilder uri = new UriBuilder
            {
                Host = $"search.ancestry{censusRegion}",
                Path = "search/collections/1940usfedcen/"
            };
            StringBuilder query = new StringBuilder();
            string forename = string.Empty;
            string surname = string.Empty;
            if (person.Forenames != "?" && person.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
                forename = HttpUtility.UrlEncode(person.Forenames);
            if (person.Surname != "?" && person.Surname.ToUpper() != Individual.UNKNOWN_NAME)
                surname = person.Surname;
            if (person.MarriedName != "?" && person.MarriedName.ToUpper() != Individual.UNKNOWN_NAME && person.MarriedName != person.Surname)
                surname += $" {person.MarriedName}";
            surname = HttpUtility.UrlEncode(surname.Trim());
            query.Append($"name={forename}_{surname}");
            if (person.BirthDate.IsKnown)
            {
                int startYear = person.BirthDate.StartDate.Year;
                int endYear = person.BirthDate.EndDate.Year;
                int year, range;
                if (startYear == FactDate.MINDATE.Year)
                {
                    year = endYear - 9;
                    range = 10;
                }
                else if (endYear == FactDate.MAXDATE.Year)
                {
                    year = startYear + 9;
                    range = 10;
                }
                else
                {
                    year = (endYear + startYear + 1) / 2;
                    range = (endYear - startYear + 1) / 2;
                    if (2 < range && range < 5) range = 5;
                    if (range > 5)
                    {
                        year = startYear + 5;
                        range = 10;
                    }
                    if (year > 1939) year = 1939;
                }
                query.Append($"&birth={year}&");
                query.Append($"&birth_x={range}-0-0&");
            }
            FactLocation bestLocation = person.BestLocation(CensusDate.UKCENSUS1939);
            if (bestLocation.IsKnown)
            {
                string location = HttpUtility.UrlEncode(bestLocation.ToString());
                query.Append($"residence={location}");
            }
            uri.Query = query.ToString();
            return uri.ToString();
        }

        string BuildFreeCenCensusQuery(string censusCountry, int censusYear, Individual person)
        {
            if (!censusCountry.Equals(Countries.UNITED_KINGDOM) && !censusCountry.Equals("Unknown"))
            {
                throw new CensusSearchException("Sorry only UK searches can be done on FreeCEN.");
            }
            FactDate censusFactDate = new FactDate(censusYear.ToString());
            UriBuilder uri = new UriBuilder
            {
                //Host = "www.freecen.org.uk",
                Scheme = Uri.UriSchemeHttps,
                Host = "freecen1.freecen.org.uk",
                Path = "/cgi/search.pl"
            };
            StringBuilder query = new StringBuilder();
            query.Append($"y={censusYear}&");
            if (person.Forenames != "?" && person.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
            {
                int pos = person.Forenames.IndexOf(" ", StringComparison.Ordinal);
                string forename = person.Forenames;
                if (pos > 0)
                    forename = person.Forenames.Substring(0, pos); //strip out any middle names as FreeCen searches better without then
                query.Append($"g={HttpUtility.UrlEncode(forename)}&");
            }
            string surname = person.SurnameAtDate(censusFactDate);
            if (surname != "?" && surname.ToUpper() != Individual.UNKNOWN_NAME)
            {
                query.Append($"s={HttpUtility.UrlEncode(surname)}&");
                query.Append("p=on&");
            }
            if (person.BirthDate.IsKnown)
            {
                int startYear = person.BirthDate.StartDate.Year;
                int endYear = person.BirthDate.EndDate.Year;
                int year, range;
                if (startYear == FactDate.MINDATE.Year)
                {
                    year = endYear - 9;
                    range = 10;
                }
                else if (endYear == FactDate.MAXDATE.Year)
                {
                    year = startYear + 9;
                    range = 10;
                }
                else
                {
                    year = (endYear + startYear + 1) / 2;
                    range = (endYear - startYear + 5) / 2;
                }
                if (range == 0)
                    query.Append("r=0&");
                else if (range <= 2)
                    query.Append("r=2&");
                else if (range <= 5)
                    query.Append("r=5&");
                else
                {
                    year = startYear + 5;
                    query.Append("r=10&");
                }
                if (year > censusYear) year = censusYear;
                query.Append($"a={year}&");
            }
            if (person.BirthLocation.IsKnown)
            {
                string location = person.BirthLocation.SubRegion;
                query.Append($"t={HttpUtility.UrlEncode(location)}&");
                query.Append($"b={person.BirthLocation.FreeCenCountyCode}&");
            }
            query.Append("c=all&"); // initially set to search all counties need a routine to return FreeCen county codes 
            query.Append("z=Find&"); // executes search
            uri.Query = query.ToString();
            return uri.ToString();
        }

        string BuildFindMyPastCensusQuery(string censusCountry, int censusYear, Individual person, string censusRegion = ".com")
        {
            // new http://search.findmypast.co.uk/results/united-kingdom-records-in-census-land-and-surveys?firstname=peter&firstname_variants=true&lastname=moir&lastname_variants=true&eventyear=1881&eventyear_offset=2&yearofbirth=1825&yearofbirth_offset=2
            FactDate censusFactDate = new FactDate(censusYear.ToString());
            UriBuilder uri = new UriBuilder
            {
                Host = $"search.findmypast{censusRegion}"
            };
            if (censusCountry.Equals(Countries.UNITED_STATES))
                uri.Path = "/results/united-states-records-in-census-land-and-surveys";
            else if (Countries.IsUnitedKingdom(censusCountry))
            {
                uri.Path = censusYear == 1911 ? $"/search-world-records/1911-census-for-england-and-wales" :
                $"/search-world-records/{censusYear}-england-wales-and-scotland-census";
            }
            else if (censusCountry.Equals(Countries.IRELAND))
                uri.Path = "/results/ireland-records-in-census-land-and-surveys";
            else
                uri.Path = "/results/world-records-in-census-land-and-surveys";
            StringBuilder query = new StringBuilder();
            query.Append($"eventyear={censusYear}&eventyear_offset=0&");

            if (person.Forenames != "?" && person.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
            {
                int pos = person.Forenames.IndexOf(" ", StringComparison.Ordinal);
                string forenames = person.Forenames;
                if (pos > 0)
                    forenames = person.Forenames.Substring(0, pos); //strip out any middle names as searches better without then
                query.Append($"firstname={HttpUtility.UrlEncode(forenames)}&");
                query.Append("firstname_variants=true&");
            }
            string surname = person.SurnameAtDate(censusFactDate);
            if (surname != "?" && surname.ToUpper() != Individual.UNKNOWN_NAME)
            {
                query.Append($"lastName={HttpUtility.UrlEncode(surname)} &");
                query.Append("lastname_variants=true&");
            }
            if (person.BirthDate.IsKnown)
            {
                int startYear = person.BirthDate.StartDate.Year;
                int endYear = person.BirthDate.EndDate.Year;
                int year, range;
                if (startYear == FactDate.MINDATE.Year)
                {
                    year = endYear - 9;
                    range = 10;
                }
                else if (endYear == FactDate.MAXDATE.Year)
                {
                    year = startYear + 9;
                    range = 10;
                }
                else
                {
                    year = (endYear + startYear + 1) / 2;
                    range = (endYear - startYear + 3) / 2;
                    if (range > 5)
                    {
                        year = startYear + 5;
                        range = 10;
                    }
                    if (year > censusYear) year = censusYear;
                }
                query.Append($"yearofbirth={year} &");
                query.Append($"yearofbirth_offset={range}&");
            }
            if (censusYear == 1911 && Countries.IsUnitedKingdom(censusCountry))
            {
                CensusReference reference = person.GetCensusReference(CensusDate.EWCENSUS1911);
                if (reference?.Piece != null && reference.Schedule == "Missing")
                    query.Append($"pieceno={reference.Piece}");
            }
            //if (person.BirthLocation != FactLocation.UNKNOWN_LOCATION)
            //{
            //    query.Append("birthPlace=" + HttpUtility.UrlEncode(person.BirthLocation.SubRegion) + "&");
            //    Tuple<string, string> area = person.BirthLocation.FindMyPastCountyCode;
            //    if (area != null)
            //    {
            //        query.Append("country=" + HttpUtility.UrlEncode(area.Item1) + "&");
            //        query.Append("coIdList=" + HttpUtility.UrlEncode(area.Item2));
            //    }
            //    else
            //    {
            //        query.Append("country=&coIdList=");
            //    }
            //}
            //else
            //{
            //    query.Append("birthPlace=&country=&coIdList=");
            //}
            uri.Query = query.ToString();
            return uri.ToString();
        }

        string BuildFindMyPast1939Query(Individual person, string censusRegion)
        {
            // new http://search.findmypast.co.uk/results/world-records/1939-register?firstname=frederick&firstname_variants=true&lastname=deakin&lastname_variants=true&yearofbirth=1879
            FactDate censusFactDate = CensusDate.UKCENSUS1939;
            UriBuilder uri = new UriBuilder
            {
                Host = $"search.findmypast{censusRegion}",
                Path = "/results/world-records/1939-register"
            };
            StringBuilder query = new StringBuilder();

            if (person.Forenames != "?" && person.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
            {
                int pos = person.Forenames.IndexOf(" ", StringComparison.Ordinal);
                string forenames = person.Forenames;
                if (pos > 0)
                    forenames = person.Forenames.Substring(0, pos); //strip out any middle names as searches better without then
                query.Append($"firstname={HttpUtility.UrlEncode(forenames)}&");
                query.Append("firstname_variants=true&");
            }
            string surname = person.SurnameAtDate(censusFactDate);
            if (surname != "?" && surname.ToUpper() != Individual.UNKNOWN_NAME)
            {
                query.Append($"lastName={HttpUtility.UrlEncode(surname)}&");
                query.Append("lastname_variants=true&");
            }
            if (person.BirthDate.IsKnown)
            {
                int startYear = person.BirthDate.StartDate.Year;
                int endYear = person.BirthDate.EndDate.Year;
                int year, range;
                if (startYear == FactDate.MINDATE.Year)
                {
                    year = endYear - 9;
                    range = 10;
                }
                else if (endYear == FactDate.MAXDATE.Year)
                {
                    year = startYear + 9;
                    range = 10;
                }
                else
                {
                    year = (endYear + startYear + 1) / 2;
                    range = (endYear - startYear + 3) / 2;
                    if (range > 5)
                    {
                        year = startYear + 5;
                        range = 10;
                    }
                    if (year > 1939) year = 1939;
                }
                query.Append($"yearofbirth={year} &");
                query.Append($"yearofbirth_offset={range}&");
            }
            uri.Query = query.ToString();
            return uri.ToString();
        }

        #endregion

        #region Birth/Marriage/Death Searching

        public enum SearchType { BIRTH = 0, MARRIAGE = 1, DEATH = 2 };

        public void SearchBMD(SearchType st, Individual individual, FactDate factdate, int searchProvider, string bmdRegion, Individual spouse)
        {
            string uri = null;
            if (factdate.IsUnknown || factdate.DateType.Equals(FactDate.FactDateType.AFT) || factdate.DateType.Equals(FactDate.FactDateType.BEF))
            {
                if (st.Equals(SearchType.BIRTH))
                {
                    CheckLooseBirth(individual);
                    factdate = individual.LooseBirthDate;
                }
                if (st.Equals(SearchType.MARRIAGE))
                {
                    if (factdate.StartDate < individual.BirthDate.StartDate.AddYears(GeneralSettings.Default.MinParentalAge))
                        factdate = new FactDate(individual.BirthDate.StartDate.AddYears(GeneralSettings.Default.MinParentalAge), factdate.EndDate);
                    //    CheckLooseMarriage(individual);
                    //    factdate = individual.LooseMarriageDate;
                }
                if (st.Equals(SearchType.DEATH))
                {
                    CheckLooseDeath(individual);
                    factdate = individual.LooseDeathDate;
                }
                if (factdate.StartDate > factdate.EndDate)
                    factdate = FactDate.UNKNOWN_DATE; // errors in facts corrupts loose births or deaths
            }
            string provider = string.Empty;
            Tuple<string, string> uris = new Tuple<string, string>(null, null);
            switch (searchProvider)
            {
                case 0: uri = BuildAncestryQuery(st, individual, factdate, bmdRegion); provider = "Ancestry"; break;
                case 1: uri = BuildFindMyPastQuery(st, individual, factdate, bmdRegion); provider = "FindMyPast"; break;
                //                case 2: uri = BuildFreeBMDQuery(st, individual, factdate); provider = "FreeBMD"; break;
                case 3: uri = BuildFamilySearchQuery(st, individual, factdate); provider = "FamilySearch"; break;
                case 4: uris = BuildScotlandsPeopleQuery(st, individual, factdate, spouse); provider = "ScotlandsPeople"; break;
                    //                case 5: uri = BuildGROQuery(st, individual, factdate); provider = "GRO"; break;
            }
            if (!string.IsNullOrEmpty(uri))
            {
                SpecialMethods.VisitWebsite(uri);
                Analytics.TrackAction(Analytics.BMDSearchAction, $"Searching {provider} BMDs");
            }
            if (searchProvider == 4)
            {
                if (!string.IsNullOrEmpty(uris.Item1))
                {
                    SpecialMethods.VisitWebsite(uris.Item1);
                    Analytics.TrackAction(Analytics.BMDSearchAction, $"Searching {provider} OPR BMDs");
                }
                if (!string.IsNullOrEmpty(uris.Item2))
                {
                    SpecialMethods.VisitWebsite(uris.Item2);
                    Analytics.TrackAction(Analytics.BMDSearchAction, $"Searching {provider} Statutory BMDs");
                }
            }
        }

        Tuple<string, string> BuildScotlandsPeopleQuery(SearchType st, Individual individual, FactDate factdate, Individual spouse)
        {
            string oprResult = string.Empty;
            string statutoryResult = string.Empty;
            bool oprrecords = factdate.EndDate.Year >= 1553 && factdate.StartDate.Year < 1855;
            bool statutory = factdate.StartDate.Year >= 1855 || factdate.EndDate.Year >= 1855;
            UriBuilder uri = new UriBuilder
            {
                Host = "www.scotlandspeople.gov.uk",
                Path = "record-results"
            };
            if (statutory)
            {
                StringBuilder query = new StringBuilder();
                query.Append("search_type=people");
                query.Append("&dl_cat=statutory");
                if (st == SearchType.BIRTH)
                    query.Append("&dl_rec=statutory-births");
                else if (st == SearchType.MARRIAGE)
                    query.Append("&dl_rec=statutory-marriages");
                else if (st == SearchType.DEATH)
                    query.Append("&dl_rec=statutory-deaths");
                query.Append($"&surname={HttpUtility.UrlEncode(individual.Surname)}&surname_so=soundex");
                if (individual.Forename != "?" && individual.Forename.ToUpper() != Individual.UNKNOWN_NAME)
                    query.Append($"&forename={HttpUtility.UrlEncode(individual.Forename)}&forename_so=syn");
                if (st == SearchType.BIRTH)
                    query.Append("&record_type=stat_births");
                else if (st == SearchType.MARRIAGE)
                {
                    query.Append("&record_type=stat_marriages");
                    if (spouse != null)
                        query.Append($"&spsurname={HttpUtility.UrlEncode(spouse.Surname)}&spsurname_so=soundex&spforename={HttpUtility.UrlEncode(spouse.Forename)}&spforename_so=syn");
                }
                else if (st == SearchType.DEATH)
                    query.Append("&record_type=stat_deaths");
                int fromYear = Math.Max(1855, factdate.StartDate.Year - 1); // -1 to add a years tolerance either side
                int toYear = Math.Min(factdate.EndDate.Year + 1, FactDate.NOW.Year); // +1 to add a years tolerance either side
                query.Append($"&from_year={fromYear}&to_year={toYear}");
                uri.Query = query.ToString();
                oprResult = uri.ToString();
            }
            if (oprrecords)
            {
                StringBuilder query = new StringBuilder();
                query.Append("search_type=people");
                if (st == SearchType.BIRTH)
                    query.Append("event=%28B%20OR%20C%20OR%20S%29&record_type%5B0%5D=opr_births&church_type=Old%20Parish%20Registers&dl_cat=church&dl_rec=church-births-baptisms");
                else if (st == SearchType.MARRIAGE)
                    query.Append("&event=M&record_type%5B0%5D=opr_marriages&church_type=Old%20Parish%20Registers&dl_cat=church&dl_rec=church-banns-marriages");
                else if (st == SearchType.DEATH)
                    query.Append("&event=D&record_type%5B0%5D=opr_deaths&church_type=Old%20Parish%20Registers&dl_cat=church&dl_rec=church-deaths-burials");
                query.Append($"&surname={HttpUtility.UrlEncode(individual.Surname)}&surname_so=soundex");
                if (individual.Forename != "?" && individual.Forename.ToUpper() != Individual.UNKNOWN_NAME)
                    query.Append($"&forename={HttpUtility.UrlEncode(individual.Forename)}&forename_so=syn");
                if (st == SearchType.MARRIAGE && spouse != null)
                    query.Append($"&spouse_name={HttpUtility.UrlEncode(spouse.Surname)}&spouse_name_so=fuzzy");
                int fromYear = Math.Max(factdate.StartDate.Year - 1, 1553);
                int toYear = Math.Min(factdate.EndDate.Year + 1, 1854);
                query.Append($"&from_year={fromYear}&to_year={toYear}");
                uri.Query = query.ToString();
                statutoryResult = uri.ToString();
            }
            return new Tuple<string, string>(oprResult, statutoryResult);
        }

        string BuildFamilySearchQuery(SearchType st, Individual individual, FactDate factdate)
        {
            // https://familysearch.org/search/record/results?count=20&query=%2Bgivenname%3AElizabeth~%20%2Bsurname%3AAckers~%20%2Bbirth_place%3A%22walton%20le%20dale%2C%20lancashire%2C%20england%22~%20%2Bbirth_year%3A1879-1881~%20%2Brecord_country%3AEngland
            UriBuilder uri = new UriBuilder
            {
                Host = "familysearch.org",
                Path = "search/record/results"
            };
            StringBuilder query = new StringBuilder();
            query.Append("count=20&query=");

            if (individual.Forename != "?" && individual.Forename.ToUpper() != Individual.UNKNOWN_NAME)
                query.Append($"%2Bgivenname%3A{HttpUtility.UrlEncode(individual.Forename)}~%20");
            string surname = GetSurname(st, individual, false);
            query.Append($"%2Bsurname%3A{HttpUtility.UrlEncode(surname)} ~%20");
            if (individual.BirthDate.IsKnown)
            {
                int startYear = individual.BirthDate.StartDate.Year;
                int endYear = individual.BirthDate.EndDate.Year;
                if (startYear == FactDate.MINDATE.Year)
                    startYear = endYear - 9;
                else if (endYear == FactDate.MAXDATE.Year)
                    endYear = startYear + 9;
                query.Append($"%2Bbirth_year%3A{startYear}-{endYear}~%20");
            }
            if (st.Equals(SearchType.BIRTH) && individual.BirthLocation.IsKnown)
            {  // add birth place if searching for a birth
                string location = individual.BirthLocation.GetLocation(FactLocation.SUBREGION).ToString();
                query.Append($"%2Bbirth_place%3A%22{HttpUtility.UrlEncode(location)}%22~%20");
            }
            string record_country = RecordCountry(st, individual, factdate);
            if (Countries.IsKnownCountry(record_country))
                query.Append($"%2Brecord_country%3A{HttpUtility.UrlEncode(record_country)}");
            uri.Query = query.ToString();
            return uri.ToString();
        }

        static string RecordCountry(SearchType st, Individual individual, FactDate factdate)
        {
            string record_country = Countries.UNKNOWN_COUNTRY;
            if (Countries.IsKnownCountry(individual.BirthLocation.Country))
                record_country = individual.BirthLocation.Country;
            if (st.Equals(SearchType.MARRIAGE))
                record_country = individual.BestLocation(factdate).Country;
            if (st.Equals(SearchType.DEATH) && Countries.IsKnownCountry(individual.DeathLocation.Country))
                record_country = individual.DeathLocation.Country;
            if (!Countries.IsKnownCountry(record_country))
                record_country = individual.BestLocation(factdate).Country;
            return record_country;
        }

        //string BuildFreeBMDQuery(SearchType st, Individual individual, FactDate factdate)
        //{
        //    throw new CensusSearchException("Not Yet"); // TODO: Add FreeBMD searching
        //}

        string BuildFindMyPastQuery(SearchType st, Individual individual, FactDate factdate, string bmdRegion)
        {
            UriBuilder uri = new UriBuilder
            {
                Host = $"search.findmypast{bmdRegion}"
            };
            string record_country = RecordCountry(st, individual, factdate);
            if (Countries.IsUnitedKingdom(record_country))
                uri.Path = "results/united-kingdom-records-in-birth-marriage-death-and-parish-records";
            else if (record_country == Countries.UNITED_STATES)
                uri.Path = "results/united-states-records-in-birth-marriage-death-and-parish-records";
            else if (record_country == Countries.NEW_ZEALAND || record_country == Countries.AUSTRALIA)
                uri.Path = "results/australia-and-new-zealand-records-in-birth-marriage-death-and-parish-records";
            else if (record_country == Countries.IRELAND)
                uri.Path = "results/ireland-records-in-birth-marriage-death-and-parish-records";
            else
                uri.Path = "results/world-records-in-birth-marriage-death-and-parish-records";
            if (st.Equals(SearchType.BIRTH))
                uri.Path += "/births-and-baptisms~church-registers";
            if (st.Equals(SearchType.MARRIAGE))
                uri.Path += "/church-registers~marriages-and-divorces";
            if (st.Equals(SearchType.DEATH))
                uri.Path += "/church-registers~wills-and-probate~deaths-and-burials";
            StringBuilder query = new StringBuilder();
            if (individual.Forenames != "?" && individual.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
                query.Append($"firstname={HttpUtility.UrlEncode(individual.Forenames)}&firstname_variants=true&");
            string surname = GetSurname(st, individual, false);
            query.Append($"lastname={HttpUtility.UrlEncode(surname)}&lastname_variants=true&");
            AppendYearandRange(individual.BirthDate, query, "yearofbirth=", "yearofbirth_offset=", true);
            if (st.Equals(SearchType.MARRIAGE))
                AppendYearandRange(factdate, query, "yearofmarriage=", "yearofmarriage_offset=", true);
            if (st.Equals(SearchType.DEATH))
                AppendYearandRange(factdate, query, "yearofdeath=", "yearofdeath_offset=", true);
            uri.Query = query.ToString();
            return uri.ToString();
        }

        static string GetSurname(SearchType st, Individual individual, bool ancestry)
        {
            string surname = string.Empty;
            if (individual is null) return surname;
            if (individual.Surname != "?" && individual.Surname.ToUpper() != Individual.UNKNOWN_NAME)
                surname = individual.Surname;
            if (st.Equals(SearchType.DEATH) && individual.MarriedName != "?" && individual.MarriedName.ToUpper() != Individual.UNKNOWN_NAME && individual.MarriedName != individual.Surname)
                surname = ancestry ? $"{surname} {individual.MarriedName}" : individual.MarriedName; // for ancestry combine names for others sites just use marriedName if death search
            surname = surname.Trim();
            return surname;
        }

        string BuildAncestryQuery(SearchType st, Individual individual, FactDate factdate, string bmdRegion)
        {
            UriBuilder uri = new UriBuilder
            {
                Host = $"search.ancestry{bmdRegion}",
                Path = "cgi-bin/sse.dll"
            };
            //gsln_x=NP&
            StringBuilder query = new StringBuilder();
            if (st.Equals(SearchType.BIRTH))
                query.Append("gl=BMD_BIRTH&");
            if (st.Equals(SearchType.MARRIAGE))
                query.Append("gl=BMD_MARRIAGE&");
            if (st.Equals(SearchType.DEATH))
                query.Append("gl=BMD_DEATH&");
            query.Append("gss=ms_f-34&");
            query.Append("rank=1&");
            query.Append("new=1&");
            query.Append("so=3&");
            query.Append("MSAV=1&");
            query.Append("msT=1&");
            if (individual.Forenames != "?" && individual.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
                query.Append($"gsfn={HttpUtility.UrlEncode(individual.Forenames)}&");
            string surname = GetSurname(st, individual, true);
            query.Append($"gsln={HttpUtility.UrlEncode(surname)}&");
            AppendYearandRange(individual.BirthDate, query, "msbdy=", "msbdp=", false);
            if (individual.BirthDate.IsKnown)
                query.Append("&msbdy_x=1");
            if (individual.BirthLocation.IsKnown)
            {
                string location = individual.BirthLocation.GetLocation(FactLocation.SUBREGION).ToString();
                query.Append($"msbpn__ftp={HttpUtility.UrlEncode(location)}&");
            }
            if (st.Equals(SearchType.DEATH) && factdate.IsKnown)
            {
                AppendYearandRange(factdate, query, "msddy=", "msddp=", false);
                query.Append("&msddy_x=1");
            }
            if (st.Equals(SearchType.MARRIAGE) && factdate.IsKnown)
            {
                AppendYearandRange(factdate, query, "msgdy=", "msgdp=", false);
                query.Append("&msgdy_x=1");
            }
            query.Append("cpxt=1&uidh=6b2&cp=11");
            uri.Query = query.ToString();
            return uri.ToString();
        }

        //string BuildGROQuery(SearchType st, Individual individual, FactDate factdate)
        //{
        //    if (st == SearchType.MARRIAGE)
        //        return null;
        //    UriBuilder uri = new UriBuilder
        //    {
        //        Host = "www.gro.gov.uk",
        //        Path = "gro/content/certificates/indexes_search.asp"
        //    };



        //    if (st.Equals(SearchType.BIRTH))
        //        query.Append("gl=BMD_BIRTH&");
        //    if (st.Equals(SearchType.DEATH))
        //        query.Append("gl=BMD_DEATH&");
        //    if (individual.Forenames != "?" && individual.Forenames.ToUpper() != Individual.UNKNOWN_NAME)
        //        query.Append("gsfn=" + HttpUtility.UrlEncode(individual.Forenames) + "&");
        //    string surname = GetSurname(st, individual, true);
        //    query.Append("gsln=" + HttpUtility.UrlEncode(surname) + "&");
        //    AppendYearandRange(individual.BirthDate, query, "msbdy=", "msbdp=", false);
        //    if (individual.BirthDate.IsKnown)
        //        query.Append("&msbdy_x=1");
        //    if (individual.BirthLocation != FactLocation.UNKNOWN_LOCATION)
        //    {
        //        string location = individual.BirthLocation.GetLocation(FactLocation.SUBREGION).ToString();
        //        query.Append("msbpn__ftp=" + HttpUtility.UrlEncode(location) + "&");
        //    }
        //    if (st.Equals(SearchType.DEATH) && factdate.IsKnown)
        //    {
        //        AppendYearandRange(factdate, query, "msddy=", "msddp=", false);
        //        query.Append("&msddy_x=1");
        //    }
        //    if (st.Equals(SearchType.MARRIAGE) && factdate.IsKnown)
        //    {
        //        AppendYearandRange(factdate, query, "msgdy=", "msgdp=", false);
        //        query.Append("&msgdy_x=1");
        //    }

        //    return uri.ToString();
        //}

        static void AppendYearandRange(FactDate factdate, StringBuilder query, string yeartext, string rangetext, bool FMP)
        {
            if (factdate.IsKnown)
            {
                int startYear = factdate.StartDate.Year;
                int endYear = factdate.EndDate.Year;
                int year, range;
                if (startYear == FactDate.MINDATE.Year)
                {
                    year = endYear - (FMP ? 39 : 9);
                    range = FMP ? 40 : 10;
                }
                else if (endYear == FactDate.MAXDATE.Year)
                {
                    year = startYear + (FMP ? 39 : 9);
                    range = FMP ? 40 : 10;
                }
                else
                {
                    year = (endYear + startYear + 1) / 2;
                    range = (endYear - startYear + 2) / 2; // add two to make year range searches always at least one year either side
                    if (2 < range && range < 5) range = 5;
                    if (range > 5 && !FMP) range = 10;
                    if (FMP)
                    {
                        if (5 < range && range < 10) range = 10;
                        if (10 < range && range < 20) range = 20;
                        if (range > 20) range = 40;
                    }
                }
                query.Append(yeartext + year + "&");
                query.Append(rangetext + range + "&");
            }
        }

        #endregion

        #region Geocoding

        public static void WriteGeocodeStatstoRTB(string title, IProgress<string> outputText)
        {
            outputText.Report($"\n{title}");
            // write geocode results - ignore UNKNOWN entry
            int notsearched = FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.NOT_SEARCHED));
            int needsReverse = FactLocation.AllLocations.Count(x => x.NeedsReverseGeocoding);
            //Predicate<FactLocation> predicate = x => x.NeedsReverseGeocoding;
            //List<FactLocation> needRev = FactLocation.AllLocations.Where(predicate).ToList();
            outputText.Report($"\n{FactLocation.GEDCOMLocationsCount} locations and addresses loaded from GEDCOM file.\n");
            outputText.Report($"    {FactLocation.GEDCOMGeocodedCount} have Lat/Long coordinates in the file.\n");
            outputText.Report($"{FactLocation.LocationsCount} locations in use after processing file and generating extra locations for tree view.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.GEDCOM_USER) && x.FoundLocation.Length > 0)} are GEDCOM/User Entered and have been geocoded.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.GEDCOM_USER) && x.FoundLocation.Length == 0)} are GEDCOM/User Entered but lack a Google Location.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.MATCHED))} have a geocoding match from Google.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.OS_50KMATCH))} have a geocoding match from Ordnance Survey.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.OS_50KFUZZY))} have a fuzzy geocoding match from Ordnance Survey.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.PARTIAL_MATCH))} have partial geocoding match from Google.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.LEVEL_MISMATCH))} have partial geocoding match at lower level of detail.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.OS_50KPARTIAL))} have partial geocoding match from Ordnance Survey.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.OUT_OF_BOUNDS))} found by Google but outside country boundary.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.INCORRECT))} marked as incorrect by user.\n");
            outputText.Report($"    {FactLocation.AllLocations.Count(x => x.GeocodeStatus.Equals(FactLocation.Geocode.NO_MATCH))} could not be found on Google.\n");
            outputText.Report($"    {notsearched} haven't been searched.");
            if (notsearched > 0)
                outputText.Report(" Use the 'Run Google/OS Geocoder' option (under Maps menu) to find them.\n");
            if (needsReverse > 0)
            {
                outputText.Report($"\nNote {needsReverse} of the searched locations are missing a Google location.");
                outputText.Report(" Use the 'Lookup Blank Google Locations' option (under Maps menu) to find them.\n");
            }
        }

        #endregion

        #region Relationship Groups
        public static List<Individual> GetFamily(Individual startIndividiual)
        {
            List<Individual> results = new List<Individual>();
            if (startIndividiual is object) // checks not null
            {
                foreach (Family f in startIndividiual.FamiliesAsSpouse)
                {
                    foreach (Individual i in f.Members)
                        results.Add(i);
                }
                foreach (ParentalRelationship pr in startIndividiual.FamiliesAsChild)
                {
                    foreach (Individual i in pr.Family.Members)
                        results.Add(i);
                }
            }
            return results;
        }

        public static List<Individual> GetAncestors(Individual startIndividual)
        {
            List<Individual> results = new List<Individual>();
            Queue<Individual> queue = new Queue<Individual>();
            results.Add(startIndividual);
            queue.Enqueue(startIndividual);
            while (queue.Count > 0)
            {
                Individual ind = queue.Dequeue();
                foreach (ParentalRelationship parents in ind.FamiliesAsChild)
                {
                    if (parents.IsNaturalFather)
                    {
                        queue.Enqueue(parents.Father);
                        results.Add(parents.Father);
                    }
                    if (parents.IsNaturalMother)
                    {
                        queue.Enqueue(parents.Mother);
                        results.Add(parents.Mother);
                    }
                }
            }
            return results;
        }

        public static List<Individual> GetDescendants(Individual startIndividual)
        {
            List<Individual> results = new List<Individual>();
            Dictionary<string, Individual> processed = new Dictionary<string, Individual>();
            Queue<Individual> queue = new Queue<Individual>();
            results.Add(startIndividual);
            queue.Enqueue(startIndividual);
            while (queue.Count > 0)
            {
                Individual parent = queue.Dequeue();
                processed.Add(parent.IndividualID, parent);
                foreach (Family f in parent.FamiliesAsSpouse)
                {
                    Individual spouse = f.Spouse(parent);
                    if (spouse != null && !processed.ContainsKey(spouse.IndividualID))
                    {
                        queue.Enqueue(spouse);
                        results.Add(spouse);
                    }
                    foreach (Individual child in f.Children)
                    {
                        // we have a child and we have a parent check if natural child
                        if (!processed.ContainsKey(child.IndividualID) && child.IsNaturalChildOf(parent))
                        {
                            queue.Enqueue(child);
                            results.Add(child);
                        }
                    }
                }
            }
            return results;
        }

        public static List<Individual> GetAllRelations(Individual ind) => GetFamily(ind).Union(GetAncestors(ind).Union(GetDescendants(ind))).ToList();
        #endregion

        #region Duplicates Processing
        long totalComparisons;
        long maxComparisons;
        long duplicatesFound;
        int currentPercentage;

        public async Task<SortableBindingList<IDisplayDuplicateIndividual>> GenerateDuplicatesList(int value, bool ignoreUnknown, IProgress<int> progress, IProgress<string> progressText, IProgress<int> maximum, CancellationToken ct)
        {
            if (duplicates != null)
            {
                maximum.Report(MaxDuplicateScore());
                return BuildDuplicateList(value, progress, progressText); // we have already processed the duplicates since the file was loaded
            }
            var groups = individuals.Where(x => x.Name != Individual.UNKNOWN_NAME).GroupBy(x => x.SurnameMetaphone).Select(x => x.ToList()).ToList();
            int numgroups = groups.Count;
            progress.Report(0);
            totalComparisons = 0;
            maxComparisons = groups.Sum(x => x.Count * (x.Count - 1L) / 2);
            currentPercentage = 0;
            duplicatesFound = 0;
            buildDuplicates = new ConcurrentBag<DuplicateIndividual>();
            var tasks = new List<Task>();
            try
            {
                foreach (var group in groups)
                {
                    var task = Task.Run(() => IdentifyDuplicates(ignoreUnknown, group, ct), ct);
                    tasks.Add(task);
                }
                var progressTask = Task.Run(() => ProgressReporter(progress, progressText, ct), ct);
                tasks.Add(progressTask);
                await Task.WhenAll(tasks).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                progress.Report(0); // if user cancels then simply clear progress do not throw away work done
            }
            catch (Exception e)
            {
                UIHelpers.ShowMessage($"Duplicate report encountered a problem. Message was: {e.Message}");
            }
            try
            {
                duplicates = new SortableBindingList<DuplicateIndividual>(buildDuplicates.ToList());
                maximum.Report(MaxDuplicateScore());
                DeserializeNonDuplicates();
                return BuildDuplicateList(value, progress, progressText);
            }
            catch (Exception e)
            {
                UIHelpers.ShowMessage($"Duplicate report encountered a problem. Message was: {e.Message}");
            }
            return null;
        }

        int MaxDuplicateScore()
        {
            int score = 0;
            foreach (DuplicateIndividual dup in buildDuplicates)
            {
                if (dup != null && dup.Score > score)
                    score = dup.Score;
            }
            return score;
        }

        void IdentifyDuplicates(bool ignoreUnknown, IList<Individual> list, CancellationToken ct)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var indA = list[i];
                for (var j = i + 1; j < list.Count; j++)
                {
                    var indB = list[j];
                    if ((indA.ForenameMetaphone.Equals(indB.ForenameMetaphone) || indA.StandardisedName.Equals(indB.StandardisedName)) &&
                       indA.BirthDate.DistanceSquared(indB.BirthDate) < 5)
                    {
                        var test = new DuplicateIndividual(indA, indB);
                        if (test.Score > 0)
                        {
                            buildDuplicates.Add(test);
                            Interlocked.Increment(ref duplicatesFound);
                        }
                    }
                    Interlocked.Increment(ref totalComparisons);
                    if (ct.IsCancellationRequested)
                        return;
                }
            }
        }

        void ProgressReporter(IProgress<int> progress, IProgress<string> progressText, CancellationToken ct)
        {
            while (totalComparisons < maxComparisons && currentPercentage < 100)
            {
                Task.Delay(1000);
                ct.ThrowIfCancellationRequested();
                var val = (int)(100 * totalComparisons / maxComparisons);
                if (val > currentPercentage)
                {
                    currentPercentage = val;
                    if (val < 100)
                        progressText.Report($"Done {totalComparisons:N0} of {maxComparisons:N0} - {val}%\nFound {duplicatesFound:N0} possible duplicates");
                    else
                        progressText.Report($"Completed {duplicatesFound:N0} possible duplicates found. Preparing display.");
                    progress.Report(val);
                }
            }
        }

        public SortableBindingList<IDisplayDuplicateIndividual> BuildDuplicateList(int minScore, IProgress<int> progress, IProgress<string> progressText)
        {
            var select = new SortableBindingList<IDisplayDuplicateIndividual>();
            long numDuplicates = duplicates.Count;
            long numProcessed = 0;
            currentPercentage = 0;
            progress.Report(0);
            progressText.Report("Preparing Display");
            if (NonDuplicates is null)
                DeserializeNonDuplicates();
            foreach (DuplicateIndividual dup in duplicates)
            {
                if (dup.Score >= minScore)
                {
                    var dispDup = new DisplayDuplicateIndividual(dup);
                    var toCheck = new NonDuplicate(dispDup);
                    dispDup.IgnoreNonDuplicate = NonDuplicates.ContainsDuplicate(toCheck);
                    if (!(GeneralSettings.Default.HideIgnoredDuplicates && dispDup.IgnoreNonDuplicate))
                        select.Add(dispDup);
                }
                numProcessed++;
                if(numProcessed % 20 == 0)
                {
                    var val = (int)(100 * numProcessed / numDuplicates);
                    if (val > currentPercentage)
                    {
                        currentPercentage = val;
                        progressText.Report($"Preparing Display. {numProcessed:N0} of {numDuplicates:N0} - {val}%");
                        progress.Report(val);
                    }
                }
            }
            progressText.Report("Prepared records. Sorting - Please wait");
            return select;
        }

        public void SerializeNonDuplicates()
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                string file = Path.Combine(GeneralSettings.Default.SavePath, "NonDuplicates.xml");
                using (Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, NonDuplicates);
                }
            }
            catch
            {
                // log.Error($"Error {e.Message} writing NonDuplicates.xml");
            }
        }

        public void DeserializeNonDuplicates()
        {
            // log.Debug("FamilyTree.DeserializeNonDuplicates");
            try
            {
                IFormatter formatter = new BinaryFormatter();
                string file = Path.Combine(GeneralSettings.Default.SavePath, "NonDuplicates.xml");
                if (File.Exists(file))
                {
                    using (Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        NonDuplicates = (List<NonDuplicate>)formatter.Deserialize(stream);
                    }
                }
                else
                    NonDuplicates = new List<NonDuplicate>();
            }
            catch
            {
                //  log.Error("Error " + e.Message + " reading NonDuplicates.xml");
                NonDuplicates = new List<NonDuplicate>();
            }
        }
        #endregion

        #region Report Issues
        public HashSet<string> UnrecognisedCensusReferences()
        {
            var result = new HashSet<string>();
            IEnumerable<Fact> unrecognised = AllIndividuals.SelectMany(x => x.PersonalFacts.Filter(f => f.IsCensusFact && f.CensusReference != null && f.CensusReference.Status.Equals(CensusReference.ReferenceStatus.UNRECOGNISED)));
            foreach (Fact f in unrecognised)
                result.Add(CensusReference.ClearCommonPhrases(f.CensusReference.Reference));
            return result;
        }

        public HashSet<string> MissingCensusReferences()
        {
            var result = new HashSet<string>();
            IEnumerable<Fact> missing = AllIndividuals.SelectMany(x => x.PersonalFacts.Filter(f => f.IsCensusFact && f.CensusReference != null && f.CensusReference.Status.Equals(CensusReference.ReferenceStatus.BLANK)));
            foreach (Fact f in missing)
                result.Add(CensusReference.ClearCommonPhrases(f.SourceList)); // for missing census references show sources for census fact
            return result;
        }

        public HashSet<string> UnrecognisedCensusReferencesNotes()
        {
            var result = new HashSet<string>();
            IEnumerable<Individual> unrecognised = AllIndividuals.Filter(i => i.UnrecognisedCensusNotes.Length > 0);
            foreach (Individual i in unrecognised)
                result.Add(i.UnrecognisedCensusNotes);
            return result;
        }

        public static void WriteUnrecognisedReferencesFile(IEnumerable<string> unrecognisedResults, IEnumerable<string> missingResults, IEnumerable<string> notesResults, string filename)
        {
            using (StreamWriter output = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write), Encoding.UTF8))
            {
                int count = 0;
                output.WriteLine("Note the counts on the loading page may not match the counts in the file as duplicates not written out each time\n");
                if (unrecognisedResults.Any())
                {
                    output.WriteLine("Census fact details where a Census reference was expected but went unrecognised");
                    unrecognisedResults = unrecognisedResults.OrderBy(x => x.ToString());
                    foreach (string line in unrecognisedResults)
                        output.WriteLine($"{++count}: {line}");
                }
                if (missingResults.Any())
                {
                    count = 0;
                    output.WriteLine("\n\nCensus fact details where a Census Reference was missing or not detected");
                    missingResults = missingResults.OrderBy(x => x.ToString());
                    foreach (string line in missingResults)
                        output.WriteLine($"{++count}: {line}");
                }
                if (notesResults.Any())
                {
                    count = 0;
                    output.WriteLine("\n\nNotes with no census recognised references\nThese are usually NOT census references and are included in case there are some that got missed");
                    notesResults = notesResults.OrderBy(x => x.ToString());
                    foreach (string line in notesResults)
                        output.WriteLine($"{++count}: {line}");
                }
            }
        }

        #endregion

        #region Today
        public void AddTodaysFacts(DateTime chosenDate, bool wholeMonth, int stepSize, IProgress<int> progress, IProgress<string> outputText)
        {
            string dateDesc;
            var sb = new StringBuilder();
            if (wholeMonth)
            {
                dateDesc = chosenDate.ToString("MMMM");
                sb.Append(@"{\rtf1\ansi \b GEDCOM and World Events in " + dateDesc + @"\b0.\line\line ");
            }
            else
            {
                dateDesc = chosenDate.ToString("d MMMM");
                sb.Append(@"{\rtf1\ansi \b GEDCOM and World Events on " + dateDesc + @"\b0.\line\line ");
            }
            var todaysFacts = new List<DisplayFact>();
            int indCount = IndividualCount;
            int count = 0;
            foreach (Individual i in individuals)
            {
                foreach (Fact f in i.AllFacts)
                    if (!f.Created && !f.IsCensusFact && f.FactType != Fact.OCCUPATION && f.FactDate.IsExact && f.FactDate.StartDate.Month == chosenDate.Month)
                        if (wholeMonth || f.FactDate.StartDate.Day == chosenDate.Day)
                            todaysFacts.Add(new DisplayFact(i, f));
                progress.Report((30 * count) / indCount);
            }
            todaysFacts.Sort(); // need to sort facts to get correct earliest date
            if (GeneralSettings.Default.ShowWorldEvents)
            {
                int earliestYear = todaysFacts.Count > 0 ? todaysFacts[0].FactDate.StartDate.Year : 1752; // if no facts show world events for Gregorian calendar to today
                List<DisplayFact> worldEvents = AddWorldEvents(earliestYear, chosenDate, wholeMonth, stepSize, progress);
                todaysFacts.AddRange(worldEvents);
                todaysFacts.Sort();
            }
            foreach (DisplayFact f in todaysFacts)
                sb.Append(f + @"\line ");
            sb.Append('}');
            outputText.Report(sb.ToString());
            progress.Report(100);
        }

        public List<DisplayFact> AddWorldEvents(int earliestYear, DateTime chosenDate, bool wholeMonth, int stepSize, IProgress<int> progress)
        {
            // use Wikipedia API at vizgr.org/historical-events/ to find what happened on that date in the past
            var events = new List<DisplayFact>();
            string URL;
            FactDate eventDate;
            int barMinimum = earliestYear;
            int barRange = chosenDate.Year - earliestYear;
            progress.Report(50);
            for (int year = earliestYear; year <= chosenDate.Year; year++)
            {
                int diff = chosenDate.Year - year;
                if (diff % stepSize == 0)
                {
                    URL = wholeMonth ?
                            @"http://www.vizgr.org/historical-events/search.php?links=true&format=xml&begin_date=" + year.ToString() + chosenDate.ToString("MM", CultureInfo.InvariantCulture) + "00" +
                             "&end_date=" + year.ToString() + chosenDate.ToString("MM", CultureInfo.InvariantCulture) + "31" :
                            @"http://www.vizgr.org/historical-events/search.php?links=true&format=xml&begin_date=" + year.ToString() + chosenDate.ToString("MMdd", CultureInfo.InvariantCulture) +
                            "&end_date=" + year.ToString() + chosenDate.ToString("MMdd", CultureInfo.InvariantCulture);
                    XmlDocument doc = GetWikipediaData(URL);
                    eventDate = wholeMonth ? new FactDate(CreateDate(year, chosenDate.Month, 1), CreateDate(year, chosenDate.Month + 1, 1).AddDays(-1)) :
                                             new FactDate(CreateDate(year, chosenDate.Month, chosenDate.Day), CreateDate(year, chosenDate.Month, chosenDate.Day));
                    if (doc.InnerText.Length > 0)
                    {
                        FactDate fd;
                        XmlNodeList nodes = doc.SelectNodes("/result/event");
                        foreach (XmlNode worldEvent in nodes)
                        {
                            XmlNode descNode = worldEvent.SelectSingleNode("description");
                            string desc = FixWikiFormatting(descNode.InnerText);
                            XmlNode dateNode = worldEvent.SelectSingleNode("date");
                            fd = GetWikiDate(dateNode, eventDate);
                            var f = new Fact("Wikipedia", Fact.WORLD_EVENT, fd, FactLocation.UNKNOWN_LOCATION, desc, true, true);
                            var df = new DisplayFact(null, string.Empty, string.Empty, f);
                            events.Add(df);
                        }
                    }
                }
                progress.Report(30 + (70 * (year - barMinimum)) / barRange);
            }
            return events;
        }

        static readonly Regex brackets = new Regex("{{.*}}", RegexOptions.Compiled);
        static readonly Regex links = new Regex("<a href=.*</a>", RegexOptions.Compiled);
        static readonly Regex quotes = new Regex("(.*)quot(.*)quot(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        string FixWikiFormatting(string input)
        {
            string result = input.Replace("ampampnbsp", " ").Replace("ampnbsp", " ").Replace("ampampndash", "-").Replace("ampndash", "-");
            //strip out {{cite xxxxx }} citation text with its urls
            result = brackets.Replace(result, string.Empty);
            result = links.Replace(result, string.Empty);
            Match match = quotes.Match(result);
            if (match.Success)
                result = match.Groups[1].ToString().Trim() + " " + '\u0022' + match.Groups[2].ToString().Trim() + '\u0022' + " " + match.Groups[3].ToString().Trim();
            return result;
        }

        static FactDate GetWikiDate(XmlNode dateNode, FactDate defaultDate)
        {
            FactDate fd;
            try
            {
                string[] dateFields = dateNode.InnerText.Split(new Char[] { '/' });
                int nodeyear = int.Parse(dateFields[0]);
                int nodemonth = int.Parse(dateFields[1]);
                int nodeday = int.Parse(dateFields[2]);
                fd = new FactDate(new DateTime(nodeyear, nodemonth, nodeday).ToString("dd MMM yyyy"));
            }
            catch (Exception)
            {
                fd = defaultDate;
            }
            return fd;
        }

        XmlDocument GetWikipediaData(string URL)
        {
            string result = string.Empty;
            var doc = new XmlDocument() { XmlResolver = null };
            try
            {
                //doc.Load(URL); // using doc.load throws XmlException slowing down loading of data
                HttpWebRequest request = WebRequest.Create(new Uri(URL)) as HttpWebRequest;
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                Encoding encode = Encoding.GetEncoding("utf-8");
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream(), encode))
                    {
                        result = reader.ReadToEnd();
                        if (!result.Contains("No events found for this query"))
                        {
                            //XmlReader xmlReader = XmlReader.Create(result, new XmlReaderSettings() { XmlResolver = null })
                            using (XmlTextReader xmlReader = new XmlTextReader(new StringReader(result)))
                                doc.Load(xmlReader);
                        }
                    }
                }
            }
            catch (XmlException)
            {
                // we have an empty result so we can just accept that and return an empty document.
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //log.Error($"Error trying to load data from {URL}\n\n{e.Message}");
            }
            return doc;
        }
        #endregion

        #region WorldWars

        public IEnumerable<IDisplayIndividual> GetWorldWars(Predicate<Individual> filter) => individuals.Filter(ind => ind.IsMale && filter(ind));
        public IEnumerable<IExportIndividual> GetExportWorldWars(Predicate<Individual> filter) => individuals.Filter(ind => ind.IsMale && filter(ind));

        #endregion

        #region TreeTops

        public IEnumerable<IDisplayIndividual> GetTreeTops(Predicate<Individual> filter) => individuals.Filter(ind => filter(ind));
        public IEnumerable<IExportIndividual> GetExportTreeTops(Predicate<Individual> filter) => individuals.Filter(ind => filter(ind));

        #endregion

    }
}
