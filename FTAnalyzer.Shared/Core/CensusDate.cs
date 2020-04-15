using System.Collections.Generic;
using System.Linq;

namespace FTAnalyzer
{
    public class CensusDate : FactDate
    {
        readonly string _displayName;

        public string Country { get; private set; }
        public string PropertyName { get; private set; }

        public static CensusDate ANYCENSUS = new CensusDate("BET 1790 AND 1940", "Any Census", Countries.UNKNOWN_COUNTRY, "");

        public static CensusDate UKCENSUS1841 = new CensusDate("06 JUN 1841", "UK Census 1841", Countries.UNITED_KINGDOM, "C1841");
        public static CensusDate UKCENSUS1851 = new CensusDate("30 MAR 1851", "UK Census 1851", Countries.UNITED_KINGDOM, "C1851");
        public static CensusDate UKCENSUS1861 = new CensusDate("07 APR 1861", "UK Census 1861", Countries.UNITED_KINGDOM, "C1861");
        public static CensusDate UKCENSUS1871 = new CensusDate("02 APR 1871", "UK Census 1871", Countries.UNITED_KINGDOM, "C1871");
        public static CensusDate UKCENSUS1881 = new CensusDate("03 APR 1881", "UK Census 1881", Countries.UNITED_KINGDOM, "C1881");
        public static CensusDate UKCENSUS1891 = new CensusDate("05 APR 1891", "UK Census 1891", Countries.UNITED_KINGDOM, "C1891");
        public static CensusDate UKCENSUS1901 = new CensusDate("31 MAR 1901", "UK Census 1901", Countries.UNITED_KINGDOM, "C1901");
        public static CensusDate UKCENSUS1911 = new CensusDate("02 APR 1911", "UK Census 1911", Countries.UNITED_KINGDOM, "C1911");
        public static CensusDate UKCENSUS1921 = new CensusDate("19 JUN 1921", "UK Census 1921", Countries.UNITED_KINGDOM, "C1921");
        public static CensusDate UKCENSUS1931 = new CensusDate("26 APR 1931", "UK Census 1931", Countries.UNITED_KINGDOM, "C1931");
        public static CensusDate UKCENSUS1939 = new CensusDate("29 SEP 1939", "UK National Register 1939", Countries.UNITED_KINGDOM, "C1939");
        public static CensusDate UKCENSUS1951 = new CensusDate("08 APR 1951", "UK Census 1951", Countries.UNITED_KINGDOM, "C1951");
        public static CensusDate UKCENSUS1961 = new CensusDate("23 APR 1961", "UK Census 1961", Countries.UNITED_KINGDOM, "C1961");
        public static CensusDate UKCENSUS1966 = new CensusDate("24 APR 1966", "UK Census 1966", Countries.UNITED_KINGDOM, "C1966");
        public static CensusDate UKCENSUS1971 = new CensusDate("25 APR 1971", "UK Census 1971", Countries.UNITED_KINGDOM, "C1971");
        public static CensusDate UKCENSUS1981 = new CensusDate("05 APR 1981", "UK Census 1981", Countries.UNITED_KINGDOM, "C1981");
        public static CensusDate UKCENSUS1991 = new CensusDate("21 APR 1991", "UK Census 1991", Countries.UNITED_KINGDOM, "C1991");
        public static CensusDate UKCENSUS2001 = new CensusDate("29 APR 2001", "UK Census 2001", Countries.UNITED_KINGDOM, "C2001");
        public static CensusDate UKCENSUS2011 = new CensusDate("27 MAR 2011", "UK Census 2011", Countries.UNITED_KINGDOM, "C2011");
//        public static CensusDate UKCENSUS2021 = new CensusDate("02 APR 2021", "UK Census 2021", Countries.UNITED_KINGDOM, "C2021");

