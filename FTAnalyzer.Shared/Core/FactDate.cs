using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static FTAnalyzer.ColourValues;

namespace FTAnalyzer
{
    public class FactDate : IComparable<FactDate>
    {
        public static DateTime MINDATE = new DateTime(1, 1, 1);
        public static DateTime MAXDATE = new DateTime(9999, 12, 31);
        public static int MAXYEARS = 110;
        public static int MINYEARS = 0;
        private static readonly int LOW = 0;
        private static readonly int HIGH = 1;
        private static readonly IFormatProvider CULTURE = new CultureInfo("en-GB", true);

        private static readonly string YEAR = "yyyy";
        private static readonly string EARLYYEAR = "yyy";
        private static readonly string MONTHYEAR = "MMM yyyy";
        private static readonly string MONTHYEAREARLY = "MMM yyy";
        private static readonly string DAYMONTH = "d MMM";
        private static readonly string MONTH = "MMM";
        public static string FULL = "d MMM yyyy";
        private static readonly string FULLEARLY = "d MMM yyy";
        private static readonly string DISPLAY = "d MMM yyyy";
        private static readonly string CHECKING = "d MMM";
        private static readonly string DATE_PATTERN = "^(\\d{0,2} )?([A-Za-z]{0,3}) *(\\d{0,4})$";
        private static readonly string INTERPRETED_DATE_PATTERN = "^INT (\\d{0,2} )?([A-Za-z]{0,3}) *(\\d{0,4}) .*$";
        private static readonly string EARLY_DATE_PATTERN = "^(\\d{3})$";
        private static readonly string DOUBLE_DATE_PATTERN = "^(\\d{0,2} )?([A-Za-z]{0,3}) *(\\d{0,4})/(\\d{0,2})$";
        private static readonly string DOUBLE_DATE_PATTERN2 = "^(\\d{0,2} )?([A-Za-z]{0,3}) *(\\d{4})/(\\d{4})$";
        private static readonly string POSTFIX = "(\\d{1,2})(?:ST|ND|RD|TH)(.*)";
        private static readonly string BETWEENFIX = "(\\d{4}) *- *(\\d{4})";
        private static readonly string BETWEENFIX2 = "([A-Za-z]{0,3}) *(\\d{4}) *- *([A-Za-z]{0,3}) *(\\d{4})";
        private static readonly string BETWEENFIX3 = "(\\d{0,2} )?([A-Za-z]{0,3}) *(\\d{4}) *- *(\\d{0,2} )?([A-Za-z]{0,3}) *(\\d{4})";
        private static readonly string BETWEENFIX4 = "(\\d{1,2}) *- *(\\d{1,2} )?([A-Za-z]{0,3}) *(\\d{4})";
        private static readonly string BETWEENFIX5 = "(\\d{1,2} )?([A-Za-z]{0,3}) *- *(\\d{1,2} )?([A-Za-z]{0,3}) *(\\d{4})";
        private static readonly string USDATEFIX = "^([A-Za-z]{3}) *(\\d{1,2} )(\\d{4})$";
        private static readonly string SPACEFIX = "^(\\d{1,2}) *([A-Za-z]{3}) *(\\d{0,4})$";

        public static FactDate UNKNOWN_DATE;
        public static FactDate MARRIAGE_LESS_THAN_13;

        private static readonly Dictionary<string, Regex> _datePatterns;
        private static Regex _regex;

