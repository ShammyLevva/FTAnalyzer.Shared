using System.Collections.Generic;
using System.Linq;

namespace FTAnalyzer
{
    public class CensusDate : FactDate
    {
        readonly string _displayName;

        public string Country { get; private set; }
        public string PropertyName { get; private set; }
        public string AncestryCatalog { get; private set; }

        public static readonly CensusDate ANYCENSUS = new("BET 1790 AND 2022", "Any Census", Countries.UNKNOWN_COUNTRY, "");

        public static readonly CensusDate UKCENSUS1841 = new("06 JUN 1841", "UK Census 1841", Countries.UNITED_KINGDOM, "C1841");
        public static readonly CensusDate UKCENSUS1851 = new("30 MAR 1851", "UK Census 1851", Countries.UNITED_KINGDOM, "C1851");
        public static readonly CensusDate UKCENSUS1861 = new("07 APR 1861", "UK Census 1861", Countries.UNITED_KINGDOM, "C1861");
        public static readonly CensusDate UKCENSUS1871 = new("02 APR 1871", "UK Census 1871", Countries.UNITED_KINGDOM, "C1871");
        public static readonly CensusDate UKCENSUS1881 = new("03 APR 1881", "UK Census 1881", Countries.UNITED_KINGDOM, "C1881");
        public static readonly CensusDate UKCENSUS1891 = new("05 APR 1891", "UK Census 1891", Countries.UNITED_KINGDOM, "C1891");
        public static readonly CensusDate UKCENSUS1901 = new("31 MAR 1901", "UK Census 1901", Countries.UNITED_KINGDOM, "C1901");
        public static readonly CensusDate UKCENSUS1911 = new("02 APR 1911", "UK Census 1911", Countries.UNITED_KINGDOM, "C1911");
        public static readonly CensusDate UKCENSUS1921 = new("19 JUN 1921", "UK Census 1921", Countries.UNITED_KINGDOM, "C1921");
        public static readonly CensusDate UKCENSUS1939 = new("29 SEP 1939", "UK National Register 1939", Countries.UNITED_KINGDOM, "C1939");
        public static readonly CensusDate UKCENSUS1951 = new("08 APR 1951", "UK Census 1951", Countries.UNITED_KINGDOM, "C1951");
        public static readonly CensusDate UKCENSUS1961 = new("23 APR 1961", "UK Census 1961", Countries.UNITED_KINGDOM, "C1961");
        public static readonly CensusDate UKCENSUS1966 = new("24 APR 1966", "UK Census 1966", Countries.UNITED_KINGDOM, "C1966");
        public static readonly CensusDate UKCENSUS1971 = new("25 APR 1971", "UK Census 1971", Countries.UNITED_KINGDOM, "C1971");
        public static readonly CensusDate UKCENSUS1981 = new("05 APR 1981", "UK Census 1981", Countries.UNITED_KINGDOM, "C1981");
        public static readonly CensusDate UKCENSUS1991 = new("21 APR 1991", "UK Census 1991", Countries.UNITED_KINGDOM, "C1991");
        public static readonly CensusDate UKCENSUS2001 = new("29 APR 2001", "UK Census 2001", Countries.UNITED_KINGDOM, "C2001");
        public static readonly CensusDate UKCENSUS2011 = new("27 MAR 2011", "UK Census 2011", Countries.UNITED_KINGDOM, "C2011");
                     
        public static readonly CensusDate EWCENSUS1841 = new("06 JUN 1841", "England & Wales Census 1841", Countries.ENG_WALES, "C1841");
        public static readonly CensusDate EWCENSUS1881 = new("03 APR 1881", "England & Wales Census 1881", Countries.ENG_WALES, "C1881");
        public static readonly CensusDate EWCENSUS1911 = new("02 APR 1911", "England & Wales Census 1911", Countries.ENG_WALES, "C1911");
        public static readonly CensusDate EWCENSUS2021 = new("21 MAR 2021", "England & Wales Census 2021", Countries.ENG_WALES, "C2021");
        public static readonly CensusDate SCOTCENSUS1881 = new("03 APR 1881", "Scotland Census 1881", Countries.SCOTLAND, "C1881");
        public static readonly CensusDate SCOTCENSUS1931 = new("26 APR 1931", "Scotland Census 1931", Countries.SCOTLAND, "C1931");
        public static readonly CensusDate SCOTCENSUS2022 = new("02 APR 2022", "Scotland Census 2022", Countries.SCOTLAND, "C2021");
                  