        public static CensusDate EWCENSUS1841 = new CensusDate("06 JUN 1841", "England & Wales Census 1841", Countries.ENG_WALES, "C1841");
        public static CensusDate EWCENSUS1881 = new CensusDate("03 APR 1881", "England & Wales Census 1881", Countries.ENG_WALES, "C1881");
        public static CensusDate EWCENSUS1911 = new CensusDate("02 APR 1911", "England & Wales Census 1911", Countries.ENG_WALES, "C1911");
        public static CensusDate SCOTCENSUS1881 = new CensusDate("03 APR 1881", "Scotland Census 1881", Countries.SCOTLAND, "C1881");

        public static CensusDate SCOTVALUATION1865 = new CensusDate("BET JUL 1864 AND MAY 1865", "Scottish Valuation Roll 1865", Countries.SCOTLAND, "V1865");
        public static CensusDate SCOTVALUATION1875 = new CensusDate("BET JUL 1874 AND MAY 1875", "Scottish Valuation Roll 1875", Countries.SCOTLAND, "V1875");
        public static CensusDate SCOTVALUATION1885 = new CensusDate("BET JUL 1884 AND MAY 1885", "Scottish Valuation Roll 1885", Countries.SCOTLAND, "V1885");
        public static CensusDate SCOTVALUATION1895 = new CensusDate("BET JUL 1894 AND MAY 1895", "Scottish Valuation Roll 1895", Countries.SCOTLAND, "V1895");
        public static CensusDate SCOTVALUATION1905 = new CensusDate("BET JUL 1904 AND MAY 1905", "Scottish Valuation Roll 1905", Countries.SCOTLAND, "V1905");
        public static CensusDate SCOTVALUATION1915 = new CensusDate("BET JUL 1914 AND MAY 1915", "Scottish Valuation Roll 1915", Countries.SCOTLAND, "V1915");
        public static CensusDate SCOTVALUATION1920 = new CensusDate("BET JUL 1919 AND MAY 1920", "Scottish Valuation Roll 1920", Countries.SCOTLAND, "V1920");
        public static CensusDate SCOTVALUATION1925 = new CensusDate("BET JUL 1924 AND MAY 1925", "Scottish Valuation Roll 1925", Countries.SCOTLAND, "V1925");

        public static CensusDate IRELANDCENSUS1901 = new CensusDate("31 MAR 1901", "Ireland Census 1901", Countries.IRELAND, "Ire1901");
        public static CensusDate IRELANDCENSUS1911 = new CensusDate("02 APR 1911", "Ireland Census 1911", Countries.IRELAND, "Ire1911");