        static FactDate()
        {
            _datePatterns = new Dictionary<string, Regex>
            {
                ["DATE_PATTERN"] = new Regex(DATE_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["INTERPRETED_DATE_PATTERN"] = new Regex(INTERPRETED_DATE_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EARLY_DATE_PATTERN"] = new Regex(EARLY_DATE_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["DOUBLE_DATE_PATTERN"] = new Regex(DOUBLE_DATE_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["DOUBLE_DATE_PATTERN2"] = new Regex(DOUBLE_DATE_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["POSTFIX"] = new Regex(POSTFIX, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX"] = new Regex(BETWEENFIX, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX2"] = new Regex(BETWEENFIX2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX3"] = new Regex(BETWEENFIX3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX4"] = new Regex(BETWEENFIX4, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX5"] = new Regex(BETWEENFIX5, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["USDATEFIX"] = new Regex(USDATEFIX, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["SPACEFIX"] = new Regex(SPACEFIX, RegexOptions.Compiled | RegexOptions.IgnoreCase),
            };
            UNKNOWN_DATE = new FactDate("UNKNOWN");
            MARRIAGE_LESS_THAN_13 = new FactDate("1600");
        }

        public enum FactDateType { BEF, AFT, BET, ABT, UNK, EXT }
        public enum NonGEDCOMFormatSelected { NONE = 0, DD_MM_YYYY = 1, MM_DD_YYYY = 2, YYYY_MM_DD = 3, YYYY_DD_MM = 4 }

        public string DateString { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public FactDateType DateType { get; private set; }
        private readonly string OriginalString;
        private string DoubleDateError;

        public bool DoubleDate { get; private set; } // Is a pre 1752 date bet 1 Jan and 25 Mar eg: 1735/36.
        private int yearfix;

        public FactDate(string str, string factRef = "")
        {
            DoubleDate = false;
            if (str == null)
                str = string.Empty;
            OriginalString = str;
            // remove any commas in date string
            yearfix = 0;
            str = FixTextDateFormats(str.ToUpper());
            str = FixCommonDateFormats(str);
            DateType = FactDateType.UNK;
            if (str == null || str.Length == 0)
                DateString = "UNKNOWN";
            else
                DateString = str.ToUpper();
            StartDate = MINDATE;
            EndDate = MAXDATE;
            if (!DateString.Equals("UNKNOWN"))
                ProcessDate(DateString, factRef);
        }

        public FactDate(DateTime startdate, DateTime enddate)
        {
            DateType = FactDateType.UNK;
            StartDate = startdate;
            EndDate = enddate;
            DateString = CalculateDateString();
            OriginalString = string.Empty;
        }

        public static string Format(string format, DateTime date) => string.Format("{0:" + format + "}", date).ToUpper();

        private string FixTextDateFormats(string str)
        {
            switch(str)
            {
                case "DECEASED":
                case "DEAD":
                    string today = DateTime.Now.ToString("dd MMM yyyy").ToUpper();
                    str = $"BEF {today}";
                    break;
                case "STILLBORN":
                case "INFANT":
                case "CHILD":
                case "YOUNG":
                case "UNMARRIED":
                case "NEVER MARRIED":
                case "NOT MARRIED":
                    throw new TextFactDateException(str);
            }
            return str;
        }

        private string FixCommonDateFormats(string str)
        {
            str = EnhancedTextInfo.RemoveDiacritics(str.Trim().ToUpper());
            str = str.Replace(",", string.Empty);
            str = str.Replace("(", string.Empty);
            str = str.Replace(")", string.Empty);
            str = str.Replace("?", string.Empty);
            str = str.Replace(".", " ");
            str = str.Replace("&", " AND ");
            str = str.Replace(" / ", "/");
            str = str.Replace("   ", " ");
            str = str.Replace("  ", " ");
            str = str.Replace("  ", " ");

            str = str.Replace("JANUARY", "JAN");
            str = str.Replace("FEBRUARY", "FEB");
            str = str.Replace("MARCH", "MAR");
            str = str.Replace("APRIL", "APR");
            str = str.Replace("APRL", "APR");
            str = str.Replace("JUNE", "JUN");
            str = str.Replace("JULY", "JUL");
            str = str.Replace("AUGUST", "AUG");
            str = str.Replace("AUGT", "AUG");
            str = str.Replace("SEPTEMBER", "SEP");
            str = str.Replace("OCTOBER", "OCT");
            str = str.Replace("NOVEMBER", "NOV");
            str = str.Replace("DECEMBER", "DEC");

            // French
            str = str.Replace("JANVIER", "JAN");
            str = str.Replace("JANV", "JAN");
            str = str.Replace("FEVRIER", "FEB");
            str = str.Replace("FEVR", "FEB");
            str = str.Replace("MARS", "MAR");
            str = str.Replace("AVRIL", "APR");
            str = str.Replace("AVRL", "APR");
            str = str.Replace("MAI", "MAY");
            str = str.Replace("JUIN", "JUN");
            str = str.Replace("JUILLET", "JUL");
            str = str.Replace("JUIL", "JUL");
            str = str.Replace("AOUT", "AUG");
            str = str.Replace("SEPTEMBRE", "SEP");
            str = str.Replace("OCTOBRE", "OCT");
            str = str.Replace("NOVEMBRE", "NOV");
            str = str.Replace("DECEMBRE", "DEC");
            str = str.Replace(" ET ", " AND ");
            str = str.Replace("DATE INCONNUE", "UNKNOWN");

            str = str.Replace("SEPT", "SEP"); // avoids confusing french translation by removing T before checking for french
            str = str.Replace("M01", "JAN");
            str = str.Replace("M02", "FEB");
            str = str.Replace("M03", "MAR");
            str = str.Replace("M04", "APR");
            str = str.Replace("M05", "MAY");
            str = str.Replace("M06", "JUN");
            str = str.Replace("M07", "JUL");
            str = str.Replace("M08", "AUG");
            str = str.Replace("M09", "SEP");
            str = str.Replace("M10", "OCT");
            str = str.Replace("M11", "NOV");
            str = str.Replace("M12", "DEC");

            str = str.Replace("ABOUT", "ABT");
            str = str.Replace("AFTER", "AFT");
            str = str.Replace("BEFORE", "BEF");
            str = str.Replace("BETWEEN", "BET");
            str = str.Replace("BTW", "BET");
            str = str.Replace("UNTIL", "TO");
            str = str.Replace("CIRCA", "ABT");
            str = str.Replace("AROUND", "ABT");
            str = str.Replace("APPROX", "ABT");

            // French 
            str = str.Replace("AVANT", "BEF");
            str = str.Replace("ENTRE", "BET");
            str = str.Replace("ENVIRON", "ABT");
            str = str.Replace("VERS", "ABT");
            str = str.Replace("APRES", "AFT");

            str = str.Replace("CAL", "ABT");
            str = str.Replace("EST", "ABT");
            str = str.Replace("CIR", "ABT");
            str = str.Replace("PRE", "BEF");
            str = str.Replace("POST", "AFT");

            str = str.Replace("QUARTER", "QTR");
            str = str.Replace("MAR QTR", "ABT MAR");
            str = str.Replace("MAR Q ", "ABT MAR ");
            str = str.Replace("JAN FEB MAR", "ABT MAR");
            str = str.Replace("JAN-MAR", "ABT MAR");
            str = str.Replace("JAN-FEB-MAR", "ABT MAR");
            str = str.Replace("JAN/FEB/MAR", "ABT MAR");
            str = str.Replace("JAN\\FEB\\MAR", "ABT MAR");
            str = str.Replace("Q1", "ABT MAR");
            str = str.Replace("1Q", "ABT MAR");
            str = str.Replace("QTR1", "ABT MAR");
            str = str.Replace("QTR 1 ", "ABT MAR ");
            str = str.Replace("JUN QTR", "ABT JUN");
            str = str.Replace("JUN Q ", "ABT JUN ");
            str = str.Replace("APR MAY JUN", "ABT JUN");
            str = str.Replace("APR-JUN", "ABT JUN");
            str = str.Replace("APR-MAY-JUN", "ABT JUN");
            str = str.Replace("APR/MAY/JUN", "ABT JUN");
            str = str.Replace("APR\\MAY\\JUN", "ABT JUN");
            str = str.Replace("Q2", "ABT JUN");
            str = str.Replace("2Q", "ABT JUN");
            str = str.Replace("QTR2", "ABT JUN");
            str = str.Replace("QTR 2 ", "ABT JUN ");
            str = str.Replace("SEP QTR", "ABT SEP");
            str = str.Replace("SEP Q ", "ABT SEP ");
            str = str.Replace("JUL AUG SEP", "ABT SEP");
            str = str.Replace("JUL-SEP", "ABT SEP");
            str = str.Replace("JUL-AUG-SEP", "ABT SEP");
            str = str.Replace("JUL/AUG/SEP", "ABT SEP");
            str = str.Replace("JUL\\AUG\\SEP", "ABT SEP");
            str = str.Replace("Q3", "ABT SEP");
            str = str.Replace("3Q", "ABT SEP");
            str = str.Replace("QTR3", "ABT SEP");
            str = str.Replace("QTR 3 ", "ABT SEP ");
            str = str.Replace("DEC QTR", "ABT DEC");
            str = str.Replace("DEC Q ", "ABT DEC ");
            str = str.Replace("OCT NOV DEC", "ABT DEC");
            str = str.Replace("OCT-DEC", "ABT DEC");
            str = str.Replace("OCT-NOV-DEC", "ABT DEC");
            str = str.Replace("OCT/NOV/DEC", "ABT DEC");
            str = str.Replace("OCT\\NOV\\DEC", "ABT DEC");
            str = str.Replace("Q4", "ABT DEC");
            str = str.Replace("4Q", "ABT DEC");
            str = str.Replace("QTR4", "ABT DEC");
            str = str.Replace("QTR 4 ", "ABT DEC ");

            str = str.Replace("ABT ABT", "ABT"); // fix any ABT X QTR's that will have been changed to ABT ABT
            str = str.Replace("BET ABT", "ABT"); // fix any BET XXX-XXX QTR's that will have been changed to BET ABT

            if (str.IndexOf("TO") > 1)
            {  // contains TO but doesn't start with TO
                if (!str.StartsWith("FROM"))
                    str = "FROM " + str;
            }
            if (str.StartsWith("FROM"))
            {
                if (str.IndexOf("TO") > 0)
                    str = str.Replace("FROM", "BET").Replace("TO", "AND");
                else
                {
                    str = str.Replace("FROM", "AFT"); // year will be one out
                    yearfix = -1;
                }
            }
            if (str.StartsWith("TO"))
            {
                str = str.Replace("TO", "BEF"); // year will be one out
                yearfix = +1;
            }
            if (str.StartsWith(">"))
                str = str.Replace(">", "AFT ");
            if (str.StartsWith("<"))
                str = str.Replace("<", "BEF ");
            if (str.StartsWith("~"))
                str = str.Replace("~", "ABT ");
            if (str.StartsWith("C1") || str.StartsWith("C2") || str.StartsWith("C 1") || str.StartsWith("C 2"))
                str = "ABT " + str.Substring(1);
            str = str.Replace("  ", " "); // fix issue if > or < or Cxxx has already got a space
            Match matcher;
            if (str.StartsWith("INT")) // Interpreted date but we can discard <<Date_Phrase>>
            {
                matcher = _datePatterns["INTERPRETED_DATE_PATTERN"].Match(str);
                if (matcher.Success)
                {
                    string result = matcher.Groups[1].ToString() + matcher.Groups[2].ToString() + " " + matcher.Groups[3].ToString();
                    return result.Trim();
                }
            }
            matcher = _datePatterns["POSTFIX"].Match(str);
            if (matcher.Success)
            {
                string result = matcher.Groups[1].ToString() + matcher.Groups[2].ToString();
                return result.Trim();
            }
            if (Properties.NonGedcomDate.Default.UseNonGedcomDates)
            {
                matcher = NonGEDCOMDateFormatRegex.Match(str);
                if (!matcher.Success) // match string is not a non gedcom format with dashes so proceed with between fixes
                {
                    Tuple<bool, string> result = BetweenFixes(str);
                    if (result.Item1)
                        return result.Item2.ToString().Trim();
                }
            }
            else
            {
                Tuple<bool, string> result = BetweenFixes(str);
                if (result.Item1)
                    return result.Item2.ToString().Trim();
            }
            matcher = _datePatterns["USDATEFIX"].Match(str);
            if (matcher.Success)
            {
                string result = matcher.Groups[2].ToString() + matcher.Groups[1].ToString() + " " + matcher.Groups[3].ToString();
                return result.Trim();
            }
            matcher = _datePatterns["SPACEFIX"].Match(str);
            if (matcher.Success)
            {
                string result = matcher.Groups[1].ToString() + " " + matcher.Groups[2].ToString() + " " + matcher.Groups[3].ToString();
                return result.Trim();
            }
            return str.Trim();
        }

        private Tuple<bool,string> BetweenFixes(string str)
        {
            Match matcher = _datePatterns["BETWEENFIX"].Match(str);
            if (matcher.Success)
                return new Tuple<bool, string>(true, "BET " + matcher.Groups[1].ToString() + " AND " + matcher.Groups[2].ToString());
            matcher = _datePatterns["BETWEENFIX2"].Match(str);
            if (matcher.Success)
                return new Tuple<bool, string>(true, "BET " + matcher.Groups[1].ToString() + " " + matcher.Groups[2].ToString() + " AND " + matcher.Groups[3].ToString() + " " + matcher.Groups[4].ToString());
            matcher = _datePatterns["BETWEENFIX3"].Match(str);
            if (matcher.Success)
                return new Tuple<bool, string>(true, "BET " + matcher.Groups[1].ToString() + matcher.Groups[2].ToString() + " " + matcher.Groups[3].ToString() + " AND " + matcher.Groups[4].ToString() + matcher.Groups[5].ToString() + " " + matcher.Groups[6].ToString());
            matcher = _datePatterns["BETWEENFIX4"].Match(str);
            if (matcher.Success)
                return  new Tuple<bool, string>(true, "BET " + matcher.Groups[1].ToString() + " " + matcher.Groups[3].ToString() + " " + matcher.Groups[4].ToString() + " AND " + matcher.Groups[2].ToString() + matcher.Groups[3].ToString() + " " + matcher.Groups[4].ToString());
            matcher = _datePatterns["BETWEENFIX5"].Match(str);
            if (matcher.Success)
                return new Tuple<bool, string>(true, "BET " + matcher.Groups[1].ToString() + matcher.Groups[2].ToString() + " " + matcher.Groups[5].ToString() + " AND " + matcher.Groups[3].ToString() + matcher.Groups[4].ToString() + " " + matcher.Groups[5].ToString());
            return new Tuple<bool, string>(false, string.Empty);
        }

        #region Process Dates

        public FactDate SubtractMonths(int months)
        {
            DateTime start = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day);
            DateTime end = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day);
            if (StartDate.Year != MINDATE.Year)
                start = start.AddMonths(-months);
            else
                start = MINDATE;
            end = end.AddMonths(-months);
            if (start < MINDATE)
                start = MINDATE;
            return new FactDate(start, end);
        }

        public FactDate AddEndDateYears(int years)
        {
            DateTime start = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day);
            DateTime end = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day);
            end = end.AddMonths(years*12);
            if (end > MAXDATE)
                end = MAXDATE;
            return new FactDate(start, end);
        }