        public static readonly CensusDate SCOTVALUATION1855 = new("BET JUL 1854 AND MAY 1855", "Scottish Valuation Roll 1855", Countries.SCOTLAND, "V1855");
        public static readonly CensusDate SCOTVALUATION1865 = new("BET JUL 1864 AND MAY 1865", "Scottish Valuation Roll 1865", Countries.SCOTLAND, "V1865");
        public static readonly CensusDate SCOTVALUATION1875 = new("BET JUL 1874 AND MAY 1875", "Scottish Valuation Roll 1875", Countries.SCOTLAND, "V1875");
        public static readonly CensusDate SCOTVALUATION1885 = new("BET JUL 1884 AND MAY 1885", "Scottish Valuation Roll 1885", Countries.SCOTLAND, "V1885");
        public static readonly CensusDate SCOTVALUATION1895 = new("BET JUL 1894 AND MAY 1895", "Scottish Valuation Roll 1895", Countries.SCOTLAND, "V1895");
        public static readonly CensusDate SCOTVALUATION1905 = new("BET JUL 1904 AND MAY 1905", "Scottish Valuation Roll 1905", Countries.SCOTLAND, "V1905");
        public static readonly CensusDate SCOTVALUATION1915 = new("BET JUL 1914 AND MAY 1915", "Scottish Valuation Roll 1915", Countries.SCOTLAND, "V1915");
        public static readonly CensusDate SCOTVALUATION1920 = new("BET JUL 1919 AND MAY 1920", "Scottish Valuation Roll 1920", Countries.SCOTLAND, "V1920");
        public static readonly CensusDate SCOTVALUATION1925 = new("BET JUL 1924 AND MAY 1925", "Scottish Valuation Roll 1925", Countries.SCOTLAND, "V1925");
        public static readonly CensusDate SCOTVALUATION1930 = new("BET JUL 1929 AND MAY 1930", "Scottish Valuation Roll 1930", Countries.SCOTLAND, "V1930");
        public static readonly CensusDate SCOTVALUATION1935 = new("BET JUL 1934 AND MAY 1935", "Scottish Valuation Roll 1935", Countries.SCOTLAND, "V1935");
        public static readonly CensusDate SCOTVALUATION1940 = new("BET JUL 1939 AND MAY 1940", "Scottish Valuation Roll 1940", Countries.SCOTLAND, "V1940");
                      
        public static readonly CensusDate IRELANDCENSUS1901 = new("31 MAR 1901", "Ireland Census 1901", Countries.IRELAND, "Ire1901");
        public static readonly CensusDate IRELANDCENSUS1911 = new("02 APR 1911", "Ireland Census 1911", Countries.IRELAND, "Ire1911");
                     
