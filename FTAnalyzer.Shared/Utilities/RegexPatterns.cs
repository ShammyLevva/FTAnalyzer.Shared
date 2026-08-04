using System.Text.RegularExpressions;

namespace FTAnalyzer.Utilities
{
    internal static partial class RegexPatterns
    {
        const string CHILDREN_STATUS_PATTERN1 = @"(\d{1,2}) Total ?,? ?(\d{1,2}) (Alive|Living) ?,? ?(\d{1,2}) Dead";
        const string CHILDREN_STATUS_PATTERN2 = @"Total:? (\d{1,2}) ?,? ?(Alive|Living):? (\d{1,2}) ?,? ?Dead:? (\d{1,2})";
        const string AGE_PATTERN = @"^(?<year>\d{1,3}y)? ?(?<month>\d{1,2}m)? ?(?<day>\d{1,2}d)?$";

        // Census reference patterns moved from CensusReference
        const string EW_CENSUS_PATTERN = @"RG *(\d{1,3}) *\/?Piece *(\d{1,5}) *\/?Folio *(\d{1,4}[a-z]?).*?\/?Page *(\d{1,3})";
        const string EW_CENSUS_PATTERN1 = @"RG *(\d{1,3}) *Piece\/Folio *(\d{1,5})[\/ ]*(\d{1,4}[a-z]?).*?Page *(\d{1,3})";
        const string EW_CENSUS_PATTERN2 = @"RG *(\d{1,3}) *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        const string EW_CENSUS_PATTERN3 = @"(\d{4}) .*?Census.*? *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?).*?Page *(\d{1,3})";
        const string EW_CENSUS_PATTERN4 = @"(\d{4}) .*?Census.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?).*?Page *(\d{1,3})";
        const string EW_CENSUS_PATTERN5 = @"(\d{4}) .*?Census.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        const string EW_CENSUS_PATTERN6 = @"Census *(\d{4}).*? *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?).*?Page *(\d{1,3})";
        const string EW_CENSUS_PATTERN7 = @"Census *(\d{4}).*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?).*?Page *(\d{1,3})";
        const string EW_CENSUS_PATTERN8 = @"Census *(\d{4}).*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        const string EW_CENSUS_PATTERN9 = @"(\d{4}) *- *.*? *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?).*?Page *(\d{1,3})";
        const string EW_CENSUS_PATTERN10 = @"(\d{4}) *- *.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?).*?Page *(\d{1,3})";
        const string EW_CENSUS_PATTERN11 = @"(\d{4}) *- *.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        const string EW_CENSUS_PATTERN12 = @"Piece (RG\d{1,2})\/(\d{1,5}).*? *Folio *(\d{1,4}[a-z]?).*?Page *(\d{1,3})";
        const string EW_CENSUS_PATTERN13 = @"(RG *\d{1,2})-(\d{1,5})-(\d{1,4}[a-z]?)-(\d{1,3})";
        const string EW_CENSUS_PATTERN14 = @"Folio *(\d{1,4}[a-z]?).*?Page *(\d{1,3}).*?(RG *\d{1,2})\/(\d{1,5})";
        const string EW_CENSUS_PATTERN15 = @"(RG *\d{1,2})\/(\d{1,5})\/(\d{1,4}[a-z]?)\/(\d{1,3})";
        const string EW_CENSUS_PATTERN16 = @"(RG *\d{1,2}) +(\d{1,5}) +(\d{1,4}[a-z]?) +(\d{1,3})";

        const string EW_CENSUS_PATTERN_FH = @"RG *(\d{1,2})\/(\d{1,5}) F(olio)? ?(\d{1,4}[a-z]?) P(age)? ?(\d{1,3})";
        const string EW_CENSUS_PATTERN_FH2 = @"RG *(\d{1,2})\/(\d{1,5}) ED *(\d{1,4}[a-z]?) F(olio)? ?(\d{1,4}[a-z]?) P(age)? ?(\d{1,3})";
        const string EW_CENSUS_PATTERN_FH3 = @"RG *(\d{1,2}) *Piece *(\d{1,5}) ED *(\d{1,4}[a-z]?) F(olio)? ?(\d{1,4}[a-z]?) P(age)? ?(\d{1,3})";

        const string EW_CENSUS_PATTERN_FS1 = @"(\d{4}).*?Census.*?p *(\d{1,3}) *Piece\/Folio *(\d{1,4}[a-z]?)\/(\d{1,3})";

        const string EW_CENSUS_PATTERN_SN = @"RG *(\d{1,3}) *\/?Piece *(\d{1,5}) *\/?Folio *(\d{1,4}[a-z]?).*?\/?SN *(\d{1,4})";
        const string EW_CENSUS_PATTERN4_SN = @"(\d{4}) .*?Census.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?).*?SN *(\d{1,4})";
        const string EW_CENSUS_PATTERN7_SN = @"Census *(\d{4}).*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?).*?SN *(\d{1,4})";
        const string EW_CENSUS_PATTERN10_SN = @"(\d{4}) *- *.*? *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?).*?SN *(\d{1,4})";
        const string EW_CENSUS_PATTERN15_SN = @"(RG *\d{1,2})\/(\d{1,5})\/(\d{1,4}[a-z]?).*?SN *(\d{1,4})";
        const string EW_CENSUS_1841_51_PATTERN_SN = @"HO *107\/(\d{1,5})\/(\d{1,3}).*?SN *(\d{1,4})";

        const string EW_MISSINGCLASS_PATTERN = @"Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        const string EW_MISSINGCLASS_PATTERN2 = @"Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?)";
        const string EW_MISSINGCLASS_PATTERN_SN = @"Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?) *SN *(\d{1,4})";

        const string EW_CENSUS_1841_51_PATTERN = @"HO *107 *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN2 = @"HO *107 *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN2A = @"HO *107 *Piece *(\d{1,5}) *Folio *(\d{1,4}[a-z]?).*?Book *(\d{1,3}) *Page *(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN3 = @"HO *107 *Piece *(\d{1,5}).*Folio *(\d{1,4}[a-z]?)?\/?(\d{1,4}[a-z]?) *Page *(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN4 = @"HO *107 *Piece *(\d{1,5}) *Book *(\d{1,3}).*?Page *(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN5 = @"HO *107 *Piece *(\d{1,5}).*?Page *(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN6 = @"Folio *(\d{1,4}[a-z]?)\/? *(Book)? *(\d{1,4}[a-z]?)? *P(age) *(\d{1,3}).*?HO[ \/]?107\/(\d{1,5})\/?(\d{1,3})?";
        const string EW_CENSUS_1841_51_PATTERN6A = @"Book *(\d{1,3}).*?Folio *(\d{1,4}[a-z]?)\/?(\d{1,2})? *Page *(\d{1,3}).*?HO[ \/]?107\/(\d{1,5})";
        const string EW_CENSUS_1841_51_PATTERN7 = @"HO *107-(\d{1,5})-(\d{1,3})-(\d{1,4}[a-z]?)-(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN8 = @"HO *107\/(\d{1,5})\/(\d{1,3})\/(\d{1,4}[a-z]?)\/(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN_FH = @"HO *107\/(\d{1,5})\/(\d{1,3}) .*?F(olio)? *(\d{1,4}[a-z]?) P(age)? *(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN_FH2 = @"HO *107\/(\d{1,5}) ED *(\d{1,4}[a-z]?) F(olio)? *(\d{1,4}[a-z]?) P(age)? *(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN_FH3 = @"HO *107\/(\d{1,5}) .*?F(olio)? *(\d{1,4}[a-z]?)\/(\d{1,4}) P(age)? *(\d{1,3})";
        const string EW_CENSUS_1841_51_PATTERN_FH4 = @"HO *107\/(\d{1,5}) .*?F(olio)? *(\d{1,4}[a-z]?) P(age)? *(\d{1,3})";

        const string EW_CENSUS_1911_1921_PATTERN = @"(RG *1[4-5])\/?( *Piece *)?(\d{1,6}) .*?SN *(\d{1,4})";
        const string EW_CENSUS_1911_1921_PATTERN2 = @"(19[1-2]1) Census.*? *Piece *(\d{1,6}) *SN *(\d{1,4})";
        const string EW_CENSUS_1911_1921_PATTERN3 = @"Census *(19[1-2]1).*? *Piece *(\d{1,6}) *SN *(\d{1,4})";
        const string EW_CENSUS_1911_1921_PATTERN4 = @"(RG *1[4-5])\/? *Piece *(\d{1,6})$";
        const string EW_CENSUS_1911_1921_PATTERN5 = @"(RG *1[4-5])\/? *Piece *(\d{1,6}) *Page *(\d{1,3})";
        const string EW_CENSUS_1911_1921_PATTERN6 = @"(RG *1[4-5])\/? *RD *(\d{1,4}) *ED *(\d{1,3}) (\d{1,5})";
        const string EW_CENSUS_1911_PATTERN1 = @"RG *78\/? *Piece *(\d{1,6}) .*?SN *(\d{1,4})";
        const string EW_CENSUS_1911_PATTERN2 = @"RG *78\/? *Piece *(\d{1,5})";
        const string EW_CENSUS_1921_PATTERN1 = @"RG *15\/? *(Piece)? *(\d{1,6}) *ED (\d{1,4}) *SN *(\d{1,4}) *Book *(\d{1,6})";
        const string EW_CENSUS_1921_PATTERN2 = @"RG *15\/? *(Piece)? *(\d{1,6}) *ED (\d{1,4}) *SN *(\d{1,4})";
        const string EW_CENSUS_1921_PATTERN3 = @"RG *15\/? *(Piece)? *(\d{1,6}) *ED (\d{1,4})$";
        const string EW_CENSUS_1921_PATTERN4 = @"RG *15\/? *(Piece)? *(\d{1,6}) *ED (\d{1,4}) *Page *(\d{1,3})";

        const string EW_1939_REGISTER_PATTERN1 = @"RG *101\/?\\? *(\d{1,6}[A-Z]?) *.\/?\\? *(\d{1,3}) *.\/?\\? *(\d{1,3}).+(\b[A-Z]{4}\b)";
        const string EW_1939_REGISTER_PATTERN1A = @"RG *101\/?\\? *(\d{1,6}[A-Z]?) *.\/?\\? *(\d{1,3}) *.\/?\\? *(\d{1,3})";
        const string EW_1939_REGISTER_PATTERN2 = @"RG *101\/?\\? *(\d{1,6}[A-Z]?).*? ED ([A-Z]{4}) RD (.*?) Marital";
        const string EW_1939_REGISTER_PATTERN3 = @"RG *101\/?\\? *(\d{1,6}[A-Z]?)";

        const string SCOT_CENSUSYEAR_PATTERN = @"(1[89]\d[15]).{1,10}(\(?GROS *\)?)?Parish *([A-Z .'-]+) *ED *(\d{1,3}[AB]?) *Page *(\d{1,4}) *Line *(\d{1,2})";
        const string SCOT_CENSUSYEAR_PATTERN2 = @"(1[89]\d[15]).{1,10}(\(?GROS *\)?)?(\d{1,3}\/\d{1,2}[AB]?) (\d{3}\/\d{2}) (\d{3,4})";
        const string SCOT_CENSUSYEAR_PATTERN3 = @"(1[89]\d[15]).{1,10}(\(?GROS *\)?)?(\d{1,3}[AB]?)\/(\d{2}[AB]?) Page *(\d{1,4})";
        const string SCOT_CENSUSYEAR_PATTERN4 = @"SCT(1[89]\d[15])\/?(\d{1,3}[AB]?) *f(olio)? *(\d{1,3}[AB]?) *p(age)? *(\d{1,4})";
        const string SCOT_CENSUS_PATTERN = @"Parish *([A-Z .'-]+) *ED *(\d{1,3}[AB]?) *Page *(\d{1,4}) *Line *(\d{1,2})";
        const string SCOT_CENSUS_PATTERN2 = @"(\(?GROS *\)?) *(\d{1,3}\/\d{1,2}[AB]?) (\d{3}\/\d{2}) (\d{3,4})";
        const string SCOT_CENSUS_PATTERN3 = @"(\(?GROS *\)?) *(\d{1,3}[AB]?-?\d?)\/(\d{2}[AB]?) Page *(\d{1,4})";
        const string SCOT_CENSUS_PATTERN4 = @"(1[89]\d[15])? *(\(?GROS *\)?) *(\d{1,3}[AB]?[-\/]?\d? ) *\/? *(\d{1,2}[AB]?) *\/ *(\d{1,4})";
        const string SCOT_CENSUS_PATTERN5 = @"(1[89]\d1)? *(\(?CENSUS *\)?) *(\d{1,3}[AB]?[-\/]?\d? ) *\/? *(\d{1,2}[AB]?) *\/ *(\d{1,4})";

        const string US_CENSUS_PATTERN = @"Year *(\d{4}) *Census *(.*?) *Roll *(.*?) *Film (.*?) *P(age)? *(\d{1,4}[ABCD]?) *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?)";
        const string US_CENSUS_PATTERN1A = @"Year *(\d{4}) *Census *(.*?),? *Roll *(.*?),? *P(age)? *(\d{1,4}[ABCD]?),? *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?)";
        const string US_CENSUS_PATTERN2 = @"Census,? *(\d{4}) *(.*?) *Roll *(.*?),? *P(age)? *(\d{1,4}[ABCD]?),? *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?)";
        const string US_CENSUS_PATTERN3 = @"Census,? *(\d{4}) *(.*?) *Ward *(.*?),? *ED *(\d{1,5}[ABCD]?-?\d{0,4}[AB]?),? *P(age)? *(\d{1,4}[AB]?)";
        const string US_CENSUS_PATTERN3A = @"Census,? *(\d{4}) *(.*?) *Ward *(.*?),? *P(age)? *(\d{1,4}[AB]?) *ED *(\d{1,5}[ABCD]?-?\d{0,4}[AB]?),?";
        const string US_CENSUS_PATTERN3B = @"Year *(\d{4}) *Census *(.*?),? *Ward *(.*?),? *P(age)? *(\d{1,4}[AB]?) *ED *(\d{1,5}[ABCD]?-?\d{0,4}[AB]?),?";
        const string US_CENSUS_PATTERN3C = @"Year *(\d{4}) *Census *(.*?),? *P(age)? *(\d{1,4}[ABCD]?),? *ED *(\d{1,5}[ABCD]?-?\d{0,4}[AB]?)";
        const string US_CENSUS_PATTERN4 = @"Census,? *(\d{4}) *(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[ABCD]?),? *P(age)? *(\d{1,4}[AB]?)(.*?)roll *(\d{1,4})";
        const string US_CENSUS_PATTERN5 = @"Census,? *(\d{4}) *(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[ABCD]?),? *P(age)? *(\d{1,4}[AB]?)";
        const string US_CENSUS_PATTERN6 = @"(\d{4}) U ?S ? Census,?( ?population SN ?)?(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[ABCD]?),? *P(age)? *(\d{1,4}[AB]?)(.*?)[A-Z]6\d\d roll (\d{1,4})";
        const string US_CENSUS_PATTERN7 = @"(\d{4}) U ?S ? Census,?( ?population SN ?)?(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[ABCD]?),? *P(age)? *(\d{1,4}[AB]?)(.*?)";
        const string US_CENSUS_PATTERN8 = @"(\d{4}) U ?S ? Census,?( ?population SN ?)?(.*?) *P(age)? *(\d{1,4}[AB]?)(.*?)[A-Z]6\d\d roll (\d{1,4})";
        const string US_CENSUS_PATTERN9 = @"(\d{4}) ?T0?(62\d).*ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?) Roll ?(\d{0,5}) *P(age)?  *(\d{1,4}[ABCD]?)?";
        const string US_CENSUS_1940_PATTERN = @"District *(\d{1,5}[AB]?-?\d{0,4}[AB]?).*?P(age)? *(\d{1,3}[ABCD]?).*?T627 ?,? *(\d{1,5}-?[AB]?)";
        const string US_CENSUS_1940_PATTERN2 = @"ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?).*? *P(age)? *(\d{1,3}[ABCD]?).*?T627.*?roll ?(\d{1,5}-?[AB]?)";
        const string US_CENSUS_1940_PATTERN3 = @"1940 *(.*?)(Roll)? *M*?-*?_*?T0*?627_(.*?) *P(age)? *(\d{1,4}[ABCD]?) *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?)";
        const string US_CENSUS_1940_PATTERN4 = @"Roll( *M?[-_]?T0?627[-_]?(\d{0,5})) ? *(\d{0,5})(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?) *P(age)? *(\d{1,4}[ABCD]?)";
        const string US_CENSUS_T62X_PATTERN1 = @"( *M?[-_]?T0?(62\d)) Roll ?(\d{0,5}) ? *(\d{0,5})(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?) *P(age)? *(\d{1,4}[ABCD]?)";
        const string US_CENSUS_TX_PATTERN1 = @"( *M?[-_]?T9) Roll ?(\d{0,5}) ? *(\d{0,5})(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?) *P(age)? *(\d{1,4}[ABCD]?)";
        const string US_CENSUS_MX_PATTERN1 = @"(M\d{2,3}) Roll ?(\d{0,5}) ? *(\d{0,5})(.*?) *ED *(\d{1,5}[AB]?-?\d{0,4}[AB]?) *P(age)? *(\d{1,4}[ABCD]?)";

        const string CANADA_CENSUS_PATTERN = @"Year *(\d{4}) *Census *(.*?) *Roll *(.*?) *P(age)? *(\d{1,4}[ABCD]?) *Family *(\d{1,4})";
        const string CANADA_CENSUS_PATTERN1 = @"Year *(\d{4}) *Census *(.*?) *P(age)? *(\d{1,4}[ABCD]?) *Family *(\d{1,4})";
        const string CANADA_CENSUS_PATTERN2 = @"(\d{4}).*?Census[ -]*District *(\d{1,5})[\/-] ?(\d{0,4}[A-Z]{0,4}) *P(age)? *(\d{1,4}[ABCD]?) *Family *(\d{1,4})";
        const string CANADA_CENSUS_PATTERN3 = @"(\d{4}).*?Census[ -]*(RG\d{2}) *District *(\d{1,5}) *SD *(\d{0,2}[A-Z]{0,4}) *Family *(\d{1,4}) *P(age)? *(\d{1,4}[ABCD]?)";
        const string CANADA_CENSUS_PATTERN4 = @"(\d{4}).*?RG31 .*?Item ?(\d{7}) (\d{1,3})[\/-] ?(\d{1,4})[\/-] ?(\d{1,4})[\/-] ?(\d{1,4})";
        const string CANADA_CENSUS_PATTERN5 = @"(\d{4}).*?RG31 .*?Item ?(\d{7}) (\d{1,3})[\/-] ?(\d{1,4})[\/-] ?(\d{1,4})";
        const string CANADA_CENSUS_PATTERN6 = @"(\d{4}).*?RG31[\/-] ?(\d{1,3})[\/-] ?(\d{1,4})[\/-] ?(\d{1,4})[\/-] ?(\d{1,4})";
        const string CANADA_CENSUS_PATTERN7 = @"(\d{4}).*?RG31[\/-] ?(\d{1,3})[\/-] ?(\d{1,4})[\/-] ?(\d{1,4})";

        const string LC_CENSUS_PATTERN_EW = @"(\d{1,5})\/(\d{1,3})\/(\d{1,3}).*?England & Wales (1841|1881)";
        const string LC_CENSUS_PATTERN_1911_EW = @"(\d{1,5})\/(\d{1,3}).*?England & Wales 1911";
        const string LC_CENSUS_PATTERN_SCOT = @"(\d{1,5}-?[AB12]?)\/(\d{1,3})\/(\d{1,3}).*?Scotland 1881";
        const string LC_CENSUS_PATTERN_1940US = @"(\d{1,5}-?[AB]?)\/(\d{1,2}[AB]?-\d{1,2}[AB]?)\/(\d{1,3}[AB]?).*?US 1940";
        const string LC_CENSUS_PATTERN_1881CANADA = @"(\d{1,5})\/(\d{0,4}[A-Z]{0,4})\/(\d{0,3})\/(\d{1,3})\/?(\d{1,3})?.*?Canada 1881";
        const string LC_ED_PATTERN = @"\d{1,3}[A-Z]?";

        const string PEOPLEFINDERS = @"Full Background Report";
        const string HAS_NUMBERS = @"\d";

        [GeneratedRegex(CHILDREN_STATUS_PATTERN1)]
        internal static partial Regex ChildrenStatusPattern1();

        [GeneratedRegex(CHILDREN_STATUS_PATTERN2)]
        internal static partial Regex ChildrenStatusPattern2();

        [GeneratedRegex(AGE_PATTERN)]
        internal static partial Regex AgeRegex();

        [GeneratedRegex(EW_CENSUS_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern();

        [GeneratedRegex(EW_CENSUS_PATTERN1, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern1();

        [GeneratedRegex(EW_CENSUS_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern2();

        [GeneratedRegex(EW_CENSUS_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern3();

        [GeneratedRegex(EW_CENSUS_PATTERN4, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern4();

        [GeneratedRegex(EW_CENSUS_PATTERN5, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern5();

        [GeneratedRegex(EW_CENSUS_PATTERN6, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern6();

        [GeneratedRegex(EW_CENSUS_PATTERN7, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern7();

        [GeneratedRegex(EW_CENSUS_PATTERN8, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern8();

        [GeneratedRegex(EW_CENSUS_PATTERN9, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern9();

        [GeneratedRegex(EW_CENSUS_PATTERN10, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern10();

        [GeneratedRegex(EW_CENSUS_PATTERN11, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern11();

        [GeneratedRegex(EW_CENSUS_PATTERN12, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern12();

        [GeneratedRegex(EW_CENSUS_PATTERN13, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern13();

        [GeneratedRegex(EW_CENSUS_PATTERN14, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern14();

        [GeneratedRegex(EW_CENSUS_PATTERN15, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern15();

        [GeneratedRegex(EW_CENSUS_PATTERN16, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern16();

        [GeneratedRegex(EW_CENSUS_PATTERN_FH, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPatternFh();

        [GeneratedRegex(EW_CENSUS_PATTERN_FH2, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPatternFh2();

        [GeneratedRegex(EW_CENSUS_PATTERN_FH3, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPatternFh3();

        [GeneratedRegex(EW_CENSUS_PATTERN_FS1, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPatternFs1();

        [GeneratedRegex(EW_CENSUS_PATTERN_SN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPatternSn();

        [GeneratedRegex(EW_CENSUS_PATTERN4_SN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern4Sn();

        [GeneratedRegex(EW_CENSUS_PATTERN7_SN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern7Sn();

        [GeneratedRegex(EW_CENSUS_PATTERN10_SN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern10Sn();

        [GeneratedRegex(EW_CENSUS_PATTERN15_SN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensusPattern15Sn();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN_SN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151PatternSn();

        [GeneratedRegex(EW_MISSINGCLASS_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwMissingclassPattern();

        [GeneratedRegex(EW_MISSINGCLASS_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex EwMissingclassPattern2();

        [GeneratedRegex(EW_MISSINGCLASS_PATTERN_SN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwMissingclassPatternSn();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern2();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN2A, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern2A();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern3();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN4, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern4();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN5, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern5();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN6, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern6();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN6A, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern6A();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN7, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern7();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN8, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151Pattern8();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN_FH, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151PatternFh();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN_FH2, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151PatternFh2();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN_FH3, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151PatternFh3();

        [GeneratedRegex(EW_CENSUS_1841_51_PATTERN_FH4, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus184151PatternFh4();

        [GeneratedRegex(EW_CENSUS_1911_1921_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus19111921Pattern();

        [GeneratedRegex(EW_CENSUS_1911_1921_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus19111921Pattern2();

        [GeneratedRegex(EW_CENSUS_1911_1921_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus19111921Pattern3();

        [GeneratedRegex(EW_CENSUS_1911_1921_PATTERN4, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus19111921Pattern4();

        [GeneratedRegex(EW_CENSUS_1911_1921_PATTERN5, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus19111921Pattern5();

        [GeneratedRegex(EW_CENSUS_1911_1921_PATTERN6, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus19111921Pattern6();

        [GeneratedRegex(EW_CENSUS_1911_PATTERN1, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus1911Pattern1();

        [GeneratedRegex(EW_CENSUS_1911_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus1911Pattern2();

        [GeneratedRegex(EW_CENSUS_1921_PATTERN1, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus1921Pattern1();

        [GeneratedRegex(EW_CENSUS_1921_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus1921Pattern2();

        [GeneratedRegex(EW_CENSUS_1921_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus1921Pattern3();

        [GeneratedRegex(EW_CENSUS_1921_PATTERN4, RegexOptions.IgnoreCase)]
        internal static partial Regex EwCensus1921Pattern4();

        [GeneratedRegex(EW_1939_REGISTER_PATTERN1, RegexOptions.IgnoreCase)]
        internal static partial Regex Ew1939RegisterPattern1();

        [GeneratedRegex(EW_1939_REGISTER_PATTERN1A, RegexOptions.IgnoreCase)]
        internal static partial Regex Ew1939RegisterPattern1A();

        [GeneratedRegex(EW_1939_REGISTER_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex Ew1939RegisterPattern2();

        [GeneratedRegex(EW_1939_REGISTER_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex Ew1939RegisterPattern3();

        [GeneratedRegex(SCOT_CENSUSYEAR_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex ScotCensusyearPattern();

        [GeneratedRegex(SCOT_CENSUSYEAR_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex ScotCensusyearPattern2();

        [GeneratedRegex(SCOT_CENSUSYEAR_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex ScotCensusyearPattern3();

        [GeneratedRegex(SCOT_CENSUSYEAR_PATTERN4, RegexOptions.IgnoreCase)]
        internal static partial Regex ScotCensusyearPattern4();

        [GeneratedRegex(SCOT_CENSUS_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex ScotCensusPattern();

        [GeneratedRegex(SCOT_CENSUS_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex ScotCensusPattern2();

        [GeneratedRegex(SCOT_CENSUS_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex ScotCensusPattern3();

        [GeneratedRegex(SCOT_CENSUS_PATTERN4, RegexOptions.IgnoreCase)]
        internal static partial Regex ScotCensusPattern4();

        [GeneratedRegex(SCOT_CENSUS_PATTERN5, RegexOptions.IgnoreCase)]
        internal static partial Regex ScotCensusPattern5();

        [GeneratedRegex(US_CENSUS_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern();

        [GeneratedRegex(US_CENSUS_PATTERN1A, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern1A();

        [GeneratedRegex(US_CENSUS_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern2();

        [GeneratedRegex(US_CENSUS_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern3();

        [GeneratedRegex(US_CENSUS_PATTERN3A, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern3A();

        [GeneratedRegex(US_CENSUS_PATTERN3B, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern3B();

        [GeneratedRegex(US_CENSUS_PATTERN3C, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern3C();

        [GeneratedRegex(US_CENSUS_PATTERN4, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern4();

        [GeneratedRegex(US_CENSUS_PATTERN5, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern5();

        [GeneratedRegex(US_CENSUS_PATTERN6, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern6();

        [GeneratedRegex(US_CENSUS_PATTERN7, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern7();

        [GeneratedRegex(US_CENSUS_PATTERN8, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern8();

        [GeneratedRegex(US_CENSUS_PATTERN9, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusPattern9();

        [GeneratedRegex(US_CENSUS_1940_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensus1940Pattern();

        [GeneratedRegex(US_CENSUS_1940_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensus1940Pattern2();

        [GeneratedRegex(US_CENSUS_1940_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensus1940Pattern3();

        [GeneratedRegex(US_CENSUS_1940_PATTERN4, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensus1940Pattern4();

        [GeneratedRegex(US_CENSUS_T62X_PATTERN1, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusT62XPattern1();

        [GeneratedRegex(US_CENSUS_TX_PATTERN1, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusTxPattern1();

        [GeneratedRegex(US_CENSUS_MX_PATTERN1, RegexOptions.IgnoreCase)]
        internal static partial Regex UsCensusMxPattern1();

        [GeneratedRegex(CANADA_CENSUS_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex CanadaCensusPattern();

        [GeneratedRegex(CANADA_CENSUS_PATTERN1, RegexOptions.IgnoreCase)]
        internal static partial Regex CanadaCensusPattern1();

        [GeneratedRegex(CANADA_CENSUS_PATTERN2, RegexOptions.IgnoreCase)]
        internal static partial Regex CanadaCensusPattern2();

        [GeneratedRegex(CANADA_CENSUS_PATTERN3, RegexOptions.IgnoreCase)]
        internal static partial Regex CanadaCensusPattern3();

        [GeneratedRegex(CANADA_CENSUS_PATTERN4, RegexOptions.IgnoreCase)]
        internal static partial Regex CanadaCensusPattern4();

        [GeneratedRegex(CANADA_CENSUS_PATTERN5, RegexOptions.IgnoreCase)]
        internal static partial Regex CanadaCensusPattern5();

        [GeneratedRegex(CANADA_CENSUS_PATTERN6, RegexOptions.IgnoreCase)]
        internal static partial Regex CanadaCensusPattern6();

        [GeneratedRegex(CANADA_CENSUS_PATTERN7, RegexOptions.IgnoreCase)]
        internal static partial Regex CanadaCensusPattern7();

        [GeneratedRegex(LC_CENSUS_PATTERN_EW, RegexOptions.IgnoreCase)]
        internal static partial Regex LcCensusPatternEw();

        [GeneratedRegex(LC_CENSUS_PATTERN_1911_EW, RegexOptions.IgnoreCase)]
        internal static partial Regex LcCensusPattern1911Ew();

        [GeneratedRegex(LC_CENSUS_PATTERN_SCOT, RegexOptions.IgnoreCase)]
        internal static partial Regex LcCensusPatternScot();

        [GeneratedRegex(LC_CENSUS_PATTERN_1940US, RegexOptions.IgnoreCase)]
        internal static partial Regex LcCensusPattern1940Us();

        [GeneratedRegex(LC_CENSUS_PATTERN_1881CANADA, RegexOptions.IgnoreCase)]
        internal static partial Regex LcCensusPattern1881Canada();

        [GeneratedRegex(LC_ED_PATTERN, RegexOptions.IgnoreCase)]
        internal static partial Regex LcEdRegex();

        [GeneratedRegex(PEOPLEFINDERS, RegexOptions.IgnoreCase)]
        internal static partial Regex Peoplefinders();

        [GeneratedRegex(HAS_NUMBERS, RegexOptions.IgnoreCase)]
        internal static partial Regex HasNumbers();

        [GeneratedRegex(@"(.*)\(.*\)")]
        internal static partial Regex GazateerSlash();
    }
}