        private string CalculateDateString()
        {
            string check;
            StringBuilder output = new StringBuilder();
            if (StartDate == MINDATE)
            {
                if (EndDate == MAXDATE)
                    return "UNKNOWN";
                else
                {
                    DateType = FactDateType.BEF;
                    output.Append("BEF ");
                }
            }
            else
            {
                check = Format(CHECKING, StartDate);
                if (EndDate == MAXDATE)
                {
                    DateType = FactDateType.AFT;
                    output.Append("AFT ");
                }
                else if (StartDate == EndDate)
                {
                    DateType = FactDateType.EXT;
                }
                else
                {
                    DateType = FactDateType.BET;
                    output.Append("BET ");
                }
                if (check.Equals("01 JAN"))
                    output.Append(Format(YEAR, StartDate));
                else
                    output.Append(Format(DISPLAY, StartDate));
                if (DateType == FactDateType.BET)
                    output.Append(" AND ");
            }
            if (EndDate != MAXDATE && EndDate != StartDate)
            {
                check = Format(CHECKING, EndDate);
                if (check.Equals("31 DEC"))
                {
                    // add 1 day to take it to 1st Jan following year
                    // this makes the range of "bef 1900" change to 
                    // "bet xxxx and 1900"
                    output.Append(Format(YEAR, EndDate));
                }
                else
                    output.Append(Format(DISPLAY, EndDate));
            }
            return output.ToString().ToUpper();
        }