        public static readonly CensusDate USCENSUS1790 = new("BET 2 AUG 1790 AND 1791", "US Federal Census 1790", Countries.UNITED_STATES, "US1790", "5058");
        public static readonly CensusDate USCENSUS1800 = new("BET 4 AUG 1800 AND MAY 1801", "US Federal Census 1800", Countries.UNITED_STATES, "US1800", "7590");
        public static readonly CensusDate USCENSUS1810 = new("BET 6 AUG 1810 AND JUN 1811", "US Federal Census 1810", Countries.UNITED_STATES, "US1810", "7613");
        public static readonly CensusDate USCENSUS1820 = new("BET 7 AUG 1820 AND SEP 1821", "US Federal Census 1820", Countries.UNITED_STATES, "US1820", "7734");
        public static readonly CensusDate USCENSUS1830 = new("BET 1 JUN 1830 AND JUN 1831", "US Federal Census 1830", Countries.UNITED_STATES, "US1830", "8058");
        public static readonly CensusDate USCENSUS1840 = new("BET 1 JUN 1840 AND DEC 1841", "US Federal Census 1840", Countries.UNITED_STATES, "US1840", "8057");
        public static readonly CensusDate USCENSUS1850 = new("BET 1 JUN 1850 AND MAR 1851", "US Federal Census 1850", Countries.UNITED_STATES, "US1850", "8054");
        public static readonly CensusDate USCENSUS1860 = new("BET 1 JUN 1860 AND MAR 1861", "US Federal Census 1860", Countries.UNITED_STATES, "US1860", "7667");
        public static readonly CensusDate USCENSUS1870 = new("BET 1 JUN 1870 AND MAR 1871", "US Federal Census 1870", Countries.UNITED_STATES, "US1870", "7163");
        public static readonly CensusDate USCENSUS1880 = new("BET 1 JUN 1880 AND 30 JUN 1880", "US Federal Census 1880", Countries.UNITED_STATES, "US1880", "6742");
        public static readonly CensusDate USCENSUS1890 = new("BET 2 JUN 1890 AND 30 JUN 1890", "US Federal Census 1890", Countries.UNITED_STATES, "US1890", "5445");
        public static readonly CensusDate USCENSUS1900 = new("BET 1 JUN 1900 AND 30 JUN 1900", "US Federal Census 1900", Countries.UNITED_STATES, "US1900", "7602");
        public static readonly CensusDate USCENSUS1910 = new("BET 15 APR 1910 AND 31 DEC 1910", "US Federal Census 1910", Countries.UNITED_STATES, "US1910", "7884");
        public static readonly CensusDate USCENSUS1920 = new("BET 1 JAN 1920 AND 31 DEC 1920", "US Federal Census 1920", Countries.UNITED_STATES, "US1920", "6061");
        public static readonly CensusDate USCENSUS1930 = new("BET 1 OCT 1929 AND 31 DEC 1930", "US Federal Census 1930", Countries.UNITED_STATES, "US1930", "6224");
        public static readonly CensusDate USCENSUS1940 = new("BET 1 APR 1940 AND 31 MAY 1940", "US Federal Census 1940", Countries.UNITED_STATES, "US1940", "2442");
        public static readonly CensusDate USCENSUS1950 = new("BET 1 APR 1950 AND 31 MAY 1950", "US Federal Census 1950", Countries.UNITED_STATES, "US1950");
        public static readonly CensusDate USCENSUS1960 = new("BET 1 APR 1960 AND 31 MAY 1960", "US Federal Census 1960", Countries.UNITED_STATES, "US1960");
        public static readonly CensusDate USCENSUS1970 = new("BET 1 APR 1970 AND 31 MAY 1970", "US Federal Census 1970", Countries.UNITED_STATES, "US1970");
        public static readonly CensusDate USCENSUS1980 = new("BET 1 APR 1980 AND 31 MAY 1980", "US Federal Census 1980", Countries.UNITED_STATES, "US1980");
        public static readonly CensusDate USCENSUS1990 = new("BET 1 APR 1990 AND 31 MAY 1990", "US Federal Census 1990", Countries.UNITED_STATES, "US1990");
        public static readonly CensusDate USCENSUS2000 = new("BET 1 APR 2000 AND 31 MAY 2000", "US Federal Census 2000", Countries.UNITED_STATES, "US2000");
        public static readonly CensusDate USCENSUS2010 = new("BET 1 APR 2010 AND 31 MAY 2010", "US Federal Census 2010", Countries.UNITED_STATES, "US2010");
        public static readonly CensusDate USCENSUS2020 = new("BET 1 APR 2020 AND 31 MAY 2020", "US Federal Census 2020", Countries.UNITED_STATES, "US2020");
                      