        public static CensusDate USCENSUS1790 = new CensusDate("BET 2 AUG 1790 AND 1791", "US Federal Census 1790", Countries.UNITED_STATES, "US1790");
        public static CensusDate USCENSUS1800 = new CensusDate("BET 4 AUG 1800 AND MAY 1801", "US Federal Census 1800", Countries.UNITED_STATES, "US1800");
        public static CensusDate USCENSUS1810 = new CensusDate("BET 6 AUG 1810 AND JUN 1811", "US Federal Census 1810", Countries.UNITED_STATES, "US1810");
        public static CensusDate USCENSUS1820 = new CensusDate("BET 7 AUG 1820 AND SEP 1821", "US Federal Census 1820", Countries.UNITED_STATES, "US1820");
        public static CensusDate USCENSUS1830 = new CensusDate("BET 1 JUN 1830 AND JUN 1831", "US Federal Census 1830", Countries.UNITED_STATES, "US1830");
        public static CensusDate USCENSUS1840 = new CensusDate("BET 1 JUN 1840 AND DEC 1841", "US Federal Census 1840", Countries.UNITED_STATES, "US1840");
        public static CensusDate USCENSUS1850 = new CensusDate("BET 1 JUN 1850 AND MAR 1851", "US Federal Census 1850", Countries.UNITED_STATES, "US1850");
        public static CensusDate USCENSUS1860 = new CensusDate("BET 1 JUN 1860 AND MAR 1861", "US Federal Census 1860", Countries.UNITED_STATES, "US1860");
        public static CensusDate USCENSUS1870 = new CensusDate("BET 1 JUN 1870 AND MAR 1871", "US Federal Census 1870", Countries.UNITED_STATES, "US1870");
        public static CensusDate USCENSUS1880 = new CensusDate("BET 1 JUN 1880 AND 30 JUN 1880", "US Federal Census 1880", Countries.UNITED_STATES, "US1880");
        public static CensusDate USCENSUS1890 = new CensusDate("BET 2 JUN 1890 AND 30 JUN 1890", "US Federal Census 1890", Countries.UNITED_STATES, "US1890");
        public static CensusDate USCENSUS1900 = new CensusDate("BET 1 JUN 1900 AND 30 JUN 1900", "US Federal Census 1900", Countries.UNITED_STATES, "US1900");
        public static CensusDate USCENSUS1910 = new CensusDate("BET 15 APR 1910 AND 31 DEC 1910", "US Federal Census 1910", Countries.UNITED_STATES, "US1910");
        public static CensusDate USCENSUS1920 = new CensusDate("BET 1 JAN 1920 AND 31 DEC 1920", "US Federal Census 1920", Countries.UNITED_STATES, "US1920");
        public static CensusDate USCENSUS1930 = new CensusDate("BET 1 OCT 1929 AND 31 DEC 1930", "US Federal Census 1930", Countries.UNITED_STATES, "US1930");
        public static CensusDate USCENSUS1940 = new CensusDate("BET 1 APR 1940 AND 31 MAY 1940", "US Federal Census 1940", Countries.UNITED_STATES, "US1940");
        public static CensusDate USCENSUS1950 = new CensusDate("BET 1 APR 1950 AND 31 MAY 1950", "US Federal Census 1950", Countries.UNITED_STATES, "US1950");
        public static CensusDate USCENSUS1960 = new CensusDate("BET 1 APR 1950 AND 31 MAY 1960", "US Federal Census 1960", Countries.UNITED_STATES, "US1960");
        public static CensusDate USCENSUS1970 = new CensusDate("BET 1 APR 1950 AND 31 MAY 1970", "US Federal Census 1970", Countries.UNITED_STATES, "US1970");
        public static CensusDate USCENSUS1980 = new CensusDate("BET 1 APR 1950 AND 31 MAY 1980", "US Federal Census 1980", Countries.UNITED_STATES, "US1980");
        public static CensusDate USCENSUS1990 = new CensusDate("BET 1 APR 1950 AND 31 MAY 1990", "US Federal Census 1990", Countries.UNITED_STATES, "US1990");
        public static CensusDate USCENSUS2000 = new CensusDate("BET 1 APR 1950 AND 31 MAY 2000", "US Federal Census 2000", Countries.UNITED_STATES, "US2000");
        public static CensusDate USCENSUS2010 = new CensusDate("BET 1 APR 1950 AND 31 MAY 2010", "US Federal Census 2010", Countries.UNITED_STATES, "US2010");
        public static CensusDate USCENSUS2020 = new CensusDate("BET 1 APR 1950 AND 31 MAY 2020", "US Federal Census 2020", Countries.UNITED_STATES, "US2020");