        private void ProcessDate(string processDate, string factRef)
        {
            // takes datestring and works out start and end dates 
            // prefixes are BEF, AFT, BET and nothing
            // dates are "YYYY" or "MMM YYYY" or "DD MMM YYYY"
            try
            {
                string dateValue;
                if (processDate.Length >= 4)
                    dateValue = processDate.Substring(4);
                else
                    dateValue = processDate;
                if (processDate.StartsWith("BEF"))
                {
                    DateType = FactDateType.BEF;
                    EndDate = ParseDate(dateValue, HIGH, -1 + yearfix);
                }
                else if (processDate.StartsWith("AFT"))
                {
                    DateType = FactDateType.AFT;
                    StartDate = ParseDate(dateValue, LOW, +1 + yearfix);
                }
                else if (processDate.StartsWith("ABT"))
                {
                    DateType = FactDateType.ABT;
                    if (processDate.StartsWith("ABT MAR") || processDate.StartsWith("ABT JUN")
                         || processDate.StartsWith("ABT SEP") || processDate.StartsWith("ABT DEC"))
                    {
                        // quarter dates
                        StartDate = ParseDate(dateValue, LOW, -3);
                    }
                    else
                    {
                        StartDate = ParseDate(dateValue, LOW, -1);
                    }
                    EndDate = ParseDate(dateValue, HIGH, 0);
                }
                else if (processDate.StartsWith("BET"))
                {
                    string fromdate;
                    string todate;
                    DateType = FactDateType.BET;
                    int pos = processDate.IndexOf(" AND ");
                    if (pos == -1)
                    {
                        pos = processDate.IndexOf("-");
                        if (pos == -1)
                            throw new Exception("Invalid BETween date no AND found");
                        fromdate = processDate.Substring(4, pos - 4);
                        todate = processDate.Substring(pos + 1);
                        pos = pos - 4; // from to code in next block expects to jump 5 chars not 1.
                    }
                    else
                    {
                        fromdate = processDate.Substring(4, pos - 4);
                        todate = processDate.Substring(pos + 5);
                    }
                    if (fromdate.Length < 3)
                        fromdate = fromdate + processDate.Substring(pos + 7);
                    else if (fromdate.Length == 3)
                        fromdate = "01 " + fromdate + processDate.Substring(pos + 8);
                    else if (fromdate.Length == 4)
                        fromdate = "01 JAN " + fromdate;
                    else if (fromdate.Length < 7 && fromdate.IndexOf(" ") > 0)
                        fromdate = fromdate + processDate.Substring(pos + 11);
                    StartDate = ParseDate(fromdate, LOW, 0, EndDate.Year);
                    EndDate = ParseDate(todate, HIGH, 0);
                }
                else
                {
                    DateType = FactDateType.EXT;
                    dateValue = processDate;
                    StartDate = ParseDate(dateValue, LOW, 0, 1);
                    EndDate = ParseDate(dateValue, HIGH, 0, 9999); // have upper default year as 9999 if no year in date
                }
            }
            catch (Exception e)
            {
                throw new FactDateException($"Error parsing date '{OriginalString}' for {factRef}. Error message was : {e.Message}\n");
            }
        }

