using FTAnalyzer.Exports;
using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static FTAnalyzer.ColourValues;

namespace FTAnalyzer
{
    public class FactDate : IComparable<FactDate>, IComparable
    {
        public static readonly DateTime MINDATE = new DateTime(1, 1, 1);
        public static readonly DateTime MAXDATE = new DateTime(9999, 12, 31);
        public static readonly IFormatProvider CULTURE = new CultureInfo("en-GB", true);
        public static readonly int MAXYEARS = 110;
        public static readonly int MINYEARS;
        const int LOW = 0;
        const int HIGH = 1;

        public readonly static string FULL = "d MMM yyyy";
        const string SPECIAL_DATE = "SPECIAL";
        const string YEAR = "yyyy";
        const string EARLYYEAR = "yyy";
        const string MONTHYEAR = "MMM yyyy";
        const string MONTHYEAREARLY = "MMM yyy";
        const string DAYMONTH = "d MMM";
        const string MONTH = "MMM";
        const string FULLEARLY = "d MMM yyy";
        const string DISPLAY = "d MMM yyyy";
        const string CHECKING = "d MMM";
        const string DATE_PATTERN = "^(\\d{0,2} )?([A-Z]{0,3}) *(\\d{0,4})$";
        const string INTERPRETED_DATE_PATTERN = "^INT (\\d{0,2} )?([A-Z]{0,3}) *(\\d{0,4}) .*$";
        const string EARLY_DATE_PATTERN = "^(\\d{3})$";
        const string DOUBLE_DATE_PATTERN = "^(\\d{0,2} )?([A-Z]{0,3}) *(\\d{0,4})/(\\d{0,2})$";
        const string DOUBLE_DATE_PATTERN2 = "^(\\d{0,2} )?([A-Z]{0,3}) *(\\d{4})/(\\d{4})$";
        const string DOUBLE_DATE_PATTERN3 = "^(\\d{0,2} )?([A-Z]{0,3}) *(\\d{3})/(\\d{2,3})$";
        const string POSTFIX = "(\\d{1,2})(?:ST|ND|RD|TH)(.*)";
        const string BETWEENFIX = "(\\d{4}) *- *(\\d{4})";
        const string BETWEENFIX2 = "([A-Z]{0,3}) *(\\d{4}) *- *([A-Z]{0,3}) *(\\d{4})";
        const string BETWEENFIX3 = "(\\d{0,2} )?([A-Z]{0,3}) *(\\d{4}) *- *(\\d{0,2} )?([A-Z]{0,3}) *(\\d{4})";
        const string BETWEENFIX4 = "(\\d{1,2}) *- *(\\d{1,2} )?([A-Z]{0,3}) *(\\d{4})";
        const string BETWEENFIX5 = "(\\d{1,2} )?([A-Z]{0,3}) *- *(\\d{1,2} )?([A-Z]{0,3}) *(\\d{4})";
        const string USDATEFIX = "^([A-Z]{3}) *(\\d{1,2} )(\\d{4})$";
        const string SPACEFIX = "^(\\d{1,2}) *([A-Z]{3}) *(\\d{0,4})$";
        const string QUAKERFIX = "^(\\d{1,2})D (\\d{1,2})M (\\d{0,4})$";

        public static FactDate UNKNOWN_DATE;
        public static FactDate MARRIAGE_LESS_THAN_13;
        public static FactDate SAME_SEX_MARRIAGE;
        public static FactDate TODAY;
        public static DateTime NOW;

        static readonly Dictionary<string, Regex> _datePatterns;
        static Regex _regex;