        public static CensusDate CANADACENSUS1851 = new CensusDate("BET 1851 AND 1852", "Canadian Census 1851/2", Countries.CANADA, "Can1851");
        public static CensusDate CANADACENSUS1861 = new CensusDate("1861", "Canadian Census 1861", Countries.CANADA, "Can1861");
        public static CensusDate CANADACENSUS1871 = new CensusDate("2 APR 1871", "Canadian Census 1871", Countries.CANADA, "Can1871");
        public static CensusDate CANADACENSUS1881 = new CensusDate("4 APR 1881", "Canadian Census 1881", Countries.CANADA, "Can1881");
        public static CensusDate CANADACENSUS1891 = new CensusDate("6 APR 1891", "Canadian Census 1891", Countries.CANADA, "Can1891");
        public static CensusDate CANADACENSUS1901 = new CensusDate("31 MAR 1901", "Canadian Census 1901", Countries.CANADA, "Can1901");
        public static CensusDate CANADACENSUS1906 = new CensusDate("1906", "Canadian Census 1906", Countries.CANADA, "Can1906");
        public static CensusDate CANADACENSUS1911 = new CensusDate("1 JUN 1911", "Canadian Census 1911", Countries.CANADA, "Can1911");
        public static CensusDate CANADACENSUS1916 = new CensusDate("1916", "Canadian Census 1916", Countries.CANADA, "Can1916");
        public static CensusDate CANADACENSUS1921 = new CensusDate("1 JUN 1921", "Canadian Census 1921", Countries.CANADA, "Can1921");
        public static CensusDate CANADACENSUS1931 = new CensusDate("1 JUN 1931", "Canadian Census 1931", Countries.CANADA, "Can1931");
        public static CensusDate CANADACENSUS1941 = new CensusDate("1 JUN 1941", "Canadian Census 1941", Countries.CANADA, "Can1941");
        public static CensusDate CANADACENSUS1951 = new CensusDate("1 JUN 1951", "Canadian Census 1951", Countries.CANADA, "Can1951");
        public static CensusDate CANADACENSUS1961 = new CensusDate("1 JUN 1961", "Canadian Census 1961", Countries.CANADA, "Can1961");
        public static CensusDate CANADACENSUS1971 = new CensusDate("1 JUN 1971", "Canadian Census 1971", Countries.CANADA, "Can1971");
        public static CensusDate CANADACENSUS1976 = new CensusDate("1976", "Canadian Census 1976", Countries.CANADA, "Can1976");
        public static CensusDate CANADACENSUS1981 = new CensusDate("1 JUN 1981", "Canadian Census 1981", Countries.CANADA, "Can1981");
        public static CensusDate CANADACENSUS1986 = new CensusDate("1986", "Canadian Census 1986", Countries.CANADA, "Can1986");
        public static CensusDate CANADACENSUS1991 = new CensusDate("1 JUN 1991", "Canadian Census 1991", Countries.CANADA, "Can1991");
        public static CensusDate CANADACENSUS1996 = new CensusDate("1996", "Canadian Census 1996", Countries.CANADA, "Can1996");
        public static CensusDate CANADACENSUS2001 = new CensusDate("1 JUN 2001", "Canadian Census 2001", Countries.CANADA, "Can2001");
        public static CensusDate CANADACENSUS2006 = new CensusDate("2006", "Canadian Census 2006", Countries.CANADA, "Can2006");
        public static CensusDate CANADACENSUS2011 = new CensusDate("1 JUN 2011", "Canadian Census 2011", Countries.CANADA, "Can2011");
        public static CensusDate CANADACENSUS2016 = new CensusDate("2016", "Canadian Census 2016", Countries.CANADA, "Can2016");

