using FTAnalyzer.Properties;
using FTAnalyzer.Utilities;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace FTAnalyzer
{
    public class CensusReference : IComparable<CensusReference>
    {
        public enum ReferenceStatus { BLANK = 0, UNRECOGNISED = 1, INCOMPLETE = 2, GOOD = 3 };
        public static readonly CensusReference UNKNOWN = new();
        public const string MISSING = "Missing";

        string unknownCensusRef;
        string Place { get; set; }
        public string Class { get; internal set; }
        public string Roll { get; internal set; }
        public string Piece { get; internal set; }
        public string Folio { get; internal set; }
        public string Page { get; internal set; }
        public string Book { get; internal set; }
        public string Schedule { get; internal set; }
        public string Parish { get; internal set; }
        public string RD { get; set; }
        public string ED { get; internal set; }
        public string SD { get; internal set; }
        public string Family { get; internal set; }
        public string ReferenceText { get; set; }
        CensusLocation CensusLocation { get; set; }
        public Fact? Fact { get; private set; }
        public bool IsUKCensus { get; private set; }
        public bool IsLCCensusFact { get; private set; }
        public ReferenceStatus Status { get; internal set; }
        public FactDate CensusYear { get; private set; }
        public string MatchString { get; private set; }
        public string Country { get; private set; }
        public string URL { get; private set; }

        internal CensusReference()
        {
            Class = string.Empty;
            Roll = string.Empty;
            Place = string.Empty;
            Piece = string.Empty;
            Folio = string.Empty;
            Book = string.Empty;
            Page = string.Empty;
            Schedule = string.Empty;
            Parish = string.Empty;
            RD = string.Empty;
            ED = string.Empty;
            SD = string.Empty;
            Family = string.Empty;
            ReferenceText = string.Empty;
            IsUKCensus = false;
            IsLCCensusFact = false;
            Status = ReferenceStatus.BLANK;
            unknownCensusRef = string.Empty;
            MatchString = string.Empty;
            Country = Countries.UNKNOWN_COUNTRY;
            URL = string.Empty;
            CensusYear = FactDate.UNKNOWN_DATE;
            CensusLocation = CensusLocation.UNKNOWN;
        }

        public CensusReference(Fact fact, XmlNode node, CensusReference? pageRef = null)
            : this()
        {
            Fact = fact;
            if (GetCensusReference(node))
                SetCensusReferenceDetails();
            if (Status != ReferenceStatus.GOOD)
            {
                var tempStatus = Status;
                if (GetCensusReference(Fact.Comment))
                    SetCensusReferenceDetails();
                if ((Status == ReferenceStatus.BLANK || Status == ReferenceStatus.UNRECOGNISED) && tempStatus == ReferenceStatus.INCOMPLETE)
                    Status = tempStatus; // if we found an incomplete status don't throw that away if comment had no status
            }
            if (fact.FactDate.IsKnown)
            {
                if (CensusYear.IsKnown && !fact.FactDate.Overlaps(CensusYear))
                {
                    if (CensusYear == CensusDate.USCENSUS1940 && !fact.FactDate.Overlaps(FactDate.YEAR1935)) // allow 1940 census reference to refer to a 1935 residence fact
                        fact.SetError((int)FamilyTree.Dataerror.FACT_ERROR, Fact.FactError.WARNINGALLOW, $"Census Fact dated {fact.FactDate} doesn't match census reference {Reference} date of {CensusYear}");
                }
                else
                    CensusYear = fact.FactDate;
            }
            else
                fact.UpdateFactDate(CensusYear);
            if (pageRef is not null && !pageRef.IsKnownStatus && !IsKnownStatus)
                unknownCensusRef = $"{pageRef.unknownCensusRef}\n{unknownCensusRef}";
            fact.SetCensusReferenceDetails(this, CensusLocation, string.Empty);
        }

        public CensusReference(string notes, bool source)
            : this()
        {
            Fact = new Fact(Fact.CENSUS_FTA, FactDate.UNKNOWN_DATE, FactLocation.UNKNOWN_LOCATION, string.Empty, false, true);
            if (GetCensusReference(notes))
            {
                if (Class.Length > 0)
                {  // don't create fact if we don't know what class it is
                    SetCensusReferenceDetails();
                    Fact.UpdateFactDate(CensusYear);
                    if (source)
                        Fact.SetCensusReferenceDetails(this, CensusLocation, $"Fact created by FTAnalyzer after finding census ref: {MatchString} in a source for this individual");
                    else
                        Fact.SetCensusReferenceDetails(this, CensusLocation, $"Fact created by FTAnalyzer after finding census ref: {MatchString} in the notes for this individual");
                }
            }
        }

        public CensusReference(string text, IProgress<string> output)
            : this()
        {
            CheckPatterns(text, output);
        }

        void SetCensusReferenceDetails()
        {
            unknownCensusRef = string.Empty;
            if (Class.Equals("SCOT", StringComparison.OrdinalIgnoreCase))
            {
                URL = GetCensusURLFromReference();
                CensusLocation = CensusLocation.SCOTLAND;
                if (Parish.Length > 0)
                {
                    ScottishParish sp = ScottishParish.FindParishFromID(Parish);
                    if (sp != ScottishParish.UNKNOWN_PARISH)
                        CensusLocation = new CensusLocation(string.Empty, string.Empty, sp.RegistrationDistrict, sp.Name, sp.Region, sp.Location.ToString());
                }
            }
            else if (Class.StartsWith("US", StringComparison.Ordinal))
            {
                CensusYear = GetCensusYearFromReference();
                if (Place.Length > 0)
                    CensusLocation = new CensusLocation(Place);
                else
                    CensusLocation = CensusLocation.UNITED_STATES;
            }
            else if (Class.StartsWith("CAN", StringComparison.Ordinal))
            {
                CensusYear = GetCensusYearFromReference();
                if (Place.Length > 0)
                    CensusLocation = new CensusLocation(Place);
                else
                    CensusLocation = CensusLocation.CANADA;
            }
            else
            {
                CensusYear = GetCensusYearFromReference();
                if (CensusYear.StartDate.Year == 1921)
                    CensusLocation = CensusLocation.Get1921CensusLocation(RD, SD);
                else
                    CensusLocation = CensusLocation.GetCensusLocation(CensusYear.StartDate.Year.ToString(), Piece);
                URL = GetCensusURLFromReference();
            }
        }

        bool GetCensusReference(XmlNode n)
        {
            if (GeneralSettings.Default.SkipCensusReferences)
                return false;
            bool pageCheck;
            bool dataCheck;
            bool childCheck;
            bool footnoteCheck;
            bool sourcetextCheck;
            string text = FamilyTree.GetText(n, "PAGE", true);
            pageCheck = GetCensusReference(text, true);
            if (pageCheck && Status == ReferenceStatus.GOOD)
                return true;
            text = FamilyTree.GetText(n, "DATA", true);
            dataCheck = GetCensusReference(text, false);
            if (dataCheck && Status == ReferenceStatus.GOOD)
                return true;
            text = FamilyTree.GetText(n, "_FOOT", true);
            footnoteCheck = GetCensusReference(text, false);
            if (footnoteCheck && Status == ReferenceStatus.GOOD)
                return true;
            text = FamilyTree.GetText(n, true);
            childCheck = GetCensusReference(text, false);
            if (childCheck && Status == ReferenceStatus.GOOD)
                return true;
            text = FamilyTree.GetText(n, "SOUR", true);
            sourcetextCheck = GetCensusReference(text, false);
            if (sourcetextCheck && Status == ReferenceStatus.GOOD)
                return true;
            text = FamilyTree.GetNotes(n);
            return pageCheck || dataCheck || GetCensusReference(text, false); // if any of the checks worked but were incomplete return true
        }

        bool GetCensusReference(string text, bool checksources = true, bool updateUnknownRef = true, ReferenceStatus oldstatus = ReferenceStatus.BLANK)
        {
            if (GeneralSettings.Default.SkipCensusReferences)
                return false;
            if (text.Length > 7) // needs to be at least 8 chars for a valid reference
            {
                if (CheckPatterns(text))
                {
                    ReferenceText = text.Trim();
                    return true;
                }
                else if (oldstatus == ReferenceStatus.BLANK)
                    // no match so store text 
                    Status = ReferenceStatus.UNRECOGNISED;
                if (updateUnknownRef)
                {
                    if (unknownCensusRef.Length == 0)
                        unknownCensusRef = $"Unknown Census Ref: {text}";
                    else
                        unknownCensusRef += $" {text}";
                }
            }
            if (checksources && Fact is not null)
            {
                // now check sources to see if census reference is in title page
                foreach (FactSource fs in Fact.Sources)
                {
                    if (CheckPatterns(fs.SourceTitle))
                    {
                        ReferenceText = fs.SourceTitle;
                        return true;
                    }
                    if (CheckPatterns(fs.Publication))
                    {
                        ReferenceText = fs.Publication;
                        return true;
                    }
                }
            }
            return false;
        }

        public void CheckFullUnknownReference(ReferenceStatus status) => GetCensusReference(UnknownRef, false, false, status);

        string UnknownRef => unknownCensusRef.Length > 20 ? unknownCensusRef[20..] : string.Empty;

        public static string ClearCommonPhrases(string input)
        {
            string output = HttpUtility.UrlDecode(input) // fixes issues with web formatted text
                                .Replace(".", " ", StringComparison.Ordinal).Replace(",", " ", StringComparison.Ordinal).Replace("(", " ", StringComparison.Ordinal)
                                .Replace(")", " ", StringComparison.Ordinal).Replace("{", " ", StringComparison.Ordinal).Replace("}", " ", StringComparison.Ordinal)
                                .Replace("«b»", " ", StringComparison.Ordinal).Replace("«i»", " ", StringComparison.Ordinal).Replace("«/b»", " ", StringComparison.Ordinal)
                                .Replace("«/i»", " ", StringComparison.Ordinal).Replace(@"\i", " ", StringComparison.Ordinal).Replace(@"\i0", " ", StringComparison.Ordinal)
                                .Replace("&nbsp", " ", StringComparison.Ordinal).Replace(";", " ", StringComparison.Ordinal).Replace(@"<b>", " ", StringComparison.Ordinal)
                                .Replace(@"</b>", " ", StringComparison.Ordinal).Replace(@"<i>", " ", StringComparison.Ordinal).Replace(@"</i>", " ", StringComparison.Ordinal)
                                .ClearWhiteSpace();
            return output.Replace("Registration District", "RD", StringComparison.OrdinalIgnoreCase)
                        .Replace("RegistrationDistrict", "RD", StringComparison.OrdinalIgnoreCase)
                        .Replace("Reg District", "RD", StringComparison.OrdinalIgnoreCase)
                        .Replace("Pg", "Page", StringComparison.OrdinalIgnoreCase)
                        .Replace("PN", "Piece", StringComparison.OrdinalIgnoreCase)
                        .Replace("Schedule No", "SN", StringComparison.OrdinalIgnoreCase)
                        .Replace("Schedule Number", "SN", StringComparison.OrdinalIgnoreCase)
                        .Replace("Schedule", "SN", StringComparison.OrdinalIgnoreCase)
                        .Replace("Sch", "SN", StringComparison.OrdinalIgnoreCase)
                        .Replace("ED institution or vessel", "ED", StringComparison.OrdinalIgnoreCase)
                        .Replace("Enumeration District ED", "ED", StringComparison.OrdinalIgnoreCase)
                        .Replace("Enumeration District", "ED", StringComparison.OrdinalIgnoreCase)
                        .Replace("EnumerationDistrict", "ED", StringComparison.OrdinalIgnoreCase)
                        .Replace("Sub District", "SD", StringComparison.OrdinalIgnoreCase)
                        .Replace("Sub-District", "SD", StringComparison.OrdinalIgnoreCase)
                        .Replace("Sheet number and letter", "Page", StringComparison.OrdinalIgnoreCase)
                        .Replace("Sheet", "Page", StringComparison.OrdinalIgnoreCase)
                        .Replace("Affiliate Film Number", " ", StringComparison.OrdinalIgnoreCase)
                        .Replace("Family History Film", "Film ", StringComparison.OrdinalIgnoreCase)
                        .Replace("FamilyHistory Film", "Film ", StringComparison.OrdinalIgnoreCase)
                        .Replace("Place", " ", StringComparison.OrdinalIgnoreCase)
                        .Replace("Family Number", "Family", StringComparison.OrdinalIgnoreCase)
                        .Replace("Family No", "Family", StringComparison.OrdinalIgnoreCase)
                        .Replace("Page Number", "Page", StringComparison.OrdinalIgnoreCase)
                        .Replace("Page No", "Page", StringComparison.OrdinalIgnoreCase)
                        .Replace("Book No", "Book", StringComparison.OrdinalIgnoreCase)
                        .Replace("Book Number", "Book", StringComparison.OrdinalIgnoreCase)
                        .Replace("Folio No", "Folio", StringComparison.OrdinalIgnoreCase)
                        .Replace("Folio Number", "Folio", StringComparison.OrdinalIgnoreCase)
                        .Replace("Piece Number", "Piece", StringComparison.OrdinalIgnoreCase)
                        .Replace("Piece No", "Piece", StringComparison.OrdinalIgnoreCase)
                        .ClearWhiteSpace();
        }

        static void WriteTimer(string patternName, string text, IProgress<string> output)
        {
            if (output is null)
                return;
            Stopwatch timer = Stopwatch.StartNew();
            Match matcher = patternName switch
            {
                "EW_CENSUS_PATTERN" => RegexPatterns.EwCensusPattern().Match(text),
                "EW_CENSUS_PATTERN1" => RegexPatterns.EwCensusPattern1().Match(text),
                "EW_CENSUS_PATTERN_SN" => RegexPatterns.EwCensusPatternSn().Match(text),
                "EW_CENSUS_PATTERN2" => RegexPatterns.EwCensusPattern2().Match(text),
                "EW_CENSUS_PATTERN_FH" => RegexPatterns.EwCensusPatternFh().Match(text),
                "EW_CENSUS_PATTERN_FH2" => RegexPatterns.EwCensusPatternFh2().Match(text),
                "EW_CENSUS_PATTERN_FH3" => RegexPatterns.EwCensusPatternFh3().Match(text),
                "EW_CENSUS_PATTERN_FS1" => RegexPatterns.EwCensusPatternFs1().Match(text),
                "EW_CENSUS_1841_51_PATTERN" => RegexPatterns.EwCensus184151Pattern().Match(text),
                "EW_CENSUS_1841_51_PATTERN2" => RegexPatterns.EwCensus184151Pattern2().Match(text),
                "EW_CENSUS_1841_51_PATTERN2A" => RegexPatterns.EwCensus184151Pattern2A().Match(text),
                "EW_CENSUS_1841_51_PATTERN3" => RegexPatterns.EwCensus184151Pattern3().Match(text),
                "EW_CENSUS_1841_51_PATTERN4" => RegexPatterns.EwCensus184151Pattern4().Match(text),
                "EW_CENSUS_1841_51_PATTERN5" => RegexPatterns.EwCensus184151Pattern5().Match(text),
                "EW_CENSUS_1841_51_PATTERN6" => RegexPatterns.EwCensus184151Pattern6().Match(text),
                "EW_CENSUS_1841_51_PATTERN6A" => RegexPatterns.EwCensus184151Pattern6A().Match(text),
                "EW_CENSUS_1841_51_PATTERN7" => RegexPatterns.EwCensus184151Pattern7().Match(text),
                "EW_CENSUS_1841_51_PATTERN8" => RegexPatterns.EwCensus184151Pattern8().Match(text),
                "EW_CENSUS_1841_51_PATTERN_SN" => RegexPatterns.EwCensus184151PatternSn().Match(text),
                "EW_CENSUS_1841_51_PATTERN_FH" => RegexPatterns.EwCensus184151PatternFh().Match(text),
                "EW_CENSUS_1841_51_PATTERN_FH2" => RegexPatterns.EwCensus184151PatternFh2().Match(text),
                "EW_CENSUS_1841_51_PATTERN_FH3" => RegexPatterns.EwCensus184151PatternFh3().Match(text),
                "EW_CENSUS_1841_51_PATTERN_FH4" => RegexPatterns.EwCensus184151PatternFh4().Match(text),
                "EW_CENSUS_1911_1921_PATTERN" => RegexPatterns.EwCensus19111921Pattern().Match(text),
                "EW_CENSUS_1911_1921_PATTERN2" => RegexPatterns.EwCensus19111921Pattern2().Match(text),
                "EW_CENSUS_1911_1921_PATTERN3" => RegexPatterns.EwCensus19111921Pattern3().Match(text),
                "EW_CENSUS_1911_1921_PATTERN4" => RegexPatterns.EwCensus19111921Pattern4().Match(text),
                "EW_CENSUS_1911_1921_PATTERN5" => RegexPatterns.EwCensus19111921Pattern5().Match(text),
                "EW_CENSUS_1911_1921_PATTERN6" => RegexPatterns.EwCensus19111921Pattern6().Match(text),
                "EW_CENSUS_1911_PATTERN1" => RegexPatterns.EwCensus1911Pattern1().Match(text),
                "EW_CENSUS_1911_PATTERN2" => RegexPatterns.EwCensus1911Pattern2().Match(text),
                "EW_CENSUS_1921_PATTERN1" => RegexPatterns.EwCensus1921Pattern1().Match(text),
                "EW_CENSUS_1921_PATTERN2" => RegexPatterns.EwCensus1921Pattern2().Match(text),
                "EW_CENSUS_1921_PATTERN3" => RegexPatterns.EwCensus1921Pattern3().Match(text),
                "EW_CENSUS_1921_PATTERN4" => RegexPatterns.EwCensus1921Pattern4().Match(text),
                "EW_CENSUS_PATTERN3" => RegexPatterns.EwCensusPattern3().Match(text),
                "EW_CENSUS_PATTERN4" => RegexPatterns.EwCensusPattern4().Match(text),
                "EW_CENSUS_PATTERN4_SN" => RegexPatterns.EwCensusPattern4Sn().Match(text),
                "EW_CENSUS_PATTERN5" => RegexPatterns.EwCensusPattern5().Match(text),
                "EW_CENSUS_PATTERN6" => RegexPatterns.EwCensusPattern6().Match(text),
                "EW_CENSUS_PATTERN7" => RegexPatterns.EwCensusPattern7().Match(text),
                "EW_CENSUS_PATTERN7_SN" => RegexPatterns.EwCensusPattern7Sn().Match(text),
                "EW_CENSUS_PATTERN8" => RegexPatterns.EwCensusPattern8().Match(text),
                "EW_CENSUS_PATTERN9" => RegexPatterns.EwCensusPattern9().Match(text),
                "EW_CENSUS_PATTERN10" => RegexPatterns.EwCensusPattern10().Match(text),
                "EW_CENSUS_PATTERN10_SN" => RegexPatterns.EwCensusPattern10Sn().Match(text),
                "EW_CENSUS_PATTERN11" => RegexPatterns.EwCensusPattern11().Match(text),
                "EW_CENSUS_PATTERN12" => RegexPatterns.EwCensusPattern12().Match(text),
                "EW_CENSUS_PATTERN13" => RegexPatterns.EwCensusPattern13().Match(text),
                "EW_CENSUS_PATTERN14" => RegexPatterns.EwCensusPattern14().Match(text),
                "EW_CENSUS_PATTERN15" => RegexPatterns.EwCensusPattern15().Match(text),
                "EW_CENSUS_PATTERN15_SN" => RegexPatterns.EwCensusPattern15Sn().Match(text),
                "EW_1939_REGISTER_PATTERN1" => RegexPatterns.Ew1939RegisterPattern1().Match(text),
                "EW_1939_REGISTER_PATTERN2" => RegexPatterns.Ew1939RegisterPattern2().Match(text),
                "EW_1939_REGISTER_PATTERN3" => RegexPatterns.Ew1939RegisterPattern3().Match(text),
                "SCOT_CENSUSYEAR_PATTERN" => RegexPatterns.ScotCensusyearPattern().Match(text),
                "SCOT_CENSUSYEAR_PATTERN2" => RegexPatterns.ScotCensusyearPattern2().Match(text),
                "SCOT_CENSUSYEAR_PATTERN3" => RegexPatterns.ScotCensusyearPattern3().Match(text),
                "SCOT_CENSUSYEAR_PATTERN4" => RegexPatterns.ScotCensusyearPattern4().Match(text),
                "SCOT_CENSUS_PATTERN" => RegexPatterns.ScotCensusPattern().Match(text),
                "SCOT_CENSUS_PATTERN2" => RegexPatterns.ScotCensusPattern2().Match(text),
                "SCOT_CENSUS_PATTERN3" => RegexPatterns.ScotCensusPattern3().Match(text),
                "SCOT_CENSUS_PATTERN4" => RegexPatterns.ScotCensusPattern4().Match(text),
                "SCOT_CENSUS_PATTERN5" => RegexPatterns.ScotCensusPattern5().Match(text),
                "US_CENSUS_PATTERN1A" => RegexPatterns.UsCensusPattern1A().Match(text),
                "US_CENSUS_PATTERN2" => RegexPatterns.UsCensusPattern2().Match(text),
                "US_CENSUS_PATTERN3" => RegexPatterns.UsCensusPattern3().Match(text),
                "US_CENSUS_PATTERN4" => RegexPatterns.UsCensusPattern4().Match(text),
                "US_CENSUS_PATTERN5" => RegexPatterns.UsCensusPattern5().Match(text),
                "US_CENSUS_PATTERN6" => RegexPatterns.UsCensusPattern6().Match(text),
                "US_CENSUS_PATTERN7" => RegexPatterns.UsCensusPattern7().Match(text),
                "US_CENSUS_PATTERN8" => RegexPatterns.UsCensusPattern8().Match(text),
                "US_CENSUS_PATTERN9" => RegexPatterns.UsCensusPattern9().Match(text),
                "US_CENSUS_1940_PATTERN" => RegexPatterns.UsCensus1940Pattern().Match(text),
                "US_CENSUS_1940_PATTERN2" => RegexPatterns.UsCensus1940Pattern2().Match(text),
                "US_CENSUS_1940_PATTERN3" => RegexPatterns.UsCensus1940Pattern3().Match(text),
                "US_CENSUS_1940_PATTERN4" => RegexPatterns.UsCensus1940Pattern4().Match(text),
                "US_CENSUS_T62X_PATTERN1" => RegexPatterns.UsCensusT62XPattern1().Match(text),
                "US_CENSUS_TX_PATTERN1" => RegexPatterns.UsCensusTxPattern1().Match(text),
                "US_CENSUS_MX_PATTERN1" => RegexPatterns.UsCensusMxPattern1().Match(text),
                "CANADA_CENSUS_PATTERN" => RegexPatterns.CanadaCensusPattern().Match(text),
                "CANADA_CENSUS_PATTERN2" => RegexPatterns.CanadaCensusPattern2().Match(text),
                "CANADA_CENSUS_PATTERN3" => RegexPatterns.CanadaCensusPattern3().Match(text),
                "CANADA_CENSUS_PATTERN4" => RegexPatterns.CanadaCensusPattern4().Match(text),
                "CANADA_CENSUS_PATTERN5" => RegexPatterns.CanadaCensusPattern5().Match(text),
                "CANADA_CENSUS_PATTERN6" => RegexPatterns.CanadaCensusPattern6().Match(text),
                "CANADA_CENSUS_PATTERN7" => RegexPatterns.CanadaCensusPattern7().Match(text),
                "LC_CENSUS_PATTERN_EW" => RegexPatterns.LcCensusPatternEw().Match(text),
                "LC_CENSUS_PATTERN_1911_EW" => RegexPatterns.LcCensusPattern1911Ew().Match(text),
                "LC_CENSUS_PATTERN_SCOT" => RegexPatterns.LcCensusPatternScot().Match(text),
                "LC_CENSUS_PATTERN_1940US" => RegexPatterns.LcCensusPattern1940Us().Match(text),
                "LC_CENSUS_PATTERN_1881CANADA" => RegexPatterns.LcCensusPattern1881Canada().Match(text),
                "EW_MISSINGCLASS_PATTERN" => RegexPatterns.EwMissingclassPattern().Match(text),
                "EW_MISSINGCLASS_PATTERN_SN" => RegexPatterns.EwMissingclassPatternSn().Match(text),
                "EW_MISSINGCLASS_PATTERN2" => RegexPatterns.EwMissingclassPattern2().Match(text),
                _ => Match.Empty
            };
            timer.Stop();
            string success = matcher.Success ? "***** MATCH *****" : "no match";
            output.Report($"Took {timer.Elapsed}s to process {patternName} resulting in {success}.\n");
        }

        static void CheckPatterns(string originalText, IProgress<string> output)
        {
            string text = ClearCommonPhrases(originalText);
            WriteTimer("EW_CENSUS_PATTERN", text, output);
            WriteTimer("EW_CENSUS_PATTERN1", text, output);
            WriteTimer("EW_CENSUS_PATTERN_SN", text, output);
            WriteTimer("EW_CENSUS_PATTERN2", text, output);
            WriteTimer("EW_CENSUS_PATTERN_FH", text, output);
            WriteTimer("EW_CENSUS_PATTERN_FH2", text, output);
            WriteTimer("EW_CENSUS_PATTERN_FH3", text, output);
            WriteTimer("EW_CENSUS_PATTERN_FS1", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN2", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN2A", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN3", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN4", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN5", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN6", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN6A", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN7", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN8", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN_SN", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN_FH", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN_FH2", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN_FH3", text, output);
            WriteTimer("EW_CENSUS_1841_51_PATTERN_FH4", text, output);
            WriteTimer("EW_CENSUS_1911_1921_PATTERN", text, output);
            WriteTimer("EW_CENSUS_1911_1921_PATTERN2", text, output);
            WriteTimer("EW_CENSUS_1911_1921_PATTERN3", text, output);
            WriteTimer("EW_CENSUS_1911_1921_PATTERN4", text, output);
            WriteTimer("EW_CENSUS_1911_1921_PATTERN5", text, output);
            WriteTimer("EW_CENSUS_1911_1921_PATTERN6", text, output);
            WriteTimer("EW_CENSUS_1911_1921_PATTERN7", text, output);
            WriteTimer("EW_CENSUS_1911_1921_PATTERN8", text, output);
            WriteTimer("EW_CENSUS_PATTERN3", text, output);
            WriteTimer("EW_CENSUS_PATTERN4", text, output);
            WriteTimer("EW_CENSUS_PATTERN4_SN", text, output);
            WriteTimer("EW_CENSUS_PATTERN5", text, output);
            WriteTimer("EW_CENSUS_PATTERN6", text, output);
            WriteTimer("EW_CENSUS_PATTERN7", text, output);
            WriteTimer("EW_CENSUS_PATTERN7_SN", text, output);
            WriteTimer("EW_CENSUS_PATTERN8", text, output);
            WriteTimer("EW_CENSUS_PATTERN9", text, output);
            WriteTimer("EW_CENSUS_PATTERN10", text, output);
            WriteTimer("EW_CENSUS_PATTERN10_SN", text, output);
            WriteTimer("EW_CENSUS_PATTERN11", text, output);
            WriteTimer("EW_CENSUS_PATTERN12", text, output);
            WriteTimer("EW_CENSUS_PATTERN13", text, output);
            WriteTimer("EW_CENSUS_PATTERN14", text, output);
            WriteTimer("EW_CENSUS_PATTERN15", text, output);
            WriteTimer("EW_CENSUS_PATTERN15_SN", text, output);
            WriteTimer("EW_1939_REGISTER_PATTERN1", text, output);
            WriteTimer("EW_1939_REGISTER_PATTERN2", text, output);
            WriteTimer("EW_1939_REGISTER_PATTERN3", text, output);
            WriteTimer("SCOT_CENSUSYEAR_PATTERN", text, output);
            WriteTimer("SCOT_CENSUSYEAR_PATTERN2", text, output);
            WriteTimer("SCOT_CENSUSYEAR_PATTERN3", text, output);
            WriteTimer("SCOT_CENSUSYEAR_PATTERN4", text, output);
            WriteTimer("SCOT_CENSUS_PATTERN", text, output);
            WriteTimer("SCOT_CENSUS_PATTERN2", text, output);
            WriteTimer("SCOT_CENSUS_PATTERN3", text, output);
            WriteTimer("SCOT_CENSUS_PATTERN4", text, output);
            WriteTimer("SCOT_CENSUS_PATTERN5", text, output);
            WriteTimer("US_CENSUS_PATTERN1A", text, output);
            WriteTimer("US_CENSUS_PATTERN2", text, output);
            WriteTimer("US_CENSUS_PATTERN3", text, output);
            WriteTimer("US_CENSUS_PATTERN4", text, output);
            WriteTimer("US_CENSUS_PATTERN5", text, output);
            WriteTimer("US_CENSUS_PATTERN6", text, output);
            WriteTimer("US_CENSUS_PATTERN7", text, output);
            WriteTimer("US_CENSUS_PATTERN8", text, output);
            WriteTimer("US_CENSUS_PATTERN9", text, output);
            WriteTimer("US_CENSUS_1940_PATTERN", text, output);
            WriteTimer("US_CENSUS_1940_PATTERN2", text, output);
            WriteTimer("US_CENSUS_1940_PATTERN3", text, output);
            WriteTimer("US_CENSUS_1940_PATTERN4", text, output);
            WriteTimer("US_CENSUS_T62X_PATTERN1", text, output);
            WriteTimer("US_CENSUS_TX_PATTERN1", text, output);
            WriteTimer("US_CENSUS_MX_PATTERN1", text, output);
            WriteTimer("CANADA_CENSUS_PATTERN", text, output);
            WriteTimer("CANADA_CENSUS_PATTERN2", text, output);
            WriteTimer("CANADA_CENSUS_PATTERN3", text, output);
            WriteTimer("CANADA_CENSUS_PATTERN4", text, output);
            WriteTimer("CANADA_CENSUS_PATTERN5", text, output);
            WriteTimer("CANADA_CENSUS_PATTERN6", text, output);
            WriteTimer("CANADA_CENSUS_PATTERN7", text, output);
            WriteTimer("LC_CENSUS_PATTERN_EW", text, output);
            WriteTimer("LC_CENSUS_PATTERN_1911_EW", text, output);
            WriteTimer("LC_CENSUS_PATTERN_SCOT", text, output);
            WriteTimer("LC_CENSUS_PATTERN_1940US", text, output);
            WriteTimer("LC_CENSUS_PATTERN_1881CANADA", text, output);
            WriteTimer("EW_MISSINGCLASS_PATTERN", text, output);
            WriteTimer("EW_MISSINGCLASS_PATTERN_SN", text, output);
            WriteTimer("EW_MISSINGCLASS_PATTERN2", text, output);
        }

        bool CheckPatterns(string originalText)
        {
            string text = ClearCommonPhrases(originalText);
            if (text.Length == 0)
                return false;
            Match matcher = RegexPatterns.HasNumbers().Match(text);
            if (!matcher.Success)
                return false; // skip checking if string has no digits
            matcher = RegexPatterns.Peoplefinders().Match(text);
            if (matcher.Success)
                return false; // skip checking if it's a peoplefinders.com result  
            matcher = RegexPatterns.EwCensusPattern().Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern1().Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPatternSn().Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Schedule = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern2().Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPatternFh().Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[4].ToString();
                Page = matcher.Groups[6].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPatternFh2().Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                Folio = matcher.Groups[5].ToString();
                Page = matcher.Groups[7].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPatternFh3().Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                Folio = matcher.Groups[5].ToString();
                Page = matcher.Groups[7].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPatternFs1().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Page = matcher.Groups[2].ToString();
                Piece = matcher.Groups[3].ToString();
                Folio = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern2().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Book = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern2A().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Book = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern3().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Book = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern4().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Book = matcher.Groups[2].ToString();
                Folio = MISSING;
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern5().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Book = MISSING;
                Folio = MISSING;
                Page = matcher.Groups[2].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern6().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Folio = matcher.Groups[1].ToString();
                Book = matcher.Groups[3].ToString();
                if (!string.IsNullOrEmpty(Book) && string.IsNullOrEmpty(matcher.Groups[2].ToString()))
                {
                    Book = matcher.Groups[1].ToString();
                    Folio = matcher.Groups[3].ToString();
                }
                Page = matcher.Groups[5].ToString();
                Piece = matcher.Groups[6].ToString();
                ED = matcher.Groups[7].ToString();
                if (Book.Length == 0 && ED.Length > 0)
                    Book = ED;
                ReferenceStatus status = string.IsNullOrEmpty(Book) && string.IsNullOrEmpty(ED) ? ReferenceStatus.INCOMPLETE : ReferenceStatus.GOOD;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), status, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern6A().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Book = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                Piece = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern7().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Book = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151Pattern8().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Book = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151PatternSn().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Book = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151PatternFh().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Book = matcher.Groups[2].ToString();
                Folio = matcher.Groups[4].ToString();
                Page = matcher.Groups[6].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151PatternFh2().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                ED = matcher.Groups[2].ToString();
                Folio = matcher.Groups[4].ToString();
                Page = matcher.Groups[6].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151PatternFh3().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[3].ToString();
                Book = matcher.Groups[4].ToString();
                Page = matcher.Groups[6].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus184151PatternFh4().Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus1921Pattern1().Match(text);
            if (matcher.Success)
            {
                Class = "RG15";
                Piece = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                Schedule = matcher.Groups[4].ToString();
                Book = matcher.Groups[5].ToString();
                RD = MISSING;
                SD = MISSING;
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus1921Pattern2().Match(text);
            if (matcher.Success)
            {
                Class = "RG15";
                Piece = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                Schedule = matcher.Groups[4].ToString();
                Book = MISSING;
                RD = MISSING;
                SD = MISSING;
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus1921Pattern3().Match(text);
            if (matcher.Success)
            {
                Class = "RG15";
                Piece = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                Page = MISSING;
                Schedule = MISSING;
                Book = MISSING;
                RD = MISSING;
                SD = MISSING;
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus1921Pattern4().Match(text);
            if (matcher.Success)
            {
                Class = "RG15";
                Piece = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                Schedule = MISSING;
                Book = MISSING;
                RD = MISSING;
                SD = MISSING;
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus19111921Pattern().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                Piece = matcher.Groups[3].ToString();
                Schedule = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus19111921Pattern2().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString() == "1911" ? "RG14" : "RG15";
                Piece = matcher.Groups[2].ToString();
                Schedule = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus19111921Pattern3().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString() == "1911" ? "RG14" : "RG15";
                Piece = matcher.Groups[2].ToString();
                Schedule = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus19111921Pattern4().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                Piece = matcher.Groups[2].ToString();
                Schedule = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus19111921Pattern5().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                Piece = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus19111921Pattern6().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                RD = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                Schedule = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus1911Pattern1().Match(text);
            if (matcher.Success)
            {
                Class = "RG78";
                Piece = matcher.Groups[1].ToString();
                Schedule = matcher.Groups[2].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensus1911Pattern2().Match(text);
            if (matcher.Success)
            {
                Class = "RG78";
                Piece = matcher.Groups[1].ToString();
                Schedule = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern3().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Book = matcher.Groups[3].ToString();
                Folio = matcher.Groups[4].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern4().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern4Sn().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Schedule = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern5().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern6().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Book = matcher.Groups[3].ToString();
                Folio = matcher.Groups[4].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern7().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern7Sn().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Schedule = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern8().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern9().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Book = matcher.Groups[3].ToString();
                Folio = matcher.Groups[4].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern10().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern10Sn().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Schedule = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern11().Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern12().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern13().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                if (Class.Right(2) != "31")
                {
                    Piece = matcher.Groups[2].ToString();
                    Folio = matcher.Groups[3].ToString();
                    Page = matcher.Groups[4].ToString();
                    SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                    return true;
                }
            }
            matcher = RegexPatterns.EwCensusPattern14().Match(text);
            if (matcher.Success)
            {
                Folio = matcher.Groups[1].ToString();
                Page = matcher.Groups[2].ToString();
                Class = matcher.Groups[3].ToString().Replace("RG ", "RG", StringComparison.Ordinal);
                Piece = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern15().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern15Sn().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Schedule = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwCensusPattern16().Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.Ew1939RegisterPattern1().Match(text);
            if (matcher.Success)
            {
                Class = "RG101";
                Piece = matcher.Groups[1].ToString();
                Page = matcher.Groups[2].ToString();
                Schedule = matcher.Groups[3].ToString();
                string letterCode = matcher.Groups[4].ToString();
                ED = CheckLetterCode(letterCode);
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.Ew1939RegisterPattern1A().Match(text);
            if (matcher.Success)
            {
                Class = "RG101";
                Piece = matcher.Groups[1].ToString();
                Page = matcher.Groups[2].ToString();
                Schedule = matcher.Groups[3].ToString();
                ED = MISSING;
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.Ew1939RegisterPattern2().Match(text);
            if (matcher.Success)
            {
                Class = "RG101";
                Piece = matcher.Groups[1].ToString();
                ED = matcher.Groups[2].ToString();
                Page = MISSING;
                Schedule = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.Ew1939RegisterPattern3().Match(text);
            if (matcher.Success)
            {
                Class = "RG101";
                Piece = matcher.Groups[1].ToString();
                ED = MISSING;
                Page = MISSING;
                Schedule = MISSING;
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.ScotCensusyearPattern().Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = CensusDate.GetUKCensusDateFromYear(matcher.Groups[1].ToString());
                if (CensusYear.BestYear == 1881)
                    CensusYear = CensusDate.SCOTCENSUS1881;
                Parish = matcher.Groups[3].ToString().TrimEnd('/');
                ED = matcher.Groups[4].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.ScotCensusyearPattern2().Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = CensusDate.GetUKCensusDateFromYear(matcher.Groups[1].ToString());
                if (CensusYear.BestYear == 1881)
                    CensusYear = CensusDate.SCOTCENSUS1881;
                Parish = matcher.Groups[3].ToString().Replace("/00", "", StringComparison.Ordinal).TrimEnd('/').Replace("/", "-", StringComparison.Ordinal);
                ED = matcher.Groups[4].ToString().Replace("/00", "", StringComparison.Ordinal).TrimStart('0');
                Page = matcher.Groups[5].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.ScotCensusyearPattern3().Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = CensusDate.GetUKCensusDateFromYear(matcher.Groups[1].ToString());
                if (CensusYear.BestYear == 1881)
                    CensusYear = CensusDate.SCOTCENSUS1881;
                Parish = matcher.Groups[3].ToString().TrimStart('0').TrimEnd('/');
                ED = matcher.Groups[4].ToString().Replace("/00", "", StringComparison.Ordinal).TrimStart('0');
                Page = matcher.Groups[5].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.ScotCensusyearPattern4().Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = CensusDate.GetUKCensusDateFromYear(matcher.Groups[1].ToString());
                if (CensusYear.BestYear == 1881)
                    CensusYear = CensusDate.SCOTCENSUS1881;
                Parish = matcher.Groups[2].ToString().TrimStart('0').TrimEnd('/');
                ED = matcher.Groups[4].ToString().Replace("/00", "", StringComparison.Ordinal).TrimStart('0');
                Page = matcher.Groups[6].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.ScotCensusPattern().Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = FactDate.UNKNOWN_DATE;
                Parish = matcher.Groups[1].ToString().Trim().TrimEnd('/');
                ED = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.ScotCensusPattern2().Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = FactDate.UNKNOWN_DATE;
                Parish = matcher.Groups[2].ToString().TrimEnd('/').Replace("/00", "", StringComparison.Ordinal).Replace("/", "-", StringComparison.Ordinal).Replace("-0", "-", StringComparison.Ordinal);
                ED = matcher.Groups[3].ToString().Replace("/00", "", StringComparison.Ordinal).TrimStart('0');
                Page = matcher.Groups[4].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.ScotCensusPattern3().Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = FactDate.UNKNOWN_DATE;
                Parish = matcher.Groups[2].ToString().TrimStart('0').TrimEnd('/');
                ED = matcher.Groups[3].ToString().Replace("/00", "", StringComparison.Ordinal).TrimStart('0');
                Page = matcher.Groups[4].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.ScotCensusPattern4().Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = new FactDate(matcher.Groups[1].ToString());
                if (CensusYear.BestYear == 1881)
                    CensusYear = CensusDate.SCOTCENSUS1881;
                Parish = matcher.Groups[3].ToString().Trim().TrimStart('0').TrimEnd('/');
                ED = matcher.Groups[4].ToString().Replace("/00", "", StringComparison.Ordinal).TrimStart('0');
                Page = matcher.Groups[5].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.ScotCensusPattern5().Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = new FactDate(matcher.Groups[1].ToString());
                if (CensusYear.BestYear == 1881)
                    CensusYear = CensusDate.SCOTCENSUS1881;
                Parish = matcher.Groups[3].ToString().Trim().TrimStart('0').TrimEnd('/');
                ED = matcher.Groups[4].ToString().Replace("/00", "", StringComparison.Ordinal).TrimStart('0');
                Page = matcher.Groups[5].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "ROLL");
                Roll = matcher.Groups[3].ToString();
                Page = matcher.Groups[6].ToString();
                ED = matcher.Groups[7].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern1A().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "ROLL");
                Roll = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                ED = matcher.Groups[6].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                if (Roll.StartsWith("T627", StringComparison.Ordinal)) Roll = Roll[5..];
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern2().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "ROLL");
                Roll = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                ED = matcher.Groups[6].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern3().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "WARD");
                Roll = matcher.Groups[3].ToString();
                Page = matcher.Groups[6].ToString();
                ED = matcher.Groups[4].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern3A().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "WARD");
                Roll = string.Empty;
                Page = matcher.Groups[5].ToString();
                ED = matcher.Groups[6].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern3B().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "WARD");
                Roll = string.Empty;
                Page = matcher.Groups[5].ToString();
                ED = matcher.Groups[6].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern3C().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "Page");
                Roll = string.Empty;
                Page = matcher.Groups[4].ToString();
                ED = matcher.Groups[5].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern4().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                ED = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                Roll = matcher.Groups[7].ToString().TrimStart('0');
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern5().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "ED");
                Page = matcher.Groups[5].ToString();
                ED = matcher.Groups[3].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern6().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[3].ToString(), originalText, "ED");
                Page = matcher.Groups[6].ToString();
                ED = matcher.Groups[4].ToString();
                Roll = matcher.Groups[8].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern7().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[3].ToString(), originalText, "ED");
                Page = matcher.Groups[6].ToString();
                ED = matcher.Groups[4].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern8().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[3].ToString(), originalText, "ED");
                Page = matcher.Groups[5].ToString();
                Roll = matcher.Groups[7].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusPattern9().Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                ED = matcher.Groups[3].ToString();
                Roll = matcher.Groups[4].ToString();
                Page = matcher.Groups[6].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensus1940Pattern().Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Roll = matcher.Groups[4].ToString();
                ED = matcher.Groups[1].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensus1940Pattern2().Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Roll = matcher.Groups[4].ToString();
                ED = matcher.Groups[1].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensus1940Pattern3().Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Place = GetOriginalPlace(matcher.Groups[1].ToString(), originalText, "T627");
                Roll = matcher.Groups[3].ToString();
                ED = matcher.Groups[6].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensus1940Pattern4().Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Roll = matcher.Groups[2].ToString();
                ED = matcher.Groups[5].ToString();
                Page = matcher.Groups[7].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusT62XPattern1().Match(text);
            if (matcher.Success)
            {
                var tCode = matcher.Groups[2].ToString();
                Class = tCode switch
                {
                    "623" => "US1900",
                    "624" => "US1910",
                    "625" => "US1920",
                    "626" => "US1930",
                    "627" => "US1940",
                    "628" => "US1950",
                    _ => string.Empty,
                };
                if (!string.IsNullOrEmpty(Class))
                {
                    Roll = matcher.Groups[3].ToString();
                    ED = matcher.Groups[6].ToString();
                    Page = matcher.Groups[8].ToString();
                    SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                    return true;
                }
            }
            matcher = RegexPatterns.UsCensusTxPattern1().Match(text);
            if (matcher.Success)
            {
                Class = $"US1880";
                Roll = matcher.Groups[2].ToString();
                ED = matcher.Groups[5].ToString();
                Page = matcher.Groups[7].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.UsCensusMxPattern1().Match(text);
            if (matcher.Success)
            {
                var tCode = matcher.Groups[1].ToString();
                Class = tCode switch
                {
                    "M407" => "US1890",
                    "M593" => "US1870",
                    "M653" => "US1860",
                    "M432" => "US1850",
                    "M704" => "US1840",
                    "M19" => "US1830",
                    "M33" => "US1820",
                    "M252" => "US1810",
                    "M32" => "US1800",
                    "M637" => "US1790",
                    _ => string.Empty,
                };
                if (!string.IsNullOrEmpty(Class))
                {
                    Roll = matcher.Groups[2].ToString();
                    ED = matcher.Groups[5].ToString();
                    Page = matcher.Groups[7].ToString();
                    SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                    return true;
                }
            }
            matcher = RegexPatterns.CanadaCensusPattern().Match(text);
            if (matcher.Success)
            {
                Class = $"CAN{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "ROLL");
                Roll = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                Family = matcher.Groups[6].ToString();
                SetFlagsandCountry(false, false, Countries.CANADA, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.CanadaCensusPattern1().Match(text);
            if (matcher.Success)
            {
                Class = $"CAN{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "Page");
                Roll = string.Empty;
                Page = matcher.Groups[4].ToString();
                Family = matcher.Groups[5].ToString();
                SetFlagsandCountry(false, false, Countries.CANADA, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.CanadaCensusPattern2().Match(text);
            if (matcher.Success)
            {
                Class = $"CAN{matcher.Groups[1]}";
                ED = matcher.Groups[2].ToString();
                SD = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                Family = matcher.Groups[6].ToString();
                SetFlagsandCountry(false, false, Countries.CANADA, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.CanadaCensusPattern3().Match(text);
            if (matcher.Success)
            {
                Class = $"CAN{matcher.Groups[1]}";
                RD = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                SD = matcher.Groups[4].ToString();
                Family = matcher.Groups[5].ToString();
                Page = matcher.Groups[7].ToString();
                SetFlagsandCountry(false, false, Countries.CANADA, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.CanadaCensusPattern4().Match(text);
            if (matcher.Success)
            {
                Class = $"CAN{matcher.Groups[1]}";
                string item = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                SD = matcher.Groups[4].ToString();
                Family = matcher.Groups[5].ToString();
                Page = matcher.Groups[6].ToString();
                SetFlagsandCountry(false, false, Countries.CANADA, ReferenceStatus.GOOD, matcher.Value);
                URL = $"https://www.bac-lac.gc.ca/eng/census/{matcher.Groups[1]}/Pages/item.aspx?itemid={item}";
                return true;
            }
            matcher = RegexPatterns.CanadaCensusPattern5().Match(text);
            if (matcher.Success)
            {
                Class = $"CAN{matcher.Groups[1]}";
                string item = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                SD = matcher.Groups[4].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(false, false, Countries.CANADA, ReferenceStatus.GOOD, matcher.Value);
                URL = $"https://www.bac-lac.gc.ca/eng/census/{matcher.Groups[1]}/Pages/item.aspx?itemid={item}";
                return true;
            }
            matcher = RegexPatterns.CanadaCensusPattern6().Match(text);
            if (matcher.Success)
            {
                Class = $"CAN{matcher.Groups[1]}";
                ED = matcher.Groups[2].ToString();
                SD = matcher.Groups[3].ToString();
                Family = matcher.Groups[4].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(false, false, Countries.CANADA, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.CanadaCensusPattern7().Match(text);
            if (matcher.Success)
            {
                Class = $"CAN{matcher.Groups[1]}";
                ED = matcher.Groups[2].ToString();
                SD = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(false, false, Countries.CANADA, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.LcCensusPatternEw().Match(text);
            if (matcher.Success)
            {
                if (matcher.Groups[4].ToString().Equals("1881", StringComparison.OrdinalIgnoreCase))
                    Class = "RG11";
                else
                    Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, true, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.LcCensusPattern1911Ew().Match(text);
            if (matcher.Success)
            {
                Class = "RG14";
                Piece = matcher.Groups[1].ToString();
                Schedule = matcher.Groups[2].ToString();
                SetFlagsandCountry(true, true, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.LcCensusPatternScot().Match(text);
            if (matcher.Success)
            {
                Class = "RG11";
                Parish = matcher.Groups[1].ToString();
                ED = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, true, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.LcCensusPattern1940Us().Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Roll = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(false, true, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.LcCensusPattern1881Canada().Match(text);
            if (matcher.Success)
            {
                Class = "CAN1881";
                CensusYear = CensusDate.CANADACENSUS1881;
                ED = matcher.Groups[1].ToString();
                SD = matcher.Groups[2].ToString();
                if (matcher.Groups[5].Length > 0)
                {
                    Page = matcher.Groups[4].ToString();
                    Family = matcher.Groups[5].ToString();
                }
                else
                {
                    Page = matcher.Groups[3].ToString();
                    Family = matcher.Groups[4].ToString();
                }
                SetFlagsandCountry(false, true, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwMissingclassPattern().Match(text);
            if (matcher.Success)
            {
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwMissingclassPatternSn().Match(text);
            if (matcher.Success)
            {
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Schedule = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = RegexPatterns.EwMissingclassPattern2().Match(text);
            if (matcher.Success)
            {
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Page = MISSING;
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            return false;
        }

        static string CheckLetterCode(string letterCode)
        {
            if (letterCode.Equals("CODE", StringComparison.OrdinalIgnoreCase))
                return "UNKNOWN";
            //TODO: Check that the code is one of the valid codes 
            return letterCode;
        }

        void SetFlagsandCountry(bool ukCensus, bool LCcensuFact, string country, ReferenceStatus status, string matchstring)
        {
            IsUKCensus = ukCensus;
            IsLCCensusFact = LCcensuFact;
            Country = country;
            Status = status;
            MatchString = matchstring;
            if (country == Countries.UNITED_STATES) FixUS1940Prefix();
        }

        static string GetOriginalPlace(string match, string originalText, string stopText)
        {
            int spacePos = match.IndexOf(' ', StringComparison.Ordinal);
            if (spacePos == -1)
                return match.ClearWhiteSpace();
            string startPlace = match[..spacePos];
            int matchPos = originalText.ToUpper().IndexOf(startPlace.ToUpper(), StringComparison.Ordinal);
            int stopPos = originalText.ToUpper().IndexOf(stopText, StringComparison.Ordinal);
            if (matchPos > -1 && stopPos > -1 && stopPos - matchPos > 0)
                return originalText[matchPos..stopPos].ClearWhiteSpace();
            return match.ClearWhiteSpace();
        }

        static string GetUKCensusClass(string year)
        {
            if (year.Equals("1841", StringComparison.OrdinalIgnoreCase) || year.Equals("1851", StringComparison.OrdinalIgnoreCase))
                return "HO107";
            if (year.Equals("1861", StringComparison.OrdinalIgnoreCase))
                return "RG9";
            if (year.Equals("1871", StringComparison.OrdinalIgnoreCase))
                return "RG10";
            if (year.Equals("1881", StringComparison.OrdinalIgnoreCase))
                return "RG11";
            if (year.Equals("1891", StringComparison.OrdinalIgnoreCase))
                return "RG12";
            if (year.Equals("1901", StringComparison.OrdinalIgnoreCase))
                return "RG13";
            if (year.Equals("1911", StringComparison.OrdinalIgnoreCase))
                return "RG14";
            return string.Empty;
        }

        FactDate GetCensusYearFromReference()
        {
            if (Class.Equals("SCOT", StringComparison.OrdinalIgnoreCase))
                return FactDate.UNKNOWN_DATE;
            if (Class.Equals("HO107", StringComparison.OrdinalIgnoreCase))
            {
                bool success = int.TryParse(Piece, out int piecenumber);
                if (success && piecenumber > 1465) // piece numbers go 1-1465 for 1841 and 1466+ for 1851.
                    return CensusDate.UKCENSUS1851;
                return CensusDate.UKCENSUS1841;
            }
            if (Class.Equals("RG9", StringComparison.OrdinalIgnoreCase) || Class.Equals("RG09", StringComparison.OrdinalIgnoreCase))
                return CensusDate.UKCENSUS1861;
            if (Class.Equals("RG10", StringComparison.OrdinalIgnoreCase))
                return CensusDate.UKCENSUS1871;
            if (Class.Equals("RG11", StringComparison.OrdinalIgnoreCase))
                return CensusDate.UKCENSUS1881;
            if (Class.Equals("RG12", StringComparison.OrdinalIgnoreCase))
                return CensusDate.UKCENSUS1891;
            if (Class.Equals("RG13", StringComparison.OrdinalIgnoreCase))
                return CensusDate.UKCENSUS1901;
            if (Class.Equals("RG14", StringComparison.OrdinalIgnoreCase) || Class.Equals("RG78", StringComparison.OrdinalIgnoreCase))
                return CensusDate.UKCENSUS1911;
            if (Class.Equals("RG15", StringComparison.OrdinalIgnoreCase))
                return CensusDate.UKCENSUS1921;
            if (Class.Equals("RG101", StringComparison.OrdinalIgnoreCase))
                return CensusDate.UKCENSUS1939;
            if (Class.StartsWith("US", StringComparison.Ordinal))
                return CensusDate.GetUSCensusDateFromReference(Class);
            if (Class.StartsWith("CAN", StringComparison.Ordinal))
                return CensusDate.GetCanadianCensusDateFromReference(Class);
            return FactDate.UNKNOWN_DATE;
        }

        string GetCensusURLFromReference()
        {
            if (CensusDate.IsUKCensusYear(CensusYear, true))
            {
                string year = CensusYear.StartDate.Year.ToString();
                string defaultRegion = Settings.Default.defaultURLRegion;
                defaultRegion ??= ".co.uk";
                if (year.Equals("1911", StringComparison.OrdinalIgnoreCase) && Countries.IsEnglandWales(Country) && Piece.Length > 0 && Schedule.Length > 0)
                    return @"https://search.findmypast" + defaultRegion + "/search-world-records/1911-census-for-england-and-wales?pieceno=" + Piece + @"&schedule=" + Schedule;
                if (year.Equals("1939", StringComparison.OrdinalIgnoreCase) && Countries.IsEnglandWales(Country) && Piece.Length > 0 && !ED.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase))
                {
                    string dir = Piece.Length > 1 ? Piece[..^1] : Piece; //strip last letter from piece
                    return @"https://search.findmypast" + defaultRegion + "/record?id=tna%2fr39%2f" + dir + "%2f" + Piece.ToLower() + "%2f" + Page + "%2f" + Schedule;
                }
                if (Countries.IsUnitedKingdom(Country))
                {
                    string querystring = string.Empty;
                    if (!Country.Equals(Countries.SCOTLAND, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Piece.Length > 0 && !Piece.Equals(MISSING, StringComparison.OrdinalIgnoreCase))
                            querystring = @"pieceno=" + Piece;
                        if (Folio.Length > 0 && !Folio.Equals(MISSING, StringComparison.OrdinalIgnoreCase))
                        {
                            string lastChar = Folio[Folio.Length..].ToUpper();
                            if (!lastChar.Equals("F", StringComparison.OrdinalIgnoreCase) && !lastChar.Equals("R", StringComparison.OrdinalIgnoreCase) && !lastChar.Equals("O", StringComparison.OrdinalIgnoreCase))
                                querystring = querystring + @"&folio=" + Folio;
                        }
                        if (Page.Length > 0 && !Page.Equals(MISSING, StringComparison.OrdinalIgnoreCase))
                            querystring = querystring + @"&page=" + Page;
                    }
                    if (year.Equals("1841", StringComparison.OrdinalIgnoreCase) && Book.Length > 0 && !Book.Equals(MISSING, StringComparison.OrdinalIgnoreCase))
                        return @"https://search.findmypast" + defaultRegion + "/search-world-records/1841-england-wales-and-scotland-census?" + querystring + @"&book=" + Book;
                    if (querystring.Length > 0)
                    {

                        return year == "1911" ? @"https ://search.findmypast.co.uk/search-world-Records/1911-census-for-england-and-wales" + querystring :
                            @"https://search.findmypast" + defaultRegion + "/search-world-records/" + year + "-england-wales-and-scotland-census?" + querystring;
                    }
                }
            }
            return string.Empty;
        }

        static string GetCensusReferenceCountry(string censusClass, string censusPiece)
        {
            bool success = int.TryParse(censusPiece, out int piece);
            if (success && censusClass.Length > 0 && censusPiece.Length > 0 && piece > 0)
            {
                if (censusClass.Equals("HO107", StringComparison.OrdinalIgnoreCase)) //1841 & 1851
                {
                    if (piece <= 1357)
                        return Countries.ENGLAND;
                    if (piece <= 1459)
                        return Countries.WALES;
                    if (piece <= 1462)
                        return Countries.CHANNEL_ISLANDS;
                    if (piece <= 1465)
                        return Countries.ISLE_OF_MAN;
                    // 1466+ is 1851 census class was still HO107
                    if (piece <= 2442)
                        return Countries.ENGLAND;
                    if (piece <= 2522)
                        return Countries.WALES;
                    if (piece <= 2526)
                        return Countries.ISLE_OF_MAN;
                    if (piece <= 2531)
                        return Countries.CHANNEL_ISLANDS;
                }
                else if (censusClass.Equals("RG9", StringComparison.OrdinalIgnoreCase) || censusClass.Equals("RG09", StringComparison.OrdinalIgnoreCase)) //1861
                {
                    if (piece <= 3973)
                        return Countries.ENGLAND;
                    if (piece <= 4373)
                        return Countries.WALES;
                    if (piece <= 4408)
                        return Countries.CHANNEL_ISLANDS;
                    if (piece <= 4432)
                        return Countries.ISLE_OF_MAN;
                    if (piece <= 4540)
                        return Countries.OVERSEAS_UK;
                }
                else if (censusClass.Equals("RG10", StringComparison.OrdinalIgnoreCase)) //1871
                {
                    if (piece <= 5291)
                        return Countries.ENGLAND;
                    if (piece <= 5754)
                        return Countries.WALES;
                    if (piece <= 5770)
                        return Countries.CHANNEL_ISLANDS;
                    if (piece <= 5778)
                        return Countries.ISLE_OF_MAN;
                    if (piece <= 5785)
                        return Countries.OVERSEAS_UK;
                }
                else if (censusClass.Equals("RG11", StringComparison.OrdinalIgnoreCase)) //1881
                {
                    if (piece <= 5216)
                        return Countries.ENGLAND;
                    if (piece <= 5595)
                        return Countries.WALES;
                    if (piece <= 5609)
                        return Countries.ISLE_OF_MAN;
                    if (piece <= 5632)
                        return Countries.CHANNEL_ISLANDS;
                    if (piece <= 5643)
                        return Countries.OVERSEAS_UK;
                }
                else if (censusClass.Equals("RG12", StringComparison.OrdinalIgnoreCase)) // 1891
                {
                    if (piece <= 4334)
                        return Countries.ENGLAND;
                    if (piece <= 4681)
                        return Countries.WALES;
                    if (piece <= 4692)
                        return Countries.ISLE_OF_MAN;
                    if (piece <= 4707)
                        return Countries.CHANNEL_ISLANDS;
                    if (piece <= 4708)
                        return Countries.OVERSEAS_UK;
                }
                else if (censusClass.Equals("RG13", StringComparison.OrdinalIgnoreCase)) //1901
                {
                    if (piece <= 4914)
                        return Countries.ENGLAND;
                    if (piece <= 5299)
                        return Countries.WALES;
                    if (piece <= 5308)
                        return Countries.ISLE_OF_MAN;
                    if (piece <= 5324)
                        return Countries.CHANNEL_ISLANDS;
                    if (piece <= 5338)
                        return Countries.OVERSEAS_UK;
                }
                else if (censusClass.Equals("RG14", StringComparison.OrdinalIgnoreCase)) //1911
                {
                    if (piece <= 31678)
                        return Countries.ENGLAND;
                    if (piece <= 34628)
                        return Countries.WALES;
                    if (piece <= 34751)
                        return Countries.ISLE_OF_MAN;
                    if (piece <= 34969)
                        return Countries.CHANNEL_ISLANDS;
                    if (piece <= 34998)
                        return Countries.OVERSEAS_UK;
                }
            }
            return Countries.ENG_WALES;
        }

        public bool IsKnownStatus => Status.Equals(ReferenceStatus.GOOD) || Status.Equals(ReferenceStatus.INCOMPLETE);
        public bool IsGoodStatus => Status.Equals(ReferenceStatus.GOOD);

        public string Reference => BuildReference(GeneralSettings.Default.UseCompactCensusRef);

        /// <summary>
        /// Reference in compact slash form regardless of the user's display preference. Lost Cousins'
        /// own website always shows references this way, so matching a scraped My Ancestors entry
        /// against a local individual (LostCousinsReconciliation) must compare like for like rather
        /// than depending on GeneralSettings.Default.UseCompactCensusRef.
        /// </summary>
        public string CompactReference => BuildReference(true);

        string BuildReference(bool compact)
        {
            if (Family.Length > 0)
            {
                if (Roll.Length > 0)
                {
                    return compact ? $"{Roll}/{Page}/{Family}" : $"Roll: {Roll}, Page: {Page}, Family: {Family}";
                }
                return compact
                    ? $"{ED}/{SD}/{Page}/{Family}"
                    : $"District: {ED}, Sub-District: {SD}, Page: {Page}, Family: {Family}";
            }
            if (Roll.Length > 0)
            {
                return compact
                    ? $"{Roll}{(ED.Length > 0 ? $"/{ED}" : "")}/{Page}"
                    : $"Roll: {Roll}{(ED.Length > 0 ? $", ED: {ED}" : "")}, Page: {Page}";
            }
            if (Piece.Length > 0 && Fact is not null)
            {
                if (Countries.IsEnglandWales(Fact.Location.Country) || Fact.IsOverseasUKCensus(Fact.Location.Country))
                {
                    if (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1851) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1861) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1871) ||
                        Fact.FactDate.Overlaps(CensusDate.UKCENSUS1881) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1891) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1901))
                    {
                        if (Page.Length > 0)
                            return compact
                                ? $"{Piece}/{Folio}/{Page}"
                                : $"Piece: {Piece}, Folio: {Folio}, Page: {Page}";
                        if (Schedule.Length > 0 && Schedule != MISSING)
                            return compact
                                ? $"{Piece}/{Folio}/SN{Schedule}"
                                : $"Piece: {Piece}, Folio: {Folio}, Schedule: {Schedule}";
                        return compact
                            ? $"{Piece}/{Folio}"
                            : $"Piece: {Piece}, Folio: {Folio}";
                    }
                    if (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1841))
                    {
                        if (Book.Length > 0)
                            return compact
                                ? $"{Piece}/{Book}/{Folio}/{Page}"
                                : $"Piece: {Piece}, Book: {Book}, Folio: {Folio}, Page: {Page}";
                        if (compact)
                            return $"{Piece}/see image/{Folio}/{Page}";
                        return $"Piece: {Piece}, Book: see census image (stamped on the census page after the piece number), Folio: {Folio}, Page: {Page}";
                    }
                    if (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1911))
                    {
                        if (Schedule.Length > 0 && Schedule != MISSING)
                            return compact ? $"{Piece}/{Schedule}" : $"Piece: {Piece}, Schedule: {Schedule}";
                        return compact ? $"{Piece}/{Page}" : $"Piece: {Piece}, Page: {Page}";
                    }
                    if (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1921))
                    {
                        if (Schedule.Length > 0 && Schedule != MISSING)
                            return compact ? $"{Piece}/{ED}/{Schedule}" : $"Piece: {Piece}, ED: {ED}, Schedule: {Schedule}";
                        return compact ? $"{Piece}/{ED}/{Page}" : $"Piece: {Piece}, ED {ED}, Page: {Page}";
                    }
                    if (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1939))
                    {
                        return compact
                            ? $"RG101/{Piece}/{Page}/{Schedule} ({ED})"
                            : $"Piece: {Piece}, Page: {Page}, Schedule {Schedule}, ED: {ED}";
                    }
                }
            }
            else if (Parish.Length > 0 && Fact is not null)
            {
                if (Fact.Location.Country.Equals(Countries.SCOTLAND, StringComparison.OrdinalIgnoreCase) && (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1841) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1851) ||
                    Fact.FactDate.Overlaps(CensusDate.UKCENSUS1861) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1871) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1881) ||
                    Fact.FactDate.Overlaps(CensusDate.UKCENSUS1891) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1901) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1911)))
                {
                    ScottishParish sp = ScottishParish.FindParishFromID(Parish);
                    if (compact)
                        return sp == ScottishParish.UNKNOWN_PARISH ? $"{Parish}/{ED}/{Page}" : $"{sp.GetReference(true)}/{ED}/{Page}";
                    return sp == ScottishParish.UNKNOWN_PARISH
                        ? $"Parish: {Parish}, ED: {ED}, Page: {Page}"
                        : $"Parish: {sp.GetReference(false)}, ED: {ED}, Page: {Page}";
                }
            }
            else if (RD.Length > 0 && Fact is not null)
            {
                if (Fact.Location.IsEnglandWales && Fact.FactDate.Overlaps(CensusDate.UKCENSUS1911))
                    return compact
                        ? $"{RD}/{ED}/{Schedule}"
                        : $"RD: {RD}, ED: {ED}, Schedule: {Schedule}";
            }
            if (unknownCensusRef.Length > 0)
                return unknownCensusRef;
            //if (ReferenceText.Length > 0)
            //  log.Warn("Census reference text not generated for :" + ReferenceText);
            return string.Empty;
        }

        void FixUS1940Prefix()
        {
            Roll = Roll.ToUpper().Replace('-', '_');
            if (Roll.StartsWith("T627_", StringComparison.Ordinal)) Roll = Roll[5..];
            else if (Roll.StartsWith("T0627_", StringComparison.Ordinal)) Roll = Roll[6..];
            else if (Roll.StartsWith("M_T627_", StringComparison.Ordinal)) Roll = Roll[7..];
            else if (Roll.StartsWith("M_T0627_", StringComparison.Ordinal)) Roll = Roll[8..];
        }

        public bool IsValidLostCousinsReference()
        {
            if (Status != ReferenceStatus.GOOD)
                return false;
            //use Peter's code to check all the entries are valid
            if (CensusYear.Overlaps(CensusDate.EWCENSUS1841) && Countries.IsEnglandWales(Country))
            {
                if (Piece.StartsWith("HO107", StringComparison.Ordinal))
                    Piece = Piece[5..];
                Piece = Piece.TrimStart('0');
                Book = Book.TrimStart('0');
                Folio = Folio.TrimStart('0').ToUpper().TrimEnd('A');
                if (!Piece.IsNumeric() || !Folio.IsNumeric() || !Page.IsNumeric() || Book.Length == 0) return false;
            }
            else if (CensusYear.Overlaps(CensusDate.EWCENSUS1881) && Countries.IsEnglandWales(Country))
            {
                if (Piece.StartsWith("RG78", StringComparison.Ordinal))
                    return false;
                if (Piece.StartsWith("RG11", StringComparison.Ordinal))
                    Piece = Piece[4..];
                Piece = Piece.TrimStart('0');
                Folio = Folio.TrimStart('0').ToUpper().TrimEnd('A');
                if (!Piece.IsNumeric() || !Folio.IsNumeric() || !Page.IsNumeric()) return false;
            }
            else if (CensusYear.Overlaps(CensusDate.SCOTCENSUS1881) && Country == Countries.SCOTLAND)
            {
                Parish = Parish.Replace('/', '-').TrimStart('0').TrimEnd('-');
                if (Parish.Length > 0)
                {
                    if (ScottishParish.IsParishID(Parish))
                        RD = Parish;
                    else
                    {
                        ScottishParish sp = ScottishParish.FindParishFromID(Parish);
                        if (sp.RegistrationDistrict != "UNK")
                            RD = sp.RegistrationDistrict;
                        else
                        {
                            RD = ScottishParish.FindParishFromName(Parish);
                            if (RD == "Unknown")
                            {
                                Status = ReferenceStatus.INCOMPLETE;
                                return false;
                            }
                        }
                    }
                }
                else
                    return false;
                ED = ED.TrimStart('0');
                Page = Page.TrimStart('0');
                if (!Page.IsNumeric()) return false;
                Match match = RegexPatterns.LcEdRegex().Match(ED); //also check d{1,3}[A-Z]? format
                if (!match.Success) return false; // check last to only do regex calc if everything else is ok
            }
            else if (CensusYear.Overlaps(CensusDate.CANADACENSUS1881) && Country == Countries.CANADA)
            {
                if (Roll.ToUpper().StartsWith("C_", StringComparison.Ordinal))
                    Roll = Roll[2..];


            }
            else if (CensusYear.Overlaps(CensusDate.IRELANDCENSUS1911) && Country == Countries.IRELAND)
            {
            }
            else if (CensusYear.Overlaps(CensusDate.EWCENSUS1911) && Countries.IsEnglandWales(Country))
            {
                if (Piece.StartsWith("RG14", StringComparison.Ordinal))
                    Piece = Piece[4..];
                if (Piece.StartsWith("PN", StringComparison.Ordinal))
                    Piece = Piece[2..];
                Piece = Piece.TrimStart('0');
                Schedule = Schedule.TrimStart('0');
                if (Schedule.Length == 0 && Page.Length > 0)
                    Schedule = "9999";
                if (!Piece.IsNumeric()) return false;
            }
            else if (CensusYear.Overlaps(CensusDate.USCENSUS1880) && Country == Countries.UNITED_STATES)
            {
                if (Roll.ToUpper().StartsWith("T9", StringComparison.Ordinal))
                    Roll = Roll[2..];
                Roll = Roll.TrimStart('-').TrimStart('_').TrimStart('0');
                Page = NumericToAlpha(Page.TrimStart('0'));
                if (!Roll.IsNumeric()) return false;
            }
            else if (CensusYear.Overlaps(CensusDate.USCENSUS1940) && Country == Countries.UNITED_STATES)
            {
                Roll = Roll.TrimStart('0');
                Page = NumericToAlpha(Page.TrimStart('0'));
                if (!Roll.IsNumeric()) return false;
            }
            return true;
        }

        string NumericToAlpha(string page)
        {
            if (page.Length > 3)
            {
                string prefix = page[..^2];
                if (Page.EndsWith(".1", StringComparison.Ordinal)) return prefix + "A";
                if (Page.EndsWith(".2", StringComparison.Ordinal)) return prefix + "B";
                if (Page.EndsWith(".3", StringComparison.Ordinal)) return prefix + "C";
                if (Page.EndsWith(".4", StringComparison.Ordinal)) return prefix + "D";
                if (Page.EndsWith(".5", StringComparison.Ordinal)) return prefix + "E";
                if (Page.EndsWith(".6", StringComparison.Ordinal)) return prefix + "F";
                if (Page.EndsWith(".7", StringComparison.Ordinal)) return prefix + "G";
                if (Page.EndsWith(".8", StringComparison.Ordinal)) return prefix + "H";
                if (Page.EndsWith(".9", StringComparison.Ordinal)) return prefix + "I";
            }
            return page;
        }

        public override string ToString() => Reference.Trim();

        public int CompareTo(CensusReference? that)
        {
            return (that is null) ? 0 : string.Compare(Reference, that.Reference, StringComparison.Ordinal);
        }
    }
}