        private DateTime ParseDate(string dateValue, int highlow, int adjustment)
        {
            return ParseDate(dateValue, highlow, adjustment, 1);
        }

        private DateTime ParseDate(string dateValue, int highlow, int adjustment, int defaultYear)
        {
            DateTime date;
            Group gDay = null, gMonth = null, gYear = null, gDouble = null;
            DateTime dt = MINDATE;
            dateValue = dateValue.Trim();
            if (dateValue.Length == 0)
                return highlow == HIGH ? MAXDATE : MINDATE;
            try
            {
                // Match the regular expression pattern against a text string.
                Match matcher = _datePatterns["DATE_PATTERN"].Match(dateValue);
                Match matcher2 = _datePatterns["EARLY_DATE_PATTERN"].Match(dateValue);
                if (matcher2.Success)
                {  // first check match vs 
                    gDay = null;
                    gMonth = null;
                    gYear = matcher2.Groups[1];
                    gDouble = null;
                }
                else if (matcher.Success)
                {
                    gDay = matcher.Groups[1];
                    gMonth = matcher.Groups[2];
                    gYear = matcher.Groups[3];
                    gDouble = null;
                }
                else
                {   // Try matching double date pattern
                    matcher = _datePatterns["DOUBLE_DATE_PATTERN"].Match(dateValue);
                    matcher2 = _datePatterns["DOUBLE_DATE_PATTERN2"].Match(dateValue);
                    matcher = Regex.Match(dateValue, DOUBLE_DATE_PATTERN);
                    matcher2 = Regex.Match(dateValue, DOUBLE_DATE_PATTERN2);
                    if (matcher.Success)
                    {
                        gDay = matcher.Groups[1];
                        gMonth = matcher.Groups[2];
                        gYear = matcher.Groups[3];
                        gDouble = matcher.Groups[4];
                        if (dateValue.Length > 3)
                            dateValue = dateValue.Substring(0, dateValue.Length - gDouble.ToString().Length - 1); // remove the trailing / and 1 or 2 digits
                    }
                    else if (matcher2.Success)
                    {
                        gDay = matcher2.Groups[1];
                        gMonth = matcher2.Groups[2];
                        gYear = matcher2.Groups[3];
                        gDouble = matcher2.Groups[4];
                        if (dateValue.Length > 5)
                            dateValue = dateValue.Substring(0, dateValue.Length - 5); // remove the trailing / and 4 digits
                    }
                    else if (Properties.NonGedcomDate.Default.UseNonGedcomDates)
                    {
                        matcher2 = NonGEDCOMDateFormatRegex.Match(dateValue);
                        if (matcher2.Success)
                        {
                            switch ((NonGEDCOMFormatSelected)Properties.NonGedcomDate.Default.FormatSelected)
                            {
                                case NonGEDCOMFormatSelected.DD_MM_YYYY:
                                    gDay = matcher2.Groups[1];
                                    gMonth = matcher2.Groups[2];
                                    gYear = matcher2.Groups[3];
                                    gDouble = null;
                                    break;
                                case NonGEDCOMFormatSelected.MM_DD_YYYY:
                                    gDay = matcher2.Groups[2];
                                    gMonth = matcher2.Groups[1];
                                    gYear = matcher2.Groups[3];
                                    gDouble = null;
                                    break;
                                case NonGEDCOMFormatSelected.YYYY_DD_MM:
                                    gDay = matcher2.Groups[2];
                                    gMonth = matcher2.Groups[3];
                                    gYear = matcher2.Groups[1];
                                    gDouble = null;
                                    break;
                                case NonGEDCOMFormatSelected.YYYY_MM_DD:
                                    gDay = matcher2.Groups[3];
                                    gMonth = matcher2.Groups[2];
                                    gYear = matcher2.Groups[1];
                                    gDouble = null;
                                    break;
                            }
                            string standardFormat = new DateTime(int.Parse(gYear.Value), int.Parse(gMonth.Value), int.Parse(gDay.Value)).ToString("dd MMM yyyy").ToString().ToUpper();
                            if (dateValue.Length > matcher2.Length)
                                dateValue = dateValue.Substring(0, matcher2.Index) + standardFormat + dateValue.Substring(matcher2.Index + matcher2.Length, dateValue.Length);
                            else
                                dateValue = standardFormat;
                        }
                    }
                    else
                        throw new Exception($"Unrecognised date format for: {dateValue}");
                }
                // Now process matched string - if gDouble is not null we have a double date to check
                string day = gDay == null ? string.Empty : gDay.ToString().Trim();
                string month = gMonth == null ? string.Empty : gMonth.ToString().Trim();
                string year = gYear == null ? string.Empty : gYear.ToString().Trim();

                if (!IsValidDoubleDate(day, month, year, gDouble))
                    throw new InvalidDoubleDateException(DoubleDateError);
                if (day.Length == 0 && month.Length == 0)
                {
                    if (year.Length == 4)
                        date = DateTime.ParseExact(dateValue, YEAR, CULTURE);
                    else
                        date = DateTime.ParseExact(dateValue, EARLYYEAR, CULTURE);
                    if (highlow == HIGH)
                        dt = new DateTime(date.Year + adjustment, 12, 31);
                    else
                        dt = new DateTime(date.Year + adjustment, 1, 1);
                }
                else if (day.Length == 0 && year.Length > 0)
                {
                    if (!DateTime.TryParseExact(dateValue, MONTHYEAR, CULTURE, DateTimeStyles.NoCurrentDateDefault, out date))
                        DateTime.TryParseExact(dateValue, MONTHYEAREARLY, CULTURE, DateTimeStyles.NoCurrentDateDefault, out date);
                    dt = new DateTime(date.Year, date.Month, 1);
                    dt = dt.AddMonths(adjustment);
                    if (highlow == HIGH)
                    {
                        // at 1st of month so add 1 month to first of next month
                        dt = dt.AddMonths(1);
                        // then subtract 1 day to be last day of correct month.
                        dt = dt.AddDays(-1);
                    }
                }
                else if (day.Length == 0 && year.Length == 0)
                {
                    date = DateTime.ParseExact(dateValue, MONTH, CULTURE);
                    dt = new DateTime(defaultYear, date.Month, 1);
                    if (highlow == HIGH)
                    {
                        // at 1st of month so add 1 month to first of next month
                        dt = dt.AddMonths(1);
                        // then subtract 1 day to be last day of correct month.
                        dt = dt.AddDays(-1);
                    }
                }
                else if (year.Length == 0)
                {
                    date = DateTime.ParseExact(dateValue, DAYMONTH, CULTURE);
                    dt = new DateTime(defaultYear, date.Month, date.Day);
                }
                else if (day.Length > 0 && month.Length > 0 && year.Length > 0)
                {
                    if (!DateTime.TryParseExact(dateValue, FULL, CULTURE, DateTimeStyles.NoCurrentDateDefault, out date))
                        DateTime.TryParseExact(dateValue, FULLEARLY, CULTURE, DateTimeStyles.NoCurrentDateDefault, out date);
                    dt = new DateTime(date.Year, date.Month, date.Day);
                    dt = dt.AddDays(adjustment);
                }
                else
                {
                    dt = (highlow == HIGH) ? MAXDATE : MINDATE;
                }
                if (gDouble != null)
                    dt = dt.TryAddYears(1); // use upper year for double dates
            }
            catch (FormatException)
            {
                throw new Exception($"Unrecognised date format for: {dateValue}");
            }
            catch (Exception e)
            {
                dt = (highlow == HIGH) ? MAXDATE : MINDATE;
                throw new Exception($"Problem with date format for: {dateValue} system said: {e.Message}");
            }
            return dt;
        }