        static FactDate()
        {
            _datePatterns = new Dictionary<string, Regex>
            {
                ["DATE_PATTERN"] = new Regex(DATE_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["INTERPRETED_DATE_PATTERN"] = new Regex(INTERPRETED_DATE_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EARLY_DATE_PATTERN"] = new Regex(EARLY_DATE_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["DOUBLE_DATE_PATTERN"] = new Regex(DOUBLE_DATE_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["DOUBLE_DATE_PATTERN2"] = new Regex(DOUBLE_DATE_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["DOUBLE_DATE_PATTERN3"] = new Regex(DOUBLE_DATE_PATTERN3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["POSTFIX"] = new Regex(POSTFIX, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX"] = new Regex(BETWEENFIX, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX2"] = new Regex(BETWEENFIX2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX3"] = new Regex(BETWEENFIX3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX4"] = new Regex(BETWEENFIX4, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["BETWEENFIX5"] = new Regex(BETWEENFIX5, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["USDATEFIX"] = new Regex(USDATEFIX, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["SPACEFIX"] = new Regex(SPACEFIX, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["QUAKERFIX"] = new Regex(QUAKERFIX, RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            UNKNOWN_DATE = new FactDate("UNKNOWN");
            MARRIAGE_LESS_THAN_13 = new FactDate("1600");
            SAME_SEX_MARRIAGE = new FactDate("AFT 1 APR 2001");
            TODAY = new FactDate(DateTime.Now.ToString("dd MMM yyyy", CULTURE));
            NOW = TODAY.StartDate;
        }

        public enum FactDateType { BEF, AFT, BET, ABT, UNK, EXT }
        public enum NonGEDCOMFormatSelected { NONE = 0, DD_MM_YYYY = 1, MM_DD_YYYY = 2, YYYY_MM_DD = 3, YYYY_DD_MM = 4 }

        public string DateString { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public FactDateType DateType { get; private set; }
        public bool SpecialDate { get; private set; }
        public string OriginalString { get; private set; }
        string DoubleDateError;

        public bool DoubleDate { get; private set; } // Is a pre 1752 date bet 1 Jan and 25 Mar eg: 1735/36.
        int yearfix;

        public FactDate(string str, string factRef = "")
        {
            DoubleDate = false;
            SpecialDate = false;
            str = str ?? "UNKNOWN";
            OriginalString = str;
            // remove any commas in date string
            yearfix = 0;
            DateType = FactDateType.UNK;
            DateString = str.Length == 0 ? "UNKNOWN" : str.ToUpper();
            StartDate = MINDATE;
            EndDate = MAXDATE;
            str = FixTextDateFormats(str.ToUpper());
            if (str != SPECIAL_DATE)
            {
                str = FixCommonDateFormats(str);
                DateString = str.Length == 0 ? "UNKNOWN" : str.ToUpper();
                if (!DateString.Equals("UNKNOWN"))
                    ProcessDate(DateString, factRef);
            }
        }

        public FactDate(DateTime startdate, DateTime enddate)
        {
            DateType = FactDateType.UNK;
            StartDate = startdate;
            EndDate = enddate;
            DateString = CalculateDateString();
            OriginalString = string.Empty;
            SpecialDate = false;
            DoubleDate = false;
        }

        public static string Format(string format, DateTime date) => string.Format("{0:" + format + "}", date).ToUpper();

        string FixTextDateFormats(string str)
        {
            switch (str)
            {
                case "SUBMITTED":
                case "PRIVATE":
                    return UNKNOWN_DATE.ToString();
                case "DECEASED":
                case "DEAD":
                    str = $"BEF {TODAY}";
                    break;
                case "STILLBORN":
                case "INFANT":
                case "CHILD":
                case "YOUNG":
                case "UNMARRIED":
                case "NEVER MARRIED":
                case "NOT MARRIED":
                    str = SPECIAL_DATE;
                    SpecialDate = true;
                    break;
            }
            return str;
        }

        string FixCommonDateFormats(string str)
        {
            str = EnhancedTextInfo.RemoveSupriousDateCharacters(str.Trim().ToUpper());
            if (!Properties.NonGedcomDate.Default.UseNonGedcomDates || Properties.NonGedcomDate.Default.Separator != ".")
                str = str.Replace(".", " ");
            if (str.StartsWith("<") && str.EndsWith(">"))
                str = str.Replace("<", "").Replace(">", "");                   
            // remove date qualifiers first
            str = str.Replace("@#DGREGORIAN@", "").Replace("@#DJULIAN@", ""); //.Replace("@#DFRENCH R@", ""); // .Replace("@#DHEBREW@", "");
            str = str.Replace(". ", " "); // even if Non GEDCOM date separator is a dot, dot space is invalid.
            str = str.Replace("&", " AND ");
            str = str.Replace(",", " ").Replace("(", " ").Replace(")", " ").Replace("?", " ").Replace("!", " ");
            str = str.Replace("#", " ").Replace("$", " ").Replace("%", " ").Replace("^", " ").Replace("'", " ");
            str = str.Replace(":", " ").Replace(";", " ").Replace("@", " ").Replace("=", " ").Replace("?", " ");
            str = str.Replace("~", "ABT ").Replace("<", "BEF ").Replace(">", "AFT ").Replace("#", " ");
            str = str.Replace(" / ", "/").Replace("\'", " ").Replace("\"", " ").Replace("`", " ").ClearWhiteSpace();

            str = str.Replace("MONDAY", "").Replace("TUESDAY", "").Replace("WEDNESDAY", "").Replace("THURSDAY", "").Replace("FRIDAY", "").Replace("SATURDAY", "").Replace("SUNDAY", "");

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

            // fix missing space between month and year for 1000-2999
            str = str.Replace("JAN1", "JAN 1");
            str = str.Replace("FEB1", "FEB 1");
            str = str.Replace("MAR1", "MAR 1");
            str = str.Replace("APR1", "APR 1");
            str = str.Replace("JUN1", "JUN 1");
            str = str.Replace("JUL1", "JUL 1");
            str = str.Replace("AUG1", "AUG 1");
            str = str.Replace("SEP1", "SEP 1");
            str = str.Replace("OCT1", "OCT 1");
            str = str.Replace("NOV1", "NOV 1");
            str = str.Replace("DEC1", "DEC 1");
            str = str.Replace("JAN2", "JAN 2");
            str = str.Replace("FEB2", "FEB 2");
            str = str.Replace("MAR2", "MAR 2");
            str = str.Replace("APR2", "APR 2");
            str = str.Replace("JUN2", "JUN 2");
            str = str.Replace("JUL2", "JUL 2");
            str = str.Replace("AUG2", "AUG 2");
            str = str.Replace("SEP2", "SEP 2");
            str = str.Replace("OCT2", "OCT 2");
            str = str.Replace("NOV2", "NOV 2");
            str = str.Replace("DEC2", "DEC 2");

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
            str = str.Replace("PEU ", " "); //french little
            str = str.Replace("REC ", " "); //french census recusement
            str = str.Replace("  ", " ");

            //German
            str = str.Replace("DEZ", "DEC");
            str = str.Replace("MARZ", "MAR");
            str = str.Replace("JUNI", "JUN");
            str = str.Replace("JULI", "JUL");
            str = str.Replace("OKT", "OCT");
            str = str.Replace("JANNER", "JAN");
            str = str.Replace("JANUAR", "JAN");
            str = str.Replace("FEBRUAR", "FEB");
            str = str.Replace("OKTOBER", "OCT");
            str = str.Replace("DEZEMBER", "DEC");

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
            str = str.Replace("CA", "ABT");

            // Quarters
            str = str.Replace("QUARTER", "QTR");
            str = str.Replace("MAR QTR", "ABT MAR");
            str = str.Replace("MAR Q ", "ABT MAR ");
            str = str.Replace("JAN FEB MAR", "ABT MAR");
            str = str.Replace("JAN-MAR", "ABT MAR");
            str = str.Replace("JAN-FEB-MAR", "ABT MAR");
            str = str.Replace("JAN/FEB/MAR", "ABT MAR");
            str = str.Replace("JAN\\FEB\\MAR", "ABT MAR");
            str = str.Replace("1ST", "1");
            str = str.Replace("2ND", "2");
            str = str.Replace("3RD", "3");
            str = str.Replace("4TH", "4");
            str = str.Replace("Q1", "ABT MAR");
            str = str.Replace("1Q", "ABT MAR");
            str = str.Replace("QTR1", "ABT MAR");
            str = str.Replace("QTR 1 ", "ABT MAR ");
            str = str.Replace("1 QTR ", "ABT MAR ");
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
            str = str.Replace("2 QTR ", "ABT JUN ");
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
            str = str.Replace("3 QTR ", "ABT SEP ");
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
            str = str.Replace("4 QTR ", "ABT DEC ");

            // post processing tidy ups
            str = str.Replace("ABT ABT", "ABT"); // fix any ABT X QTR's that will have been changed to ABT ABT
            str = str.Replace("BET ABT", "ABT"); // fix any BET XXX-XXX QTR's that will have been changed to BET ABT
            str = str.Replace("ABT1", "ABT 1");
            str = str.Replace("ABT2", "ABT 2");
            str = str.Replace("MON", "");
            str = str.Replace("TUE", "");
            str = str.Replace("WED", "");
            str = str.Replace("THU", "");
            str = str.Replace("FRI", "");
            str = str.Replace("SAT", "");
            str = str.Replace("SUN", "");

            // remove common extra words
            str = str.Replace("DIED IN INFANCY", "INFANT");
            str = str.Replace("CENSUS", "");

            // deal with CE/AD and BCE or BC date
            str = str.Replace("B C E", "BCE").Replace("C E", "CE").Replace("B C", "BC").Replace("A D", "AD").TrimEnd();
            if (str.EndsWith("CE"))
                str = str.Replace("CE", "");
            if (str.EndsWith("AD"))
                str = str.Replace("AD", "");
            if (str.EndsWith("BCE") || str.EndsWith("BC"))
                return "UNKNOWN";

            // process date
            if (str.IndexOf("TO", StringComparison.Ordinal) > 1)
            {  // contains TO but doesn't start with TO
                if (!str.StartsWith("FROM", StringComparison.Ordinal))
                    str = "FROM " + str;
            }
            if (str.StartsWith("FROM", StringComparison.Ordinal))
            {
                if (str.IndexOf("TO", StringComparison.Ordinal) > 0)
                    str = str.Replace("FROM", "BET").Replace("TO", "AND");
                else
                {
                    str = str.Replace("FROM", "AFT"); // year will be one out
                    yearfix = -1;
                }
            }
            if (str.StartsWith("TO", StringComparison.Ordinal))
            {
                str = str.Replace("TO", "BEF"); // year will be one out
                yearfix = +1;
            }
            if (str.StartsWith(">", StringComparison.Ordinal))
                str = str.Replace(">", "AFT ");
            if (str.StartsWith("<", StringComparison.Ordinal))
                str = str.Replace("<", "BEF ");
            if (str.StartsWith("~", StringComparison.Ordinal))
                str = str.Replace("~", "ABT ");
            if (str.StartsWith("C1", StringComparison.Ordinal) || str.StartsWith("C2", StringComparison.Ordinal) ||
                str.StartsWith("C 1", StringComparison.Ordinal) || str.StartsWith("C 2", StringComparison.Ordinal))
                str = "ABT " + str.Substring(1);
            str = str.Replace("  ", " "); // fix issue if > or < or Cxxx has already got a space
            Match matcher;
            if (str.StartsWith("INT", StringComparison.Ordinal)) // Interpreted date but we can discard <<Date_Phrase>>
            {
                matcher = _datePatterns["INTERPRETED_DATE_PATTERN"].Match(str);
                if (matcher.Success)
                {
                    string result = matcher.Groups[1] + matcher.Groups[2].ToString() + " " + matcher.Groups[3];
                    return result.Trim();
                }
            }
            matcher = _datePatterns["POSTFIX"].Match(str);
            if (matcher.Success)
            {
                string result = $"{matcher.Groups[1]}{matcher.Groups[2]}";
                return result.Trim();
            }
            if (Properties.NonGedcomDate.Default.UseNonGedcomDates)
            {
                matcher = NonGEDCOMDateFormatRegex.Match(str);
                if (!matcher.Success) // match string is not a non gedcom format with dashes so proceed with between fixes
                {
                    Tuple<bool, string> result = BetweenFixes(str);
                    if (result.Item1)
                        return result.Item2.Trim();
                }
            }
            else
            {
                Tuple<bool, string> result = BetweenFixes(str);
                if (result.Item1)
                    return result.Item2.Trim();
            }
            matcher = _datePatterns["USDATEFIX"].Match(str);
            if (matcher.Success)
            {
                string result = $"{matcher.Groups[2]}{matcher.Groups[1]} {matcher.Groups[3]}";
                return result.Trim();
            }
            matcher = _datePatterns["SPACEFIX"].Match(str);
            if (matcher.Success)
            {
                string result = $"{matcher.Groups[1]} {matcher.Groups[2]} {matcher.Groups[3]}";
                return result.Trim();
            }
            matcher = _datePatterns["QUAKERFIX"].Match(str);
            if (matcher.Success)
            {
                int day = int.Parse(matcher.Groups[1].ToString());
                int month = 2 + int.Parse(matcher.Groups[2].ToString());
                if (month > 12) month -= 12;
                int year = int.Parse(matcher.Groups[3].ToString());
                return new DateTime(year, month, day).ToString("dd MMMM yyyy");
            }
            str = str.TrimEnd('-');
            return str.Trim();
        }

        Tuple<bool, string> BetweenFixes(string str)
        {
            Match matcher = _datePatterns["BETWEENFIX"].Match(str);
            if (matcher.Success)
                return new Tuple<bool, string>(true, $"BET {matcher.Groups[1]} AND {matcher.Groups[2]}");
            matcher = _datePatterns["BETWEENFIX2"].Match(str);
            if (matcher.Success)
                return new Tuple<bool, string>(true, $"BET {matcher.Groups[1]} {matcher.Groups[2]} AND {matcher.Groups[3]} {matcher.Groups[4]}");
            matcher = _datePatterns["BETWEENFIX3"].Match(str);
            if (matcher.Success)
                return new Tuple<bool, string>(true, $"BET {matcher.Groups[1]}{matcher.Groups[2]} {matcher.Groups[3]} AND {matcher.Groups[4]}{matcher.Groups[5]} {matcher.Groups[6]}");
            matcher = _datePatterns["BETWEENFIX4"].Match(str);
            if (matcher.Success)
                return new Tuple<bool, string>(true, $"BET {matcher.Groups[1]} {matcher.Groups[3]} {matcher.Groups[4]} AND {matcher.Groups[2]}{matcher.Groups[3]} {matcher.Groups[4]}");
            matcher = _datePatterns["BETWEENFIX5"].Match(str);
            if (matcher.Success)
                return new Tuple<bool, string>(true, $"BET {matcher.Groups[1]}{matcher.Groups[2]} {matcher.Groups[5]} AND {matcher.Groups[3]}{matcher.Groups[4]} {matcher.Groups[5]}");
            return new Tuple<bool, string>(false, string.Empty);
        }

        #region Process Dates

        public FactDate SubtractMonths(int months)
        {
            DateTime start = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day);
            DateTime end = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day);
            start = StartDate < MINDATE.AddMonths(months) ? MINDATE : start.AddMonths(-months);
            end = EndDate < MINDATE.AddMonths(months) ? MINDATE : end.AddMonths(-months);
            return new FactDate(start, end);
        }

        public FactDate AddEndDateYears(int years)
        {
            DateTime start = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day);
            DateTime end = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day);
            end = end >= MAXDATE.AddYears(-years) ? MAXDATE : end.AddYears(years);
            return new FactDate(start, end);
        }