        public static readonly CensusDate CANADACENSUS1851 = new("BET 1851 AND 1852", "Canadian Census 1851/2", Countries.CANADA, "Can1851");
        public static readonly CensusDate CANADACENSUS1861 = new("1861", "Canadian Census 1861", Countries.CANADA, "Can1861");
        public static readonly CensusDate CANADACENSUS1871 = new("2 APR 1871", "Canadian Census 1871", Countries.CANADA, "Can1871");
        public static readonly CensusDate CANADACENSUS1881 = new("4 APR 1881", "Canadian Census 1881", Countries.CANADA, "Can1881");
        public static readonly CensusDate CANADACENSUS1891 = new("6 APR 1891", "Canadian Census 1891", Countries.CANADA, "Can1891");
        public static readonly CensusDate CANADACENSUS1901 = new("31 MAR 1901", "Canadian Census 1901", Countries.CANADA, "Can1901");
        public static readonly CensusDate CANADACENSUS1906 = new("1906", "Canadian Census 1906", Countries.CANADA, "Can1906");
        public static readonly CensusDate CANADACENSUS1911 = new("1 JUN 1911", "Canadian Census 1911", Countries.CANADA, "Can1911");
        public static readonly CensusDate CANADACENSUS1916 = new("1916", "Canadian Census 1916", Countries.CANADA, "Can1916");
        public static readonly CensusDate CANADACENSUS1921 = new("1 JUN 1921", "Canadian Census 1921", Countries.CANADA, "Can1921");
        public static readonly CensusDate CANADACENSUS1931 = new("1 JUN 1931", "Canadian Census 1931", Countries.CANADA, "Can1931");
        public static readonly CensusDate CANADACENSUS1941 = new("1 JUN 1941", "Canadian Census 1941", Countries.CANADA, "Can1941");
        public static readonly CensusDate CANADACENSUS1951 = new("1 JUN 1951", "Canadian Census 1951", Countries.CANADA, "Can1951");
        public static readonly CensusDate CANADACENSUS1961 = new("1 JUN 1961", "Canadian Census 1961", Countries.CANADA, "Can1961");
        public static readonly CensusDate CANADACENSUS1971 = new("1 JUN 1971", "Canadian Census 1971", Countries.CANADA, "Can1971");
        public static readonly CensusDate CANADACENSUS1976 = new("1976", "Canadian Census 1976", Countries.CANADA, "Can1976");
        public static readonly CensusDate CANADACENSUS1981 = new("1 JUN 1981", "Canadian Census 1981", Countries.CANADA, "Can1981");
        public static readonly CensusDate CANADACENSUS1986 = new("1986", "Canadian Census 1986", Countries.CANADA, "Can1986");
        public static readonly CensusDate CANADACENSUS1991 = new("1 JUN 1991", "Canadian Census 1991", Countries.CANADA, "Can1991");
        public static readonly CensusDate CANADACENSUS1996 = new("1996", "Canadian Census 1996", Countries.CANADA, "Can1996");
        public static readonly CensusDate CANADACENSUS2001 = new("1 JUN 2001", "Canadian Census 2001", Countries.CANADA, "Can2001");
        public static readonly CensusDate CANADACENSUS2006 = new("2006", "Canadian Census 2006", Countries.CANADA, "Can2006");
        public static readonly CensusDate CANADACENSUS2011 = new("1 JUN 2011", "Canadian Census 2011", Countries.CANADA, "Can2011");
        public static readonly CensusDate CANADACENSUS2016 = new("2016", "Canadian Census 2016", Countries.CANADA, "Can2016");

        public static readonly ISet<CensusDate> UK_CENSUS = new HashSet<CensusDate>(new CensusDate[] {
            UKCENSUS1841, UKCENSUS1851, UKCENSUS1861, UKCENSUS1871, UKCENSUS1881, UKCENSUS1891, UKCENSUS1901, UKCENSUS1911, UKCENSUS1921,
            UKCENSUS1939, UKCENSUS1951, UKCENSUS1961, UKCENSUS1966, UKCENSUS1971, UKCENSUS1981, UKCENSUS1991, UKCENSUS2001,
            UKCENSUS2011, EWCENSUS2021, SCOTCENSUS1931, SCOTCENSUS2022
        });