        private bool IsValidDoubleDate(string day, string month, string year, Group gDouble)
        {
            DoubleDate = false;   // set property
            DoubleDateError = string.Empty;
            if (gDouble == null)  // normal date so its valid double date
                return true;
            // check if valid double date if so set double date to true
            string doubleyear = gDouble.ToString().Trim();
            if (doubleyear.Length == 4)
                doubleyear = doubleyear.Substring(2);
            if (doubleyear.Length == 1 && year.Length >= 2)
                doubleyear = year.Substring(year.Length - 2, 1) + doubleyear;
            if (doubleyear == null || (doubleyear.Length != 2 && doubleyear.Length != 4) || year.Length < 3)
            {
                DoubleDateError = "Year part of double date is an invalid length.";
                return false;
            }
            int iYear = Convert.ToInt32(year);
            if (iYear >= 1752)
            {
                DoubleDateError = "Double dates are only valid prior to 1752 (for most GB countries and colonies).";
                return false; // double years are only for pre 1752
            }
            if (month.Length == 3 && month != "JAN" && month != "FEB" && month != "MAR")
            {
                DoubleDateError = "Double dates years are only valid for Jan-Mar of a year.";
                return false; // double years must be pre Mar 25th of year
            }
            if (doubleyear == "00" && year.Substring(2, 2) != "99")
            {
                DoubleDateError = "Double date year should be 99/00 if it crosses century.";
                return false; // check for change of century year
            }
            int iDoubleYear = doubleyear == "00" ?
                Convert.ToInt32((Convert.ToInt32(year.Substring(0, 2)) + 1).ToString() + doubleyear) :
                Convert.ToInt32(year.Substring(0, 2) + doubleyear);
            if (iDoubleYear - iYear != 1)
            {
                DoubleDateError = "Double date years must be for adjacent years.";
                return false; // must only be 1 year between double years
            }
            DoubleDate = true; // passed all checks
            return DoubleDate;
        }