        string CalculateDateString()
        {
            string check;
            StringBuilder output = new StringBuilder();
            if (StartDate == MINDATE)
            {
                if (EndDate == MAXDATE)
                    return "UNKNOWN";
                DateType = FactDateType.BEF;
                output.Append("BEF ");
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

        void ProcessDate(string processDate, string factRef)
        {
            // takes datestring and works out start and end dates 
            // prefixes are BEF, AFT, BET and nothing
            // dates are "YYYY" or "MMM YYYY" or "DD MMM YYYY"
            try
            {
                string dateValue = processDate.Length >= 4 ? processDate.Substring(4) : processDate;
                if (processDate.StartsWith("BEF", StringComparison.Ordinal))
                {
                    DateType = FactDateType.BEF;
                    EndDate = ParseDate(dateValue, HIGH, -1 + yearfix);
                }
                else if (processDate.StartsWith("AFT", StringComparison.Ordinal))
                {
                    DateType = FactDateType.AFT;
                    StartDate = ParseDate(dateValue, LOW, +1 + yearfix);
                }
                else if (processDate.StartsWith("ABT", StringComparison.Ordinal))
                {
                    DateType = FactDateType.ABT;
                    if (processDate.StartsWith("ABT MAR", StringComparison.Ordinal) || processDate.StartsWith("ABT JUN", StringComparison.Ordinal)
                         || processDate.StartsWith("ABT SEP", StringComparison.Ordinal) || processDate.StartsWith("ABT DEC", StringComparison.Ordinal))
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
                else if (processDate.StartsWith("BET", StringComparison.Ordinal))
                {
                    string fromdate;
                    string todate;
                    DateType = FactDateType.BET;
                    int pos = processDate.IndexOf(" AND ", StringComparison.Ordinal);
                    if (pos == -1)
                    {
                        pos = processDate.IndexOf("-", StringComparison.Ordinal);
                        byte[] asciiBytes = Encoding.ASCII.GetBytes(processDate);
                        if (pos == -1)
                            throw new FactDateException("Invalid BETween date no AND found");
                        fromdate = processDate.Substring(4, pos - 4);
                        todate = processDate.Substring(pos + 1);
                        pos -= 4; // from to code in next block expects to jump 5 chars not 1.
                    }
                    else
                    {
                        fromdate = processDate.Substring(4, pos - 4);
                        todate = processDate.Substring(pos + 5);
                    }
                    if (fromdate.Length < 3)
                        fromdate = fromdate + " " + processDate.Substring(pos + 7);
                    else if (fromdate.Length == 3 && !fromdate.StartsWithNumeric())
                        fromdate = "01 " + fromdate + processDate.Substring(pos + 8);
                    else if (fromdate.Length == 4)
                        fromdate = "01 JAN " + fromdate;
                    else if (fromdate.Length < 7 && fromdate.IndexOf(" ", StringComparison.Ordinal) > 0)
                        fromdate = fromdate + " " + processDate.Substring(pos + 11);
                    StartDate = ParseDate(fromdate.Replace("  ", " "), LOW, 0, EndDate.Year);
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

        DateTime ParseDate(string dateValue, int highlow, int adjustment) => ParseDate(dateValue, highlow, adjustment, 1);

        DateTime ParseDate(string dateValue, int highlow, int adjustment, int defaultYear)
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
                    Match matcher3 = _datePatterns["DOUBLE_DATE_PATTERN3"].Match(dateValue);
                    if (matcher.Success)
                    {
                        gDay = matcher.Groups[1];
                        gMonth = matcher.Groups[2];
                        gYear = matcher.Groups[3];
                        gDouble = matcher.Groups[4];
                        if (dateValue.IndexOf("/", StringComparison.Ordinal) > 0)
                            dateValue = dateValue.Substring(0, dateValue.IndexOf("/", StringComparison.Ordinal)); // remove the trailing / and 1 or 2 digits
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
                    else if (matcher3.Success)
                    {
                        gDay = matcher3.Groups[1];
                        gMonth = matcher3.Groups[2];
                        gYear = matcher3.Groups[3];
                        gDouble = matcher3.Groups[4];
                        if (dateValue.IndexOf("/", StringComparison.Ordinal) > 0)
                            dateValue = dateValue.Substring(0, dateValue.IndexOf("/", StringComparison.Ordinal)); // remove the trailing / and 1 or 2 digits
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
                            string standardFormat = new DateTime(int.Parse(gYear.Value), int.Parse(gMonth.Value), int.Parse(gDay.Value)).ToString("dd MMM yyyy").ToUpper();
                            dateValue = dateValue.Length > matcher2.Length
                                ? dateValue.Substring(0, matcher2.Index) + standardFormat + dateValue.Substring(matcher2.Index + matcher2.Length, dateValue.Length)
                                : standardFormat;
                        }
                    }
                    else
                        throw new FactDateException($"Unrecognised date format for: {dateValue}");
                }
                // Now process matched string - if gDouble is not null we have a double date to check
                string day = gDay is null ? string.Empty : gDay.ToString().Trim();
                string month = gMonth is null ? string.Empty : gMonth.ToString().Trim();
                string year = gYear is null ? string.Empty : gYear.ToString().Trim();

                if (!IsValidDoubleDate(month, year, gDouble))
                    throw new InvalidDoubleDateException(DoubleDateError);
                if (day.Length == 0 && month.Length == 0)
                {
                    date = year.Length == 4 ? DateTime.ParseExact(dateValue, YEAR, CULTURE) : DateTime.ParseExact(dateValue, EARLYYEAR, CULTURE);
                    dt = highlow == HIGH ? new DateTime(date.Year + adjustment, 12, 31) : new DateTime(date.Year + adjustment, 1, 1);
                }
                else if (day.Length == 0 && year.Length > 0)
                {
                    if (!DateTime.TryParseExact(dateValue, MONTHYEAR, CULTURE, DateTimeStyles.NoCurrentDateDefault, out date))
                        DateTime.TryParseExact(dateValue, MONTHYEAREARLY, CULTURE, DateTimeStyles.NoCurrentDateDefault, out date);
                    dt = new DateTime(date.Year, date.Month, 1);
                    if (dt != FactDate.MINDATE)
                    {
                        dt = dt.AddMonths(adjustment);
                        if (highlow == HIGH)
                        {
                            // at 1st of month so add 1 month to first of next month
                            dt = dt.AddMonths(1);
                            // then subtract 1 day to be last day of correct month.
                            dt = dt.AddDays(-1);
                        }
                    }
                }
                else if (day.Length == 0 && year.Length == 0)
                {
                    date = DateTime.ParseExact(dateValue, MONTH, CULTURE);
                    dt = new DateTime(defaultYear, date.Month, 1);
                    if (dt != MAXDATE.AddDays(-30)) // ignore if 1st of Dec 9999
                    {
                        if (highlow == HIGH)
                        {
                            // at 1st of month so add 1 month to first of next month
                            dt = dt.AddMonths(1);
                            // then subtract 1 day to be last day of correct month.
                            dt = dt.AddDays(-1);
                        }
                    }
                }
                else if (year.Length == 0)
                {
                    date = DateTime.ParseExact(dateValue, DAYMONTH, CULTURE);
                    dt = new DateTime(defaultYear, date.Month, date.Day);
                }
                else if (day.Length > 0 && month.Length > 0 && year.Length > 0)
                {
                    if (gDouble != null && day == "29" && month == "FEB")
                    {
                        int doubleYear = int.Parse(year) + 1;
                        dateValue = $"29 FEB {doubleYear}";
                    }
                    if (!DateTime.TryParseExact(dateValue, FULL, CULTURE, DateTimeStyles.NoCurrentDateDefault, out date))
                        DateTime.TryParseExact(dateValue, FULLEARLY, CULTURE, DateTimeStyles.NoCurrentDateDefault, out date);
                    if (date.Year == 1 && date.Year.ToString() != year)
                    {
                        // we have valid date format but invalid day/month combo eg: 29th Feb in odd year.
                        throw new FactDateException("Date has normal format but is not a valid date. eg: like 31 NOV 1900 or 29 FEB 1735");
                    }
                    else
                        dt = new DateTime(date.Year, date.Month, date.Day);
                    dt = dt.AddDays(adjustment);
                }
                else
                {
                    dt = (highlow == HIGH) ? MAXDATE : MINDATE;
                }
                if (gDouble != null && !(day == "29" && month == "FEB"))
                    dt = dt.TryAddYears(1); // use upper year for double dates as long as we haven't already done so for 29th FEB
            }
            catch (FormatException)
            {
                throw new FactDateException($"Unrecognised date format for: {dateValue}");
            }
            catch (Exception e)
            {
                dt = (highlow == HIGH) ? MAXDATE : MINDATE;
                throw new FactDateException($"Problem with date format for: {dateValue} system said: {e.Message}");
            }
            return dt;
        }

        bool IsValidDoubleDate(string month, string year, Group gDouble)
        {
            DoubleDate = false;   // set property
            DoubleDateError = string.Empty;
            if (gDouble is null)  // normal date so its valid double date
                return true;
            // check if valid double date if so set double date to true
            string doubleyear = gDouble.ToString().Trim();
            if (doubleyear.Length == 4)
                doubleyear = doubleyear.Substring(2);
            if (doubleyear.Length == 3)
                doubleyear = doubleyear.Substring(1, 2);
            if (doubleyear.Length == 1 && year.Length >= 2)
                doubleyear = year.Substring(year.Length - 2, 1) + doubleyear;
            if (doubleyear is null || doubleyear.Length < 2 || doubleyear.Length > 4 || year.Length < 3)
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
            int iDoubleYear;
            if (year.Length == 3)
            {
                iDoubleYear = doubleyear == "00" ?
                Convert.ToInt32((Convert.ToInt32(year.Substring(0, 1)) + 1).ToString() + doubleyear) :
                Convert.ToInt32(year.Substring(0, 1) + doubleyear);
            }
            else
            {
                iDoubleYear = doubleyear == "00" ?
                Convert.ToInt32((Convert.ToInt32(year.Substring(0, 2)) + 1).ToString() + doubleyear) :
                Convert.ToInt32(year.Substring(0, 2) + doubleyear);
            }
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
            return that is null || EndDate < that.StartDate;
        }

        /*
         * @return whether that FactDate starts before this FactDate
         */
        public bool StartsBefore(FactDate that)
        {
            if (!DoubleDate && that != null && that.DoubleDate)
                return StartDate < that.StartDate || StartDate < that.StartDate.TryAddYears(-1);
            return that is null || StartDate < that.StartDate;
        }

        public bool StartsOnOrBefore(FactDate that)
        { 
            if (!DoubleDate && that != null && that.DoubleDate)
                return StartDate < that.StartDate || StartDate < that.StartDate.TryAddYears(-1);
            return that is null || StartDate <= that.StartDate;
        }

        /*
         * @return whether that FactDate is after this FactDate
         */
        public bool IsAfter(FactDate that)
        {
            if (DoubleDate && that != null && !that.DoubleDate)
                return StartDate > that.EndDate || StartDate.TryAddYears(-1) > that.EndDate;
            // easy case is extremes whole of date after other
            return that is null || StartDate > that.EndDate;
        }

        /*
         * @return whether that FactDate ends after this FactDate
         */
        public bool EndsAfter(FactDate that)
        {
            if (DoubleDate && that != null && !that.DoubleDate)
                return EndDate > that.EndDate || EndDate.TryAddYears(-1) > that.EndDate;
            return that is null || EndDate > that.EndDate;
        }

        public bool Overlaps(FactDate that)
        {
            // two dates overlap if not entirely before or after
            return that is null || !(IsBefore(that) || IsAfter(that));
        }

        public bool IsNotBEForeOrAFTer => StartDate != MINDATE && EndDate != MAXDATE;

        public bool FactYearMatches(FactDate factDate)
        {
            if (factDate is null) return false;
            if (factDate.StartDate.Year != factDate.EndDate.Year ||
                 StartDate.Year != EndDate.Year) return false;
            // both this & that have exact years now return whether this and that match
            return StartDate.Year == factDate.StartDate.Year;
        }

        public bool CensusYearMatches(CensusDate censusDate)
        {
            if (IsAfter(censusDate)) return false; // if the date is after the census date then it can't be a census record
            if (censusDate is null) return false;
            if (StartDate.Year != EndDate.Year) return false;
            // both this & that have exact years now return whether this and that match given a census date can go over a year end
            return StartDate.Year == censusDate.StartDate.Year || StartDate.Year == censusDate.EndDate.Year;
        }

        public bool Contains(FactDate that) => that is null || StartDate < that.StartDate && EndDate > that.EndDate;

        public bool IsLongYearSpan => Math.Abs(StartDate.Year - EndDate.Year) > 5;

        public bool IsExact => StartDate.Equals(EndDate);

        public bool IsKnown => !Equals(UNKNOWN_DATE);
        public bool IsUnknown => Equals(UNKNOWN_DATE);

        public int BestYear
        {
            get
            {
                if (IsUnknown)
                    return 0;
                if (StartDate == MINDATE)
                    return EndDate.Year;
                if (EndDate == MAXDATE)
                    return StartDate.Year;
                return StartDate.Year + ((EndDate.Year - StartDate.Year) / 2);
            }
        }

        public long DistanceSquared(FactDate when)
        {
            long startDiff = ((StartDate.Year - when.StartDate.Year) * 12) + (StartDate.Month - when.StartDate.Month);
            long endDiff = ((EndDate.Year - when.EndDate.Year) * 12) + (EndDate.Month - when.EndDate.Month);
            long difference = startDiff * startDiff + endDiff * endDiff;
            return difference;
            //double startDiff = ((StartDate.Year - when.StartDate.Year) * 12) + (StartDate.Month - when.StartDate.Month);
            //double endDiff = ((EndDate.Year - when.EndDate.Year) * 12) + (EndDate.Month - when.EndDate.Month);
            //double difference = Math.Sqrt(Math.Pow(startDiff, 2.0) + Math.Pow(endDiff, 2.0));
            //return difference;
        }

        double DaysSpan => EndDate.Subtract(StartDate).TotalDays;

        public int MonthsDifference(CensusDate when)
        {
            if (when is null) return 0;
            if (DaysSpan > 366)
                return 12;
            var x = EndDate.Subtract(when.StartDate).TotalDays/30;
            var y = when.EndDate.Subtract(StartDate).TotalDays/30;
            double minGap = Math.Min(x, y);
            return (int)minGap;
        }

        public double DaysDifference(FactDate when)
        {
            if (when is null) return 0d;
            if (DaysSpan > 100 || when.DaysSpan > 100)
                return double.MaxValue;
            double minGap = Math.Min(Math.Abs(EndDate.Subtract(when.StartDate).TotalDays), Math.Abs(when.EndDate.Subtract(StartDate).TotalDays));
            return minGap;
        }

        public BMDColours DateStatus(bool ignoreUnknown)
        {
            // EMPTY = dark grey, UNKNOWN_DATE = red, OPEN_ENDED_DATE = ??, VERY_WIDE_DATE = orange_red, WIDE_DATE = orange, 
            // NARROW_DATE = yellow, JUST_YEAR_DATE = yellow_green, APPROX_DATE = pale green, EXACT_DATE = lawn_green
            if (DateType == FactDateType.UNK)
                return ignoreUnknown ? BMDColours.EMPTY : BMDColours.UNKNOWN_DATE;
            if (DateType == FactDateType.BEF || DateType == FactDateType.AFT)
                return BMDColours.OPEN_ENDED_DATE;
            TimeSpan ts = EndDate - StartDate;
            if (ts.Days > 365.25 * 10)
                return BMDColours.VERY_WIDE_DATE; // more than 10 years
            if (ts.Days > 365.25 * 2)
                return BMDColours.WIDE_DATE; // over 2 years less than 10 years
            if (ts.Days > 365.25)
                return BMDColours.NARROW_DATE; // over 1 year less than 2 years
            if (ts.Days > 125)
                return BMDColours.JUST_YEAR_DATE; // more than 4 months less than 1 year
            if (ts.Days > 0)
                return BMDColours.APPROX_DATE; // less than 4 months not exact
            return BMDColours.EXACT_DATE; // exact date
        }

        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is FactDate))
                return false;
            FactDate f = (FactDate)obj;
            // two FactDates are equal if same datestring or same start and- enddates
            return DateString.ToUpper().Equals(f.DateString.ToUpper()) ||
                   (StartDate.Equals(f.StartDate) && EndDate.Equals(f.EndDate));
        }