        public static readonly ISet<CensusDate> US_FEDERAL_CENSUS = new HashSet<CensusDate>(new CensusDate[] {
            USCENSUS1790, USCENSUS1800, USCENSUS1810, USCENSUS1820, USCENSUS1830, USCENSUS1840, USCENSUS1850, USCENSUS1860, USCENSUS1870, 
            USCENSUS1880, USCENSUS1890, USCENSUS1900, USCENSUS1910, USCENSUS1920, USCENSUS1930, USCENSUS1940, USCENSUS1950, USCENSUS1960, 
            USCENSUS1970, USCENSUS1980, USCENSUS1990, USCENSUS2000, USCENSUS2010, USCENSUS2020
        });

        public static readonly ISet<CensusDate> CANADIAN_CENSUS = new HashSet<CensusDate>(new CensusDate[] {
            CANADACENSUS1851, CANADACENSUS1861, CANADACENSUS1871, CANADACENSUS1881, CANADACENSUS1891, CANADACENSUS1901, CANADACENSUS1906,
            CANADACENSUS1911, CANADACENSUS1916, CANADACENSUS1921, CANADACENSUS1931, CANADACENSUS1941, CANADACENSUS1951, CANADACENSUS1961,
            CANADACENSUS1971, CANADACENSUS1976, CANADACENSUS1981, CANADACENSUS1986, CANADACENSUS1991, CANADACENSUS1996, CANADACENSUS2001,
            CANADACENSUS2006, CANADACENSUS2011, CANADACENSUS2016
        });

        public static readonly ISet<CensusDate> SUPPORTED_CENSUS = new HashSet<CensusDate>(new CensusDate[] {
            UKCENSUS1841, UKCENSUS1851, UKCENSUS1861, UKCENSUS1871, UKCENSUS1881, UKCENSUS1891, UKCENSUS1901, UKCENSUS1911, UKCENSUS1921,
            UKCENSUS1939, UKCENSUS1951, UKCENSUS1961, UKCENSUS1966, UKCENSUS1971, UKCENSUS1981, UKCENSUS1991, UKCENSUS2001,
            UKCENSUS2011, 
            USCENSUS1790, USCENSUS1800, USCENSUS1810, USCENSUS1820, USCENSUS1830, USCENSUS1840, USCENSUS1850, USCENSUS1860, USCENSUS1870,
            USCENSUS1880, USCENSUS1890, USCENSUS1900, USCENSUS1910, USCENSUS1920, USCENSUS1930, USCENSUS1940, USCENSUS1950, USCENSUS1960,
            USCENSUS1970, USCENSUS1980, USCENSUS1990, USCENSUS2000, USCENSUS2010, USCENSUS2020,
            CANADACENSUS1851, CANADACENSUS1861, CANADACENSUS1871, CANADACENSUS1881, CANADACENSUS1891, CANADACENSUS1901, CANADACENSUS1906,
            CANADACENSUS1911, CANADACENSUS1916, CANADACENSUS1921, CANADACENSUS1931, CANADACENSUS1941, CANADACENSUS1951, CANADACENSUS1961,
            CANADACENSUS1971, CANADACENSUS1976, CANADACENSUS1981, CANADACENSUS1986, CANADACENSUS1991, CANADACENSUS1996, CANADACENSUS2001,
            CANADACENSUS2006, CANADACENSUS2011, CANADACENSUS2016, IRELANDCENSUS1911,
            EWCENSUS1841, EWCENSUS1881, EWCENSUS1911, EWCENSUS2021, SCOTCENSUS1881, SCOTCENSUS1931, SCOTCENSUS2022
        });

        public static readonly ISet<CensusDate> LOSTCOUSINS_CENSUS = new HashSet<CensusDate>(new CensusDate[] {
            EWCENSUS1841, EWCENSUS1881, SCOTCENSUS1881, EWCENSUS1911, USCENSUS1880, USCENSUS1940, CANADACENSUS1881, IRELANDCENSUS1911
        });