        #endregion

        #region Properties

        public FactDate AverageDate
        {
            get
            {
                if (DateString.Equals("UNKNOWN"))
                    return UNKNOWN_DATE;
                if (StartDate == MINDATE)
                    return new FactDate(EndDate, EndDate);
                if (EndDate == MAXDATE)
                    return new FactDate(StartDate, StartDate);
                TimeSpan ts = EndDate.Subtract(StartDate);
                double midPointSeconds = ts.TotalSeconds / 2.0;
                DateTime average = StartDate.AddSeconds(midPointSeconds);
                return new FactDate(average, average);
            }
        }

        #endregion

        #region Logical operations
        /*
         * @return whether that FactDate is before this FactDate
         */
        public bool IsBefore(FactDate that)
        {
            if (!DoubleDate && that != null && that.DoubleDate)
                return EndDate < that.StartDate || EndDate < that.StartDate.TryAddYears(-1);
            // easy case is extremes whole of date before other
            return (that == null) ? true : EndDate < that.StartDate;
        }

        /*
         * @return whether that FactDate starts before this FactDate
         */
        public bool StartsBefore(FactDate that)
        {
            if (!DoubleDate && that != null && that.DoubleDate)
                return StartDate < that.StartDate || StartDate < that.StartDate.TryAddYears(-1);
            return (that == null) ? true : StartDate < that.StartDate;
        }

        /*
         * @return whether that FactDate is after this FactDate
         */
        public bool IsAfter(FactDate that)
        {
            if (DoubleDate && that != null && !that.DoubleDate)
                return StartDate > that.EndDate || StartDate.TryAddYears(-1) > that.EndDate;
            // easy case is extremes whole of date after other
            return (that == null) ? true : StartDate > that.EndDate;
        }

        /*
         * @return whether that FactDate ends after this FactDate
         */
        public bool EndsAfter(FactDate that)
        {
            if (DoubleDate && that != null && !that.DoubleDate)
                return EndDate > that.EndDate || EndDate.TryAddYears(-1) > that.EndDate;
            return (that == null) ? true : EndDate > that.EndDate;
        }