        public static bool operator ==(FactDate a, FactDate b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;
            // If one is null, but not both, return false.
            if ((a is null) || (b is null))
                return false;
            return a.Equals(b);
        }

        public static bool operator <(FactDate left, FactDate right) => left is null ? right is object : left.CompareTo(right) < 0;

        public static bool operator <=(FactDate left, FactDate right) => left is null || left.CompareTo(right) <= 0;

        public static bool operator >(FactDate left, FactDate right) => left is object && left.CompareTo(right) > 0;

        public static bool operator >=(FactDate left, FactDate right) => left is null ? right is null : left.CompareTo(right) >= 0;

        public static bool operator !=(FactDate a, FactDate b) => !(a == b);

        public override int GetHashCode() => base.GetHashCode();

        public int CompareTo(object that) => CompareTo(that as FactDate);

        public int CompareTo(FactDate that)
        {
            if (Equals(that))
                return 0;
            return StartDate.Equals(that.StartDate) ? EndDate.CompareTo(that.EndDate) : StartDate.CompareTo(that.StartDate);
        }

        public override string ToString()
        {
            if (DateString.StartsWith("BET 1 JAN", StringComparison.Ordinal))
                return "BET " + DateString.Substring(10);
            return DateString.StartsWith("AFT 1 JAN", StringComparison.Ordinal) ? "AFT " + DateString.Substring(10) : DateString;
        }
        #endregion

        public static Regex NonGEDCOMDateFormatRegex
        {
            get => _regex ?? new Regex(Properties.NonGedcomDate.Default.Regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            set => _regex = value;
        }
    }
}