        public static readonly ISet<CensusDate> UK_CENSUS = new HashSet<CensusDate>(new CensusDate[] {
            UKCENSUS1841, UKCENSUS1851, UKCENSUS1861, UKCENSUS1871, UKCENSUS1881, UKCENSUS1891, UKCENSUS1901, UKCENSUS1911, UKCENSUS1939,
            UKCENSUS1921, UKCENSUS1931, UKCENSUS1951, UKCENSUS1961, UKCENSUS1966, UKCENSUS1971, UKCENSUS1981, UKCENSUS1991, UKCENSUS2001,
            UKCENSUS2011
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

        public static ISet<CensusDate> SUPPORTED_CENSUS = new HashSet<CensusDate>(new CensusDate[] {
            UKCENSUS1841, UKCENSUS1851, UKCENSUS1861, UKCENSUS1871, UKCENSUS1881, UKCENSUS1891, UKCENSUS1901, UKCENSUS1911, UKCENSUS1939,
            UKCENSUS1921, UKCENSUS1931, UKCENSUS1951, UKCENSUS1961, UKCENSUS1966, UKCENSUS1971, UKCENSUS1981, UKCENSUS1991, UKCENSUS2001,
            UKCENSUS2011, 
            USCENSUS1790, USCENSUS1800, USCENSUS1810, USCENSUS1820, USCENSUS1830, USCENSUS1840, USCENSUS1850, USCENSUS1860, USCENSUS1870,
            USCENSUS1880, USCENSUS1890, USCENSUS1900, USCENSUS1910, USCENSUS1920, USCENSUS1930, USCENSUS1940, USCENSUS1950, USCENSUS1960,
            USCENSUS1970, USCENSUS1980, USCENSUS1990, USCENSUS2000, USCENSUS2010, USCENSUS2020,
            CANADACENSUS1851, CANADACENSUS1861, CANADACENSUS1871, CANADACENSUS1881, CANADACENSUS1891, CANADACENSUS1901, CANADACENSUS1906,
            CANADACENSUS1911, CANADACENSUS1916, CANADACENSUS1921, CANADACENSUS1931, CANADACENSUS1941, CANADACENSUS1951, CANADACENSUS1961,
            CANADACENSUS1971, CANADACENSUS1976, CANADACENSUS1981, CANADACENSUS1986, CANADACENSUS1991, CANADACENSUS1996, CANADACENSUS2001,
            CANADACENSUS2006, CANADACENSUS2011, CANADACENSUS2016, IRELANDCENSUS1911,
            EWCENSUS1841, EWCENSUS1881, EWCENSUS1911, SCOTCENSUS1881
        });

        public static ISet<CensusDate> LOSTCOUSINS_CENSUS = new HashSet<CensusDate>(new CensusDate[] {
            EWCENSUS1841, EWCENSUS1881, SCOTCENSUS1881, EWCENSUS1911, USCENSUS1880, USCENSUS1940, CANADACENSUS1881, IRELANDCENSUS1911
        });

        public static ISet<CensusDate> VALUATIONROLLS = new HashSet<CensusDate>(new CensusDate[] {
             SCOTVALUATION1865, SCOTVALUATION1875, SCOTVALUATION1885, SCOTVALUATION1895, SCOTVALUATION1905, SCOTVALUATION1915, SCOTVALUATION1920, SCOTVALUATION1925
        });

        CensusDate(string str, string displayName, string country, string propertyName)
            : base(str)
        {
            _displayName = displayName;
            Country = country;
            PropertyName = propertyName;
        }

        public CensusDate EquivalentUSCensus
        {
            get
            {
                switch (StartDate.Year)
                {
                    case 1841:
                        return USCENSUS1840;
                    case 1851:
                        return USCENSUS1850;
                    case 1861:
                        return USCENSUS1860;
                    case 1871:
                        return USCENSUS1870;
                    case 1881:
                        return USCENSUS1880;
                    case 1891:
                        return USCENSUS1890;
                    case 1901:
                        return USCENSUS1900;
                    case 1911:
                        return USCENSUS1910;
                    case 1939:
                        return USCENSUS1940;
                    default:
                        return null;
                }
            }
        }

        public static bool IsCensusYear(FactDate fd, string Country, bool exactYear)
        {
            switch (Country)
            {
                case Countries.UNITED_STATES:
                    return US_FEDERAL_CENSUS.Any(cd => fd.Overlaps(cd));

                case Countries.CANADA:
                    return CANADIAN_CENSUS.Any(cd => fd.Overlaps(cd));

                default:
                    return SUPPORTED_CENSUS.Any(cd =>
                        (exactYear && fd.CensusYearMatches(cd)) || (!exactYear && fd.Overlaps(cd)));
            }
        }

        public static bool IsUKCensusYear(FactDate fd, bool exactYear) =>
            UK_CENSUS.Any(cd =>
                (exactYear && fd.CensusYearMatches(cd)) || (!exactYear && fd.Overlaps(cd))
            );

        public static bool IsLostCousinsCensusYear(FactDate fd, bool exactYear) =>
            GetLostCousinsCensusYear(fd, exactYear) != null;

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