        public bool Overlaps(FactDate that)
        {
            // two dates overlap if not entirely before or after
            return (that == null) ? true : !(IsBefore(that) || IsAfter(that));
        }

        public bool IsNotBEForeOrAFTer => StartDate != MINDATE && EndDate != MAXDATE;

        public bool FactYearMatches(FactDate factDate)
        {
            if (factDate == null) return false;
            if (factDate.StartDate.Year != factDate.EndDate.Year ||
                 this.StartDate.Year != this.EndDate.Year) return false;
            // both this & that have exact years now return whether this and that match
            return this.StartDate.Year == factDate.StartDate.Year;
        }

        public bool CensusYearMatches(CensusDate censusDate)
        {
            if (censusDate == null) return false;
           if(this.StartDate.Year != this.EndDate.Year) return false;
            // both this & that have exact years now return whether this and that match given a census date can go over a year end
            return this.StartDate.Year == censusDate.StartDate.Year || this.StartDate.Year == censusDate.EndDate.Year;
        }

        public bool Contains(FactDate that)
        {
            return (that == null) ? true :
                (this.StartDate < that.StartDate && this.EndDate > that.EndDate);
        }

        public bool IsLongYearSpan
        {
            get
            {
                int diff = Math.Abs(StartDate.Year - EndDate.Year);
                return (diff > 5);
            }
        }

        public bool IsExact => StartDate.Equals(EndDate);

        public bool IsKnown => !Equals(UNKNOWN_DATE);

        public int BestYear
        {
            get
            {
                if (!IsKnown)
                    return 0;
                if (StartDate == MINDATE)
                    return EndDate.Year;
                if (EndDate == MAXDATE)
                    return StartDate.Year;
                return StartDate.Year + (int)((EndDate.Year - StartDate.Year) / 2);
            }
        }

        public double Distance(FactDate when)
        {
            double startDiff = ((StartDate.Year - when.StartDate.Year) * 12) + (StartDate.Month - when.StartDate.Month);
            double endDiff = ((EndDate.Year - when.EndDate.Year) * 12) + (EndDate.Month - when.EndDate.Month);
            double difference = Math.Sqrt(Math.Pow(startDiff, 2.0) + Math.Pow(endDiff, 2.0));
            return difference;
        }

        public BMDColour DateStatus(bool ignoreUnknown)
        {
            // EMPTY = dark grey, UNKNOWN_DATE = red, OPEN_ENDED_DATE = ??, VERY_WIDE_DATE = orange_red, WIDE_DATE = orange, 
            // NARROW_DATE = yellow, JUST_YEAR_DATE = yellow_green, APPROX_DATE = pale green, EXACT_DATE = lawn_green
            if (DateType == FactDateType.UNK)
                return ignoreUnknown ? BMDColour.EMPTY : BMDColour.UNKNOWN_DATE;
            if (DateType == FactDateType.BEF || DateType == FactDateType.AFT)
                return BMDColour.OPEN_ENDED_DATE;
            TimeSpan ts = EndDate - StartDate;
            if (ts.Days > 365.25 * 10)
                return BMDColour.VERY_WIDE_DATE; // more than 10 years
            if (ts.Days > 365.25 * 2)
                return BMDColour.WIDE_DATE; // over 2 years less than 10 years
            if (ts.Days > 365.25)
                return BMDColour.NARROW_DATE; // over 1 year less than 2 years
            else if (ts.Days > 125)
                return BMDColour.JUST_YEAR_DATE; // more than 4 months less than 1 year
            else if (ts.Days > 0)
                return BMDColour.APPROX_DATE; // less than 4 months not exact
            else
                return BMDColour.EXACT_DATE; // exact date
        }

        #endregion

        #region Overrides
        public override bool Equals(object that)
        {
            if (that == null || !(that is FactDate))
                return false;
            FactDate f = (FactDate)that;
            // two FactDates are equal if same datestring or same start and- enddates
            return (DateString.ToUpper().Equals(f.DateString.ToUpper())) ||
                   (StartDate.Equals(f.StartDate) && EndDate.Equals(f.EndDate));
        }

        public static bool operator ==(FactDate a, FactDate b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if ((a is null) || (b is null))
            {
                return false;
            }
            return a.Equals(b);
        }


        public static bool operator !=(FactDate a, FactDate b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(FactDate that)
        {
            if (Equals(that))
                return 0;
            else if (StartDate.Equals(that.StartDate))
                return EndDate.CompareTo(that.EndDate);
            else
                return StartDate.CompareTo(that.StartDate);
        }

        public override string ToString()
        {
            if (DateString.StartsWith("BET 1 JAN"))
                return "BET " + DateString.Substring(10);
            else if (DateString.StartsWith("AFT 1 JAN"))
                return "AFT " + DateString.Substring(10);
            else
                return DateString;
        }
        #endregion

        public static Regex NonGEDCOMDateFormatRegex
        {
            get
            {
                if (_regex == null)
                    _regex = new Regex(Properties.NonGedcomDate.Default.Regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return _regex;
            }

            set
            {
                _regex = value;
            }
        }
    }
}