        public static readonly ISet<CensusDate> VALUATIONROLLS = new HashSet<CensusDate>(new CensusDate[] {
            SCOTVALUATION1855, SCOTVALUATION1865, SCOTVALUATION1875, SCOTVALUATION1885, SCOTVALUATION1895, SCOTVALUATION1905, 
            SCOTVALUATION1915, SCOTVALUATION1920, SCOTVALUATION1925, SCOTVALUATION1930, SCOTVALUATION1935, SCOTVALUATION1940
        });

        CensusDate(string str, string displayName, string country, string propertyName, string ancestryCatalog = "")
            : base(str)
        {
            _displayName = displayName;
            Country = country;
            PropertyName = propertyName;
            AncestryCatalog = ancestryCatalog;
        }

        public CensusDate EquivalentUSCensus
        {
            get
            {
                return StartDate.Year switch
                {
                    1841 => USCENSUS1840,
                    1851 => USCENSUS1850,
                    1861 => USCENSUS1860,
                    1871 => USCENSUS1870,
                    1881 => USCENSUS1880,
                    1891 => USCENSUS1890,
                    1901 => USCENSUS1900,
                    1911 => USCENSUS1910,
                    1921 => USCENSUS1920,
                    1931 => USCENSUS1930,
                    1939 => USCENSUS1940,
                    1951 => USCENSUS1950,
                    1961 => USCENSUS1960,
                    1971 => USCENSUS1970,
                    1981 => USCENSUS1980,
                    1991 => USCENSUS1990,
                    2001 => USCENSUS2000,
                    2011 => USCENSUS2010,
                    2021 or 2022 => USCENSUS2020,
                    _ => null,
                };
            }
        }

        public static bool IsCensusYear(FactDate fd, string Country, bool exactYear)
        {
            return Country switch
            {
                Countries.UNITED_STATES => US_FEDERAL_CENSUS.Any(cd => fd.Overlaps(cd)),
                Countries.CANADA => CANADIAN_CENSUS.Any(cd => fd.Overlaps(cd)),
                _ => SUPPORTED_CENSUS.Any(cd => (exactYear && fd.CensusYearMatches(cd)) || (!exactYear && fd.Overlaps(cd))),
            };
        }

        public static bool IsUKCensusYear(FactDate fd, bool exactYear) =>
            UK_CENSUS.Any(cd =>
                (exactYear && fd.CensusYearMatches(cd)) || (!exactYear && fd.Overlaps(cd))
            );

        public static bool IsLostCousinsCensusYear(FactDate fd, bool exactYear) =>
            GetLostCousinsCensusYear(fd, exactYear) is not null;

        public static bool IsCensusCountry(FactDate fd, FactLocation location) =>
            SUPPORTED_CENSUS.Any(cd => cd.Country == location.CensusCountry && fd.CensusYearMatches(cd));

        public static CensusDate GetLostCousinsCensusYear(FactDate fd, bool exactYear) =>
            LOSTCOUSINS_CENSUS.FirstOrDefault(
                cd => (exactYear && fd.CensusYearMatches(cd)) || (!exactYear && fd.Overlaps(cd))
            );

        public static FactDate GetUSCensusDateFromReference(string reference) =>
            US_FEDERAL_CENSUS.FirstOrDefault(cd => cd.PropertyName == reference) ?? UNKNOWN_DATE;

        public static FactDate GetCanadianCensusDateFromReference(string reference) =>
            CANADIAN_CENSUS.FirstOrDefault(cd => cd.PropertyName.ToUpper() == reference) ?? UNKNOWN_DATE;

        public static FactDate GetUKCensusDateFromYear(string year) =>
            UK_CENSUS.FirstOrDefault(cd => cd.PropertyName == $"C{year}") ?? UNKNOWN_DATE;

        public override string ToString() => _displayName;
    }
}
