using FTAnalyzer.Properties;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public class CensusReference : IComparable<CensusReference>
    {
        static readonly string EW_CENSUS_PATTERN = @"RG *(\d{1,3}) *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN1 = @"RG *(\d{1,3}) *Piece\/Folio *(\d{1,5})[\/ ]*(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN2 = @"RG *(\d{1,3}) *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        static readonly string EW_CENSUS_PATTERN3 = @"(\d{4}) .*?Census.*? *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN4 = @"(\d{4}) .*?Census.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN5 = @"(\d{4}) .*?Census.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        static readonly string EW_CENSUS_PATTERN6 = @"Census *(\d{4}).*? *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN7 = @"Census *(\d{4}).*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN8 = @"Census *(\d{4}).*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        static readonly string EW_CENSUS_PATTERN9 = @"(\d{4}) *- *.*? *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN10 = @"(\d{4}) *- *.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN11 = @"(\d{4}) *- *.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        static readonly string EW_CENSUS_PATTERN12 = @"Piece (RG\d{1,2})\/(\d{1,5}).*? *Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN13 = @"(RG *\d{1,2})-(\d{1,5})-(\d{1,4}[a-z]?)-(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN14 = @"Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3}).*?(RG *\d{1,2})\/(\d{1,5})";

        static readonly string EW_CENSUS_PATTERN_FH = @"RG *(\d{1,2})\/(\d{1,5}) F(olio)? ?(\d{1,4}[a-z]?) P(age)? ?(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN_FH2 = @"RG *(\d{1,2})\/(\d{1,5}) ED *(\d{1,4}[a-z]?) F(olio)? ?(\d{1,4}[a-z]?) P(age)? ?(\d{1,3})";
        static readonly string EW_CENSUS_PATTERN_FH3 = @"RG *(\d{1,2}) *Piece *(\d{1,5}) ED *(\d{1,4}[a-z]?) F(olio)? ?(\d{1,4}[a-z]?) P(age)? ?(\d{1,3})";

        static readonly string EW_CENSUS_PATTERN_FS1 = @"(\d{4}).*?Census.*?p *(\d{1,3}) *Piece\/Folio *(\d{1,4}[a-z]?)\/(\d{1,3})";

        static readonly string EW_MISSINGCLASS_PATTERN = @"Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_MISSINGCLASS_PATTERN2 = @"Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        
        static readonly string EW_CENSUS_1841_51_PATTERN = @"HO *107 *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN2 = @"HO *107 *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN2A = @"HO *107 *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?).*?Book *(\d{1,3}) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN3 = @"HO *107 *Piece *(\d{1,5}) *(Book\/)?Folio *(\d{1,3}[a-z]?)?\/?(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN4 = @"HO *107 *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Page *(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN5 = @"HO *107 *Piece *(\d{1,5}).*?Page *(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN6 = @"Folio *(\d{1,4}[a-z]?)\/? *(Book)? *(\d{1,2})? *Page *(\d{1,3}).*?HO[ \/]?107\/(\d{1,5})\/?(\d{1,3})?";
        static readonly string EW_CENSUS_1841_51_PATTERN6A = @"Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?)\/?(\d{1,2})? *Page *(\d{1,3}).*?HO[ \/]?107\/(\d{1,5})";
        static readonly string EW_CENSUS_1841_51_PATTERN7 = @"HO *107-(\d{1,5})-(\d{1,3})-(\d{1,4}[a-z]?)-(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN_FH = @"HO *107\/(\d{1,5})\/(\d{1,3}) .*?F(olio)? *(\d{1,4}[a-z]?) p(age)? *(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN_FH2 = @"HO *107\/(\d{1,5}) ED *(\d{1,4}[a-z]?) F(olio)? *(\d{1,4}[a-z]?) p(age)? *(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN_FH3 = @"HO *107\/(\d{1,5}) .*?F(olio)? *(\d{1,4}[a-z]?)\/(\d{1,4}) p(age)? *(\d{1,3})";
        static readonly string EW_CENSUS_1841_51_PATTERN_FH4 = @"HO *107\/(\d{1,5}) .*?F(olio)? *(\d{1,4}[a-z]?) p(age)? *(\d{1,3})";

        static readonly string EW_CENSUS_1911_PATTERN = @"RG *14\/?( *Piece *)?(\d{1,6}) .*?SN *(\d{1,4})";
        static readonly string EW_CENSUS_1911_PATTERN2 = @"1911 Census.*? *Piece *(\d{1,6}) *SN *(\d{1,4})";
        static readonly string EW_CENSUS_1911_PATTERN3 = @"Census *1911.*? *Piece *(\d{1,6}) *SN *(\d{1,4})";
        static readonly string EW_CENSUS_1911_PATTERN4 = @"RG *14\/? *Piece *(\d{1,6})$";
        static readonly string EW_CENSUS_1911_PATTERN5 = @"RG *14\/? *Piece *(\d{1,6}) *Page *(\d{1,3})";
        static readonly string EW_CENSUS_1911_PATTERN6 = @"RG *14\/? *RD *(\d{1,4}) *ED *(\d{1,3}) (\d{1,5})";
        static readonly string EW_CENSUS_1911_PATTERN78 = @"RG *78\/? *Piece *(\d{1,6}) .*?SN *(\d{1,4})";
        static readonly string EW_CENSUS_1911_PATTERN78b = @"RG *78\/? *Piece *(\d{1,5})";

        static readonly string EW_1939_REGISTER_PATTERN1 = @"RG *101\/?\\? *(\d{1,6}[A-Z]?) *.\/?\\? *(\d{1,3}) *.\/?\\? *(\d{1,3}).+([A-Z]{4})";
        static readonly string EW_1939_REGISTER_PATTERN2 = @"RG *101\/?\\? *(\d{1,6}[A-Z]?).*? ED ([A-Z]{4}) RD (.*?) Marital";
        static readonly string EW_1939_REGISTER_PATTERN3 = @"RG *101\/?\\? *(\d{1,6}[A-Z]?)";

        static readonly string SCOT_CENSUSYEAR_PATTERN = @"(1[89]\d[15]).{1,10}(\(?GROS *\)?)?Parish *([A-Z .'-]+) *ED *(\d{1,3}[AB]?) *Page *(\d{1,4}) *Line *(\d{1,2})";
        static readonly string SCOT_CENSUSYEAR_PATTERN2 = @"(1[89]\d[15]).{1,10}(\(?GROS *\)?)?(\d{1,3}\/\d{1,2}[AB]?) (\d{3}\/\d{2}) (\d{3,4})";
        static readonly string SCOT_CENSUSYEAR_PATTERN3 = @"(1[89]\d[15]).{1,10}(\(?GROS *\)?)?(\d{1,3}[AB]?)\/(\d{2}[AB]?) Page *(\d{1,4})";
        static readonly string SCOT_CENSUSYEAR_PATTERN4 = @"SCT(1[89]\d[15])\/?(\d{1,3}[AB]?) *f(olio)? *(\d{1,3}[AB]?) *p(age)? *(\d{1,4})";
        static readonly string SCOT_CENSUS_PATTERN = @"Parish *([A-Z .'-]+) *ED *(\d{1,3}[AB]?) *Page *(\d{1,4}) *Line *(\d{1,2})";
        static readonly string SCOT_CENSUS_PATTERN2 = @"(\(?GROS *\)?)?(\d{1,3}\/\d{1,2}[AB]?) (\d{3}\/\d{2}) (\d{3,4})";
        static readonly string SCOT_CENSUS_PATTERN3 = @"(\(?GROS *\)?)?(\d{1,3}[AB]?)\/(\d{2}[AB]?) Page *(\d{1,4})";

        static readonly string US_CENSUS_PATTERN = @"Year *(\d{4}) *Census *(.*?) *Roll *(.*?) *Film (.*?) *P(age)? *(\d{1,4}[ABCD]?) *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?)";
        static readonly string US_CENSUS_PATTERN1A = @"Year *(\d{4}) *Census *(.*?),? *Roll *(.*?),? *P(age)? *(\d{1,4}[ABCD]?),? *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?)";
        static readonly string US_CENSUS_PATTERN2 = @"Census,? *(\d{4}) *(.*?) *Roll *(.*?),? *P(age)? *(\d{1,4}[ABCD]?),? *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?)";
        static readonly string US_CENSUS_PATTERN3 = @"Census,? *(\d{4}) *(.*?) *Ward *(.*?),? *ED *(\d{1,5}[ABCD]?-?\d{0,4}[AB]?),? *P(age)? *(\d{1,4}[AB]?)";
        static readonly string US_CENSUS_PATTERN4 = @"Census,? *(\d{4}) *(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[ABCD]?),? *P(age)? *(\d{1,4}[AB]?)(.*?)roll *(\d{1,4})";
        static readonly string US_CENSUS_PATTERN5 = @"Census,? *(\d{4}) *(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[ABCD]?),? *P(age)? *(\d{1,4}[AB]?)";
        static readonly string US_CENSUS_1940_PATTERN = @"District *(\d{1,5}[AB]?-?\d{0,4}[AB]?).*?P(age)? *(\d{1,3}[ABCD]?).*?T627 ?,? *(\d{1,5}-?[AB]?)";
        static readonly string US_CENSUS_1940_PATTERN2 = @"ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?).*? *P(age)? *(\d{1,3}[ABCD]?).*?T627.*?roll ?(\d{1,5}-?[AB]?)";
        static readonly string US_CENSUS_1940_PATTERN3 = @"1940 *(.*?)(Roll)? *M*?-*?_*?T0*?627_(.*?) *P(age)? *(\d{1,4}[ABCD]?) *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?)";
        static readonly string US_CENSUS_1940_PATTERN4 = @"Roll( *M*?-*?_*?T0*?627_)?(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?) *P(age)? *(\d{1,4}[ABCD]?)";

        static readonly string CANADA_CENSUS_PATTERN = @"Year *(\d{4}) *Census *(.*?) *Roll *(.*?) *P(age)? *(\d{1,4}[ABCD]?) *Family *(\d{1,4})";
        static readonly string CANADA_CENSUS_PATTERN2 = @"(\d{4}) *Census[ -]*District *(\d{1,5})\/(\d{0,4}[A-Z]{0,4}) *P(age)? *(\d{1,4}[ABCD]?) *Family *(\d{1,4})";

        static readonly string LC_CENSUS_PATTERN_EW = @"(\d{1,5})\/(\d{1,3})\/(\d{1,3}).*?England & Wales (1841|1881)";
        static readonly string LC_CENSUS_PATTERN_1911_EW = @"(\d{1,5})\/(\d{1,3}).*?England & Wales 1911";
        static readonly string LC_CENSUS_PATTERN_SCOT = @"(\d{1,5}-?[AB12]?)\/(\d{1,3})\/(\d{1,3}).*?Scotland 1881";
        static readonly string LC_CENSUS_PATTERN_1940US = @"(T627[-_])(\d{1,5}-?[AB]?)\/(\d{1,2}[AB]?-\d{1,2}[AB]?)\/(\d{1,3}[AB]?).*?US 1880";
        static readonly string LC_CENSUS_PATTERN_1881CANADA = @"(\d{1,5})\/(\d{0,4}[A-Z]{0,4})\/(\d{0,3})\/(\d{1,3})\/?(\d{1,3})?.*?Canada 1881";

        static readonly Dictionary<string, Regex> censusRegexs;

        static CensusReference()
        {
            censusRegexs = new Dictionary<string, Regex>
            {
                ["EW_CENSUS_PATTERN"] = new Regex(EW_CENSUS_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN1"] = new Regex(EW_CENSUS_PATTERN1, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN2"] = new Regex(EW_CENSUS_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN3"] = new Regex(EW_CENSUS_PATTERN3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN4"] = new Regex(EW_CENSUS_PATTERN4, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN5"] = new Regex(EW_CENSUS_PATTERN5, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN6"] = new Regex(EW_CENSUS_PATTERN6, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN7"] = new Regex(EW_CENSUS_PATTERN7, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN8"] = new Regex(EW_CENSUS_PATTERN8, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN9"] = new Regex(EW_CENSUS_PATTERN9, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN10"] = new Regex(EW_CENSUS_PATTERN10, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN11"] = new Regex(EW_CENSUS_PATTERN11, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN12"] = new Regex(EW_CENSUS_PATTERN12, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN13"] = new Regex(EW_CENSUS_PATTERN13, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN14"] = new Regex(EW_CENSUS_PATTERN14, RegexOptions.Compiled | RegexOptions.IgnoreCase),

                ["EW_CENSUS_PATTERN_FH"] = new Regex(EW_CENSUS_PATTERN_FH, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN_FH2"] = new Regex(EW_CENSUS_PATTERN_FH2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN_FH3"] = new Regex(EW_CENSUS_PATTERN_FH3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_PATTERN_FS1"] = new Regex(EW_CENSUS_PATTERN_FS1, RegexOptions.Compiled | RegexOptions.IgnoreCase),

                ["EW_MISSINGCLASS_PATTERN"] = new Regex(EW_MISSINGCLASS_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_MISSINGCLASS_PATTERN2"] = new Regex(EW_MISSINGCLASS_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),

                ["EW_CENSUS_1841_51_PATTERN"] = new Regex(EW_CENSUS_1841_51_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN2"] = new Regex(EW_CENSUS_1841_51_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN2A"] = new Regex(EW_CENSUS_1841_51_PATTERN2A, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN3"] = new Regex(EW_CENSUS_1841_51_PATTERN3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN4"] = new Regex(EW_CENSUS_1841_51_PATTERN4, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN5"] = new Regex(EW_CENSUS_1841_51_PATTERN5, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN6"] = new Regex(EW_CENSUS_1841_51_PATTERN6, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN6A"] = new Regex(EW_CENSUS_1841_51_PATTERN6A, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN7"] = new Regex(EW_CENSUS_1841_51_PATTERN7, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN_FH"] = new Regex(EW_CENSUS_1841_51_PATTERN_FH, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN_FH2"] = new Regex(EW_CENSUS_1841_51_PATTERN_FH2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN_FH3"] = new Regex(EW_CENSUS_1841_51_PATTERN_FH3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1841_51_PATTERN_FH4"] = new Regex(EW_CENSUS_1841_51_PATTERN_FH4, RegexOptions.Compiled | RegexOptions.IgnoreCase),

                ["EW_CENSUS_1911_PATTERN"] = new Regex(EW_CENSUS_1911_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1911_PATTERN2"] = new Regex(EW_CENSUS_1911_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1911_PATTERN3"] = new Regex(EW_CENSUS_1911_PATTERN3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1911_PATTERN4"] = new Regex(EW_CENSUS_1911_PATTERN4, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1911_PATTERN5"] = new Regex(EW_CENSUS_1911_PATTERN5, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1911_PATTERN6"] = new Regex(EW_CENSUS_1911_PATTERN6, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1911_PATTERN78"] = new Regex(EW_CENSUS_1911_PATTERN78, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_CENSUS_1911_PATTERN78b"] = new Regex(EW_CENSUS_1911_PATTERN78b, RegexOptions.Compiled | RegexOptions.IgnoreCase),

                ["EW_1939_REGISTER_PATTERN1"] = new Regex(EW_1939_REGISTER_PATTERN1, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_1939_REGISTER_PATTERN2"] = new Regex(EW_1939_REGISTER_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["EW_1939_REGISTER_PATTERN3"] = new Regex(EW_1939_REGISTER_PATTERN3, RegexOptions.Compiled | RegexOptions.IgnoreCase),

                ["SCOT_CENSUSYEAR_PATTERN"] = new Regex(SCOT_CENSUSYEAR_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["SCOT_CENSUSYEAR_PATTERN2"] = new Regex(SCOT_CENSUSYEAR_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["SCOT_CENSUSYEAR_PATTERN3"] = new Regex(SCOT_CENSUSYEAR_PATTERN3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["SCOT_CENSUSYEAR_PATTERN4"] = new Regex(SCOT_CENSUSYEAR_PATTERN4, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["SCOT_CENSUS_PATTERN"] = new Regex(SCOT_CENSUS_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["SCOT_CENSUS_PATTERN2"] = new Regex(SCOT_CENSUS_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["SCOT_CENSUS_PATTERN3"] = new Regex(SCOT_CENSUS_PATTERN3, RegexOptions.Compiled | RegexOptions.IgnoreCase),

                ["US_CENSUS_PATTERN"] = new Regex(US_CENSUS_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["US_CENSUS_PATTERN1A"] = new Regex(US_CENSUS_PATTERN1A, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["US_CENSUS_PATTERN2"] = new Regex(US_CENSUS_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["US_CENSUS_PATTERN3"] = new Regex(US_CENSUS_PATTERN3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["US_CENSUS_PATTERN4"] = new Regex(US_CENSUS_PATTERN4, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["US_CENSUS_PATTERN5"] = new Regex(US_CENSUS_PATTERN5, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["US_CENSUS_1940_PATTERN"] = new Regex(US_CENSUS_1940_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["US_CENSUS_1940_PATTERN2"] = new Regex(US_CENSUS_1940_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["US_CENSUS_1940_PATTERN3"] = new Regex(US_CENSUS_1940_PATTERN3, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["US_CENSUS_1940_PATTERN4"] = new Regex(US_CENSUS_1940_PATTERN4, RegexOptions.Compiled | RegexOptions.IgnoreCase),

                ["CANADA_CENSUS_PATTERN"] = new Regex(CANADA_CENSUS_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["CANADA_CENSUS_PATTERN2"] = new Regex(CANADA_CENSUS_PATTERN2, RegexOptions.Compiled | RegexOptions.IgnoreCase),

                ["LC_CENSUS_PATTERN_EW"] = new Regex(LC_CENSUS_PATTERN_EW, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["LC_CENSUS_PATTERN_1911_EW"] = new Regex(LC_CENSUS_PATTERN_1911_EW, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["LC_CENSUS_PATTERN_SCOT"] = new Regex(LC_CENSUS_PATTERN_SCOT, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["LC_CENSUS_PATTERN_1940US"] = new Regex(LC_CENSUS_PATTERN_1940US, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["LC_CENSUS_PATTERN_1881CANADA"] = new Regex(LC_CENSUS_PATTERN_1881CANADA, RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
        }

        public enum ReferenceStatus { BLANK = 0, UNRECOGNISED = 1, INCOMPLETE = 2, GOOD = 3 };
        public static CensusReference UNKNOWN = new CensusReference();
        static readonly string MISSING = "Missing";

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
        string ReferenceText { get; set; }
        CensusLocation CensusLocation { get; set; }
        public Fact Fact { get; private set; }
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

        public CensusReference(Fact fact, XmlNode node, CensusReference pageRef = null)
            : this()
        {
            Fact = fact;
            if (GetCensusReference(node))
                SetCensusReferenceDetails();
            else
            {
                if (GetCensusReference(Fact.Comment))
                    SetCensusReferenceDetails();
            }
            if (fact.FactDate.IsKnown)
            {
                if (CensusYear.IsKnown && !fact.FactDate.Overlaps(CensusYear))
                    fact.SetError((int)FamilyTree.Dataerror.FACT_ERROR, Fact.FactError.WARNINGALLOW, $"Census Fact dated {fact.FactDate} doesn't match census reference {Reference} date of {CensusYear}");
                else
                    CensusYear = fact.FactDate;
            }
            else
                fact.UpdateFactDate(CensusYear);
            if (pageRef != null && !pageRef.IsKnownStatus && !IsKnownStatus)
                unknownCensusRef = $"{pageRef.unknownCensusRef}\n{unknownCensusRef}";
            fact.SetCensusReferenceDetails(this, CensusLocation, string.Empty);
        }

        public CensusReference(string individualID, string notes, bool source)
            : this()
        {
            Fact = new Fact(individualID, Fact.CENSUS_FTA, FactDate.UNKNOWN_DATE, FactLocation.UNKNOWN_LOCATION, string.Empty, false, true);
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

        void SetCensusReferenceDetails()
        {
            unknownCensusRef = string.Empty;
            if (Class.Equals("SCOT"))
            {
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
                CensusLocation = CensusLocation.GetCensusLocation(CensusYear.StartDate.Year.ToString(), Piece);
                URL = GetCensusURLFromReference();
            }
        }

        bool GetCensusReference(XmlNode n)
        {
            if (GeneralSettings.Default.SkipCensusReferences)
                return false;
            string text = FamilyTree.GetText(n, "PAGE", true);
            if (GetCensusReference(text, true))
                return true;
            text = FamilyTree.GetNotes(n);
            return GetCensusReference(text, false); // we have already checked sources so don't do it again.
        }

        bool GetCensusReference(string text, bool checksources = true, bool updateUnknownRef = true)
        {
            if (GeneralSettings.Default.SkipCensusReferences)
                return false;
            if (text.Length > 0)
            {
                if (CheckPatterns(text))
                {
                    ReferenceText = text.Trim();
                    return true;
                }
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
            if (checksources)
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

        public void CheckFullUnknownReference() => GetCensusReference(UnknownRef, false, false);

        string UnknownRef => unknownCensusRef.Length > 20 ? unknownCensusRef.Substring(20) : string.Empty;

        public static string ClearCommonPhrases(string input)
        {
            string output = input.Replace(".", " ").Replace(",", " ").Replace("(", " ")
                                 .Replace(")", " ").Replace("{", " ").Replace("}", " ")
                                 .Replace("«b»"," ").Replace("«i»"," ").Replace("«/b»", " ")
                                 .Replace("«/i»", " ").Replace(@"\i", " ").Replace(@"\i0", " ")
                                 .ClearWhiteSpace();
            return output.Replace("Registration District", "RD", StringComparison.OrdinalIgnoreCase)
                        .Replace("RegistrationDistrict", "RD", StringComparison.OrdinalIgnoreCase)
                        .Replace("Reg District", "RD", StringComparison.OrdinalIgnoreCase)
                        .Replace("Pg", "Page", StringComparison.OrdinalIgnoreCase)
                        .Replace("PN", "Piece", StringComparison.OrdinalIgnoreCase)
                        .Replace("Schedule No", "SN", StringComparison.OrdinalIgnoreCase)
                        .Replace("Schedule Number", "SN", StringComparison.OrdinalIgnoreCase)
                        .Replace("Schedule", "SN", StringComparison.OrdinalIgnoreCase)
                        .Replace("ED institution or vessel", "ED", StringComparison.OrdinalIgnoreCase)
                        .Replace("Enumeration District ED", "ED", StringComparison.OrdinalIgnoreCase)
                        .Replace("Enumeration District", "ED", StringComparison.OrdinalIgnoreCase)
                        .Replace("EnumerationDistrict", "ED", StringComparison.OrdinalIgnoreCase)
                        .Replace("Sub District", "SD", StringComparison.OrdinalIgnoreCase)
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

        bool CheckPatterns(string originalText)
        {
            string text = ClearCommonPhrases(originalText);
            if (text.Length == 0)
                return false;
            Match matcher = censusRegexs["EW_CENSUS_PATTERN"].Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN1"].Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN2"].Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN_FH"].Match(text);
            if (matcher.Success)
            {
                Class = $"RG{matcher.Groups[1]}";
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[4].ToString();
                Page = matcher.Groups[6].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN_FH2"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_PATTERN_FH3"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_PATTERN_FS1"].Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Page = matcher.Groups[2].ToString();
                Piece = matcher.Groups[3].ToString();
                Folio = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN"].Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN2"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN2A"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN3"].Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Book = matcher.Groups[3].ToString();
                Folio = matcher.Groups[4].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN4"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN5"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN6"].Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Folio = matcher.Groups[1].ToString();
                Book = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                Piece = matcher.Groups[5].ToString();
                ED = matcher.Groups[6].ToString();
                ReferenceStatus status = string.IsNullOrEmpty(Book) && string.IsNullOrEmpty(ED) ? ReferenceStatus.INCOMPLETE : ReferenceStatus.GOOD;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), status, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN6A"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN7"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN_FH"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN_FH2"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN_FH3"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_1841_51_PATTERN_FH4"].Match(text);
            if (matcher.Success)
            {
                Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1911_PATTERN"].Match(text);
            if (matcher.Success)
            {
                Class = "RG14";
                Piece = matcher.Groups[2].ToString();
                Schedule = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1911_PATTERN2"].Match(text);
            if (matcher.Success)
            {
                Class = "RG14";
                Piece = matcher.Groups[1].ToString();
                Schedule = matcher.Groups[2].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1911_PATTERN3"].Match(text);
            if (matcher.Success)
            {
                Class = "RG14";
                Piece = matcher.Groups[1].ToString();
                Schedule = matcher.Groups[2].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1911_PATTERN4"].Match(text);
            if (matcher.Success)
            {
                Class = "RG14";
                Piece = matcher.Groups[1].ToString();
                Schedule = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1911_PATTERN5"].Match(text);
            if (matcher.Success)
            {
                Class = "RG14";
                Piece = matcher.Groups[1].ToString();
                Page = matcher.Groups[2].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1911_PATTERN6"].Match(text);
            if (matcher.Success)
            {
                Class = "RG14";
                RD = matcher.Groups[1].ToString();
                ED = matcher.Groups[2].ToString();
                Schedule = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1911_PATTERN78"].Match(text);
            if (matcher.Success)
            {
                Class = "RG78";
                Piece = matcher.Groups[1].ToString();
                Schedule = matcher.Groups[2].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_1911_PATTERN78b"].Match(text);
            if (matcher.Success)
            {
                Class = "RG78";
                Piece = matcher.Groups[1].ToString();
                Schedule = "Missing";
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN3"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_PATTERN4"].Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN5"].Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN6"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_PATTERN7"].Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN8"].Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN9"].Match(text);
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
            matcher = censusRegexs["EW_CENSUS_PATTERN10"].Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN11"].Match(text);
            if (matcher.Success)
            {
                Class = GetUKCensusClass(matcher.Groups[1].ToString());
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = MISSING;
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN12"].Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN13"].Match(text);
            if (matcher.Success)
            {
                Class = matcher.Groups[1].ToString();
                Piece = matcher.Groups[2].ToString();
                Folio = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_CENSUS_PATTERN14"].Match(text);
            if (matcher.Success)
            {
                Folio = matcher.Groups[1].ToString();
                Page = matcher.Groups[2].ToString();
                Class = matcher.Groups[3].ToString().Replace("RG ","RG");
                Piece = matcher.Groups[4].ToString();
                SetFlagsandCountry(true, false, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_1939_REGISTER_PATTERN1"].Match(text);
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
            matcher = censusRegexs["EW_1939_REGISTER_PATTERN2"].Match(text);
            if (matcher.Success)
            {
                Class = "RG101";
                Piece = matcher.Groups[1].ToString();
                ED = matcher.Groups[2].ToString();
                Page = "Missing";
                Schedule = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_1939_REGISTER_PATTERN3"].Match(text);
            if (matcher.Success)
            {
                Class = "RG101";
                Piece = matcher.Groups[1].ToString();
                ED = "Missing";
                Page = "Missing";
                Schedule = "Missing";
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["SCOT_CENSUSYEAR_PATTERN"].Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = CensusDate.GetUKCensusDateFromYear(matcher.Groups[1].ToString());
                Parish = matcher.Groups[3].ToString();
                ED = matcher.Groups[4].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["SCOT_CENSUSYEAR_PATTERN2"].Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = CensusDate.GetUKCensusDateFromYear(matcher.Groups[1].ToString());
                Parish = matcher.Groups[3].ToString().Replace("/00", "").Replace("/", "-");
                ED = matcher.Groups[4].ToString().Replace("/00", "").TrimStart('0');
                Page = matcher.Groups[5].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["SCOT_CENSUSYEAR_PATTERN3"].Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = CensusDate.GetUKCensusDateFromYear(matcher.Groups[1].ToString());
                Parish = matcher.Groups[3].ToString().TrimStart('0');
                ED = matcher.Groups[4].ToString().Replace("/00", "").TrimStart('0');
                Page = matcher.Groups[5].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["SCOT_CENSUSYEAR_PATTERN4"].Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = CensusDate.GetUKCensusDateFromYear(matcher.Groups[1].ToString());
                Parish = matcher.Groups[2].ToString().TrimStart('0');
                ED = matcher.Groups[4].ToString().Replace("/00", "").TrimStart('0');
                Page = matcher.Groups[6].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["SCOT_CENSUS_PATTERN"].Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = FactDate.UNKNOWN_DATE;
                Parish = matcher.Groups[1].ToString().Trim();
                ED = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["SCOT_CENSUS_PATTERN2"].Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = FactDate.UNKNOWN_DATE;
                Parish = matcher.Groups[2].ToString().Replace("/00", "").Replace("/", "-").Replace("-0", "-");
                ED = matcher.Groups[3].ToString().Replace("/00", "").TrimStart('0');
                Page = matcher.Groups[4].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["SCOT_CENSUS_PATTERN3"].Match(text);
            if (matcher.Success)
            {
                Class = "SCOT";
                CensusYear = FactDate.UNKNOWN_DATE;
                Parish = matcher.Groups[2].ToString().TrimStart('0');
                ED = matcher.Groups[3].ToString().Replace("/00", "").TrimStart('0');
                Page = matcher.Groups[4].ToString().TrimStart('0');
                SetFlagsandCountry(true, false, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["US_CENSUS_PATTERN"].Match(text);
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
            matcher = censusRegexs["US_CENSUS_PATTERN1A"].Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "ROLL");
                Roll = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                ED = matcher.Groups[6].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                if (Roll.StartsWith("T627")) Roll = Roll.Substring(5);
                return true;
            }
            matcher = censusRegexs["US_CENSUS_PATTERN2"].Match(text);
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
            matcher = censusRegexs["US_CENSUS_PATTERN3"].Match(text);
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
            matcher = censusRegexs["US_CENSUS_PATTERN4"].Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                ED = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                Roll = matcher.Groups[7].ToString().TrimStart('0');
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["US_CENSUS_PATTERN5"].Match(text);
            if (matcher.Success)
            {
                Class = $"US{matcher.Groups[1]}";
                Place = GetOriginalPlace(matcher.Groups[2].ToString(), originalText, "ED");
                Page = matcher.Groups[5].ToString();
                ED = matcher.Groups[3].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["US_CENSUS_1940_PATTERN"].Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Roll = $"{matcher.Groups[4]}";
                ED = matcher.Groups[1].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["US_CENSUS_1940_PATTERN2"].Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Roll = $"{matcher.Groups[4]}";
                ED = matcher.Groups[1].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["US_CENSUS_1940_PATTERN3"].Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Place = GetOriginalPlace(matcher.Groups[1].ToString(), originalText, "T627");
                Roll = $"{matcher.Groups[3]}";
                ED = matcher.Groups[6].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["US_CENSUS_1940_PATTERN4"].Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Roll = $"{matcher.Groups[2]}";
                ED = matcher.Groups[3].ToString();
                Page = matcher.Groups[5].ToString();
                SetFlagsandCountry(false, false, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["CANADA_CENSUS_PATTERN"].Match(text);
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
            matcher = censusRegexs["CANADA_CENSUS_PATTERN2"].Match(text);
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
            matcher = censusRegexs["LC_CENSUS_PATTERN_EW"].Match(text);
            if (matcher.Success)
            {
                if (matcher.Groups[4].ToString().Equals("1881"))
                    Class = "RG11";
                else
                    Class = "HO107";
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, true, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["LC_CENSUS_PATTERN_1911_EW"].Match(text);
            if (matcher.Success)
            {
                Class = "RG14";
                Piece = matcher.Groups[1].ToString();
                Schedule = matcher.Groups[2].ToString();
                SetFlagsandCountry(true, true, GetCensusReferenceCountry(Class, Piece), ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["LC_CENSUS_PATTERN_SCOT"].Match(text);
            if (matcher.Success)
            {
                Class = "RG11";
                Parish = matcher.Groups[1].ToString();
                ED = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, true, Countries.SCOTLAND, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["LC_CENSUS_PATTERN_1940US"].Match(text);
            if (matcher.Success)
            {
                Class = "US1940";
                Roll = matcher.Groups[2].ToString();
                ED = matcher.Groups[3].ToString();
                Page = matcher.Groups[4].ToString();
                SetFlagsandCountry(false, true, Countries.UNITED_STATES, ReferenceStatus.GOOD, matcher.Value);
                return true;
            }
            matcher = censusRegexs["LC_CENSUS_PATTERN_1881CANADA"].Match(text);
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
            matcher = censusRegexs["EW_MISSINGCLASS_PATTERN"].Match(text);
            if (matcher.Success)
            {
                Piece = matcher.Groups[1].ToString();
                Folio = matcher.Groups[2].ToString();
                Page = matcher.Groups[3].ToString();
                SetFlagsandCountry(true, false, Countries.ENG_WALES, ReferenceStatus.INCOMPLETE, matcher.Value);
                return true;
            }
            matcher = censusRegexs["EW_MISSINGCLASS_PATTERN2"].Match(text);
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

        string CheckLetterCode(string letterCode)
        {
            if (letterCode.Equals("CODE"))
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
            if(country == Countries.UNITED_STATES) FixUS1940Prefix();
        }

        string GetOriginalPlace(string match, string originalText, string stopText)
        {
            int spacePos = match.IndexOf(" ", StringComparison.Ordinal);
            if (spacePos == -1)
                return match.ClearWhiteSpace();
            string startPlace = match.Substring(0, spacePos);
            int matchPos = originalText.ToUpper().IndexOf(startPlace.ToUpper(), StringComparison.Ordinal);
            int stopPos = originalText.ToUpper().IndexOf(stopText, StringComparison.Ordinal);
            if (matchPos > -1 && stopPos > -1 && stopPos - matchPos > 0)
                return originalText.Substring(matchPos, stopPos - matchPos).ClearWhiteSpace();
            return match.ClearWhiteSpace();
        }

        string GetUKCensusClass(string year)
        {
            if (year.Equals("1841") || year.Equals("1851"))
                return "HO107";
            if (year.Equals("1861"))
                return "RG9";
            if (year.Equals("1871"))
                return "RG10";
            if (year.Equals("1881"))
                return "RG11";
            if (year.Equals("1891"))
                return "RG12";
            if (year.Equals("1901"))
                return "RG13";
            if (year.Equals("1911"))
                return "RG14";
            return string.Empty;
        }

        FactDate GetCensusYearFromReference()
        {
            if (Class.Equals("SCOT"))
                return FactDate.UNKNOWN_DATE;
            if (Class.Equals("HO107"))
            {
                bool success = int.TryParse(Piece, out int piecenumber);
                if (success && piecenumber > 1465) // piece numbers go 1-1465 for 1841 and 1466+ for 1851.
                    return CensusDate.UKCENSUS1851;
                return CensusDate.UKCENSUS1841;
            }
            if (Class.Equals("RG9") || Class.Equals("RG09"))
                return CensusDate.UKCENSUS1861;
            if (Class.Equals("RG10"))
                return CensusDate.UKCENSUS1871;
            if (Class.Equals("RG11"))
                return CensusDate.UKCENSUS1881;
            if (Class.Equals("RG12"))
                return CensusDate.UKCENSUS1891;
            if (Class.Equals("RG13"))
                return CensusDate.UKCENSUS1901;
            if (Class.Equals("RG14") || Class.Equals("RG78"))
                return CensusDate.UKCENSUS1911;
            if (Class.Equals("RG101"))
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
                if (year.Equals("1911") && Countries.IsEnglandWales(Country) && Piece.Length > 0 && Schedule.Length > 0)
                    return @"http://search.findmypast.co.uk/results/world-records/1911-census-for-england-and-wales?pieceno=" + Piece + @"&schedule=" + Schedule;
                if (year.Equals("1939") && Countries.IsEnglandWales(Country) && Piece.Length > 0 && !ED.Equals("UNKNOWN"))
                {
                    string dir = Piece.Length > 1 ? Piece.Substring(0, Piece.Length - 1) : Piece; //strip last letter from piece
                    return @"https://search.findmypast.co.uk/record?id=tna%2fr39%2f" + dir + "%2f" + Piece.ToLower() + "%2f" + Page + "%2f" + Schedule;
                }
                if (Countries.IsUnitedKingdom(Country))
                {
                    string querystring = string.Empty;
                    if (!Country.Equals(Countries.SCOTLAND))
                    {
                        if (Piece.Length > 0 && !Piece.Equals(MISSING))
                            querystring = @"pieceno=" + Piece;
                        if (Folio.Length > 0 && !Folio.Equals(MISSING))
                        {
                            string lastChar = Folio.Substring(Folio.Length).ToUpper();
                            if (!lastChar.Equals("F") && !lastChar.Equals("R") && !lastChar.Equals("O"))
                                querystring = querystring + @"&folio=" + Folio;
                        }
                        if (Page.Length > 0 && !Page.Equals(MISSING))
                            querystring = querystring + @"&page=" + Page;
                    }
                    if (year.Equals("1841") && Book.Length > 0 && !Book.Equals(MISSING))
                        return @"http://search.findmypast.co.uk/results/world-records/1841-england-wales-and-scotland-census?" + querystring + @"&book=" + Book;
                    if (querystring.Length > 0)
                        return @"http://search.findmypast.co.uk/results/world-records/" + year + "-england-wales-and-scotland-census?" + querystring;
                }
            }
            return string.Empty;
        }

        string GetCensusReferenceCountry(string censusClass, string censusPiece)
        {
            bool success = int.TryParse(censusPiece, out int piece);
            if (success && censusClass.Length > 0 && censusPiece.Length > 0 && piece > 0)
            {
                if (censusClass.Equals("HO107")) //1841 & 1851
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
                    if (piece <= 1522)
                        return Countries.WALES;
                    if (piece <= 1526)
                        return Countries.ISLE_OF_MAN;
                    if (piece <= 2531)
                        return Countries.CHANNEL_ISLANDS;
                }
                else if (censusClass.Equals("RG9") || censusClass.Equals("RG09")) //1861
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
                else if (censusClass.Equals("RG10")) //1871
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
                else if (censusClass.Equals("RG11")) //1881
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
                else if (censusClass.Equals("RG12")) // 1891
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
                else if (censusClass.Equals("RG13")) //1901
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
                else if (censusClass.Equals("RG14")) //1911
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

        public string Reference
        {
            get
            {
                if (Family.Length > 0)
                {
                    if (Roll.Length > 0)
                    {
                        return GeneralSettings.Default.UseCompactCensusRef ? $"{Roll}/{Page}/{Family}" : $"Roll: {Roll}, Page: {Page}, Family: {Family}";
                    }
                    return GeneralSettings.Default.UseCompactCensusRef
                        ? $"{ED}/{SD}/{Page}/{Family}"
                        : $"District: {ED}, Sub-District: {SD}, Page: {Page}, Family: {Family}";
                }
                if (Roll.Length > 0)
                {
                    return GeneralSettings.Default.UseCompactCensusRef
                        ? $"{Roll}{(ED.Length > 0 ? $"/{ED}" : "")}/{Page}"
                        : $"Roll: {Roll}{(ED.Length > 0 ? $", ED: {ED}" : "")}, Page: {Page}";
                }
                if (Piece.Length > 0)
                {
                    if (Countries.IsEnglandWales(Fact.Location.Country) || Fact.IsOverseasUKCensus(Fact.Location.Country))
                    {
                        if (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1851) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1861) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1871) ||
                            Fact.FactDate.Overlaps(CensusDate.UKCENSUS1881) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1891) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1901))
                            return GeneralSettings.Default.UseCompactCensusRef
                                ? $"{Piece}/{Folio}/{Page}"
                                : $"Piece: {Piece}, Folio: {Folio}, Page: {Page}";
                        if (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1841))
                        {
                            if (Book.Length > 0)
                                return GeneralSettings.Default.UseCompactCensusRef
                                    ? $"{Piece}/{Book}/{Folio}/{Page}"
                                    : $"Piece: {Piece}, Book: {Book}, Folio: {Folio}, Page: {Page}";
                            if (GeneralSettings.Default.UseCompactCensusRef)
                                return $"{Piece}/see image/{Folio}/{Page}";
                            return $"Piece: {Piece}, Book: see census image (stamped on the census page after the piece number), Folio: {Folio}, Page: {Page}";
                        }
                        if (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1911))
                        {
                            if (Schedule.Length > 0)
                                return GeneralSettings.Default.UseCompactCensusRef ? $"{Piece}/{Schedule}" : $"Piece: {Piece}, Schedule: {Schedule}";
                            return GeneralSettings.Default.UseCompactCensusRef ? $"{Piece}/{Page}" : $"Piece: {Piece}, Page: {Page}";
                        }
                        if (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1939))
                        {
                            return GeneralSettings.Default.UseCompactCensusRef
                                ? $"RG101/{Piece}/{Page}/{Schedule} ({ED})"
                                : $"Piece: {Piece}, Page: {Page}, Schedule {Schedule}, ED: {ED}";
                        }
                    }
                }
                else if (Parish.Length > 0)
                {
                    if (Fact.Location.Country.Equals(Countries.SCOTLAND) && (Fact.FactDate.Overlaps(CensusDate.UKCENSUS1841) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1851) ||
                        Fact.FactDate.Overlaps(CensusDate.UKCENSUS1861) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1871) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1881) ||
                        Fact.FactDate.Overlaps(CensusDate.UKCENSUS1891) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1901) || Fact.FactDate.Overlaps(CensusDate.UKCENSUS1911)))
                    {
                        ScottishParish sp = ScottishParish.FindParishFromID(Parish);
                        if (GeneralSettings.Default.UseCompactCensusRef)
                            return sp == ScottishParish.UNKNOWN_PARISH ? $"{Parish}/{ED}/{Page}" : $"{sp.Reference}/{ED}/{Page}";
                        return sp == ScottishParish.UNKNOWN_PARISH
                            ? $"Parish: {Parish}, ED: {ED}, Page: {Page}"
                            : $"Parish: {sp.Reference}, ED: {ED}, Page: {Page}";
                    }
                }
                else if (RD.Length > 0)
                {
                    if (Fact.Location.IsEnglandWales && Fact.FactDate.Overlaps(CensusDate.UKCENSUS1911))
                        return GeneralSettings.Default.UseCompactCensusRef
                            ? $"{RD}/{ED}/{Schedule}"
                            : $"RD: {RD}, ED: {ED}, Schedule: {Schedule}";
                }
                if (unknownCensusRef.Length > 0)
                    return unknownCensusRef;
                //if (ReferenceText.Length > 0)
                //  log.Warn("Census reference text not generated for :" + ReferenceText);
                return string.Empty;
            }
        }
        
        void FixUS1940Prefix()
        {
            Roll = Roll.ToUpper().Replace('-', '_');
            if (Roll.StartsWith("T627_")) Roll = Roll.Substring(5);
            else if (Roll.StartsWith("T0627_")) Roll = Roll.Substring(6);
            else if (Roll.StartsWith("M_T627_")) Roll = Roll.Substring(7);
            else if (Roll.StartsWith("M_T0627_")) Roll = Roll.Substring(8);
        }

        static readonly Regex LCEDregex = new Regex(@"\d{1,3}[A-Z]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool IsValidLostCousinsReference()
        {
            if (Status != ReferenceStatus.GOOD)
                return false;
            //use Peter's code to check all the entries are valid
            if (CensusYear.Overlaps(CensusDate.EWCENSUS1841) && Countries.IsEnglandWales(Country))
            {
                if (Piece.StartsWith("HO107"))
                    Piece = Piece.Substring(5);
                Piece = Piece.TrimStart('0');
                Book = Book.TrimStart('0');
                Folio = Folio.TrimStart('0').ToUpper().TrimEnd('A');
                if (!Piece.IsNumeric() || !Folio.IsNumeric() || !Page.IsNumeric() || Book.Length == 0) return false;
            }
            else if (CensusYear.Overlaps(CensusDate.EWCENSUS1881) && Countries.IsEnglandWales(Country))
            {
                if (Piece.StartsWith("RG78"))
                    return false;
                if (Piece.StartsWith("RG11"))
                    Piece = Piece.Substring(4);
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
                Match match = LCEDregex.Match(ED); //also check d{1,3}[A-Z]? format
                if (!match.Success) return false; // check last to only do regex calc if everything else is ok
            }
            else if (CensusYear.Overlaps(CensusDate.CANADACENSUS1881) && Country == Countries.CANADA)
            {
                if (Roll.ToUpper().StartsWith("C_"))
                    Roll = Roll.Substring(2);


            }
            else if (CensusYear.Overlaps(CensusDate.IRELANDCENSUS1911) && Country == Countries.IRELAND)
            {
            }
            else if (CensusYear.Overlaps(CensusDate.EWCENSUS1911) && Countries.IsEnglandWales(Country))
            {
                if (Piece.StartsWith("RG14"))
                    Piece = Piece.Substring(4);
                if (Piece.StartsWith("PN"))
                    Piece = Piece.Substring(2);
                Piece = Piece.TrimStart('0');
                Schedule = Schedule.TrimStart('0');
                if (Schedule.Length == 0 && Page.Length > 0)
                    Schedule = "9999";
                if (!Piece.IsNumeric()) return false;
            }
            else if (CensusYear.Overlaps(CensusDate.USCENSUS1880) && Country == Countries.UNITED_STATES)
            {
                if (Roll.ToUpper().StartsWith("T9"))
                    Roll = Roll.Substring(2);
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
                string prefix = page.Substring(0, page.Length - 2);
                if (Page.EndsWith(".1")) return prefix + "A";
                if (Page.EndsWith(".2")) return prefix + "B";
                if (Page.EndsWith(".3")) return prefix + "C";
                if (Page.EndsWith(".4")) return prefix + "D";
                if (Page.EndsWith(".5")) return prefix + "E";
                if (Page.EndsWith(".6")) return prefix + "F";
                if (Page.EndsWith(".7")) return prefix + "G";
                if (Page.EndsWith(".8")) return prefix + "H";
                if (Page.EndsWith(".9")) return prefix + "I";
            }
            return page;
        }

        public override string ToString() => Reference.Trim();

        public int CompareTo(CensusReference that) => string.Compare(Reference, that.Reference, StringComparison.Ordinal);
    }
}
