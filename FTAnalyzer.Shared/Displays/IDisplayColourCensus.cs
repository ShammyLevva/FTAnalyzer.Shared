using FTAnalyzer.Utilities;
using System.Numerics;
using static FTAnalyzer.ColourValues;

namespace FTAnalyzer
{
    public interface IDisplayColourCensus : IColumnComparer<IDisplayColourCensus>
    {
        [ColumnDetail("Ref", 50)]
        string IndividualID { get; }
        [ColumnDetail("Forenames", 100)]
        string Forenames { get; }
        [ColumnDetail("Surnames", 75)]
        string Surname { get; }
        [ColumnDetail("Relation", 120)]
        string Relation { get; }
        [ColumnDetail("Relation to Root", 170)]
        string RelationToRoot { get; }

        [ColumnDetail("1841", 40)]
        CensusColours C1841 { get; }
        [ColumnDetail("1851", 40)]
        CensusColours C1851 { get; }
        [ColumnDetail("1861", 40)]
        CensusColours C1861 { get; }
        [ColumnDetail("1871", 40)]
        CensusColours C1871 { get; }
        [ColumnDetail("1881", 40)]
        CensusColours C1881 { get; }
        [ColumnDetail("1891", 40)]
        CensusColours C1891 { get; }
        [ColumnDetail("1901", 40)]
        CensusColours C1901 { get; }
        [ColumnDetail("1911", 40)]
        CensusColours C1911 { get; }
        [ColumnDetail("1939", 40)]
        CensusColours C1939 { get; }

        [ColumnDetail("1790", 40)]
        CensusColours US1790 { get; }
        [ColumnDetail("1800", 40)]
        CensusColours US1800 { get; }
        [ColumnDetail("1810", 40)]
        CensusColours US1810 { get; }
        [ColumnDetail("1820", 40)]
        CensusColours US1820 { get; }
        [ColumnDetail("1830", 40)]
        CensusColours US1830 { get; }
        [ColumnDetail("1840", 40)]
        CensusColours US1840 { get; }
        [ColumnDetail("1850", 40)]
        CensusColours US1850 { get; }
        [ColumnDetail("1860", 40)]
        CensusColours US1860 { get; }
        [ColumnDetail("1870", 40)]
        CensusColours US1870 { get; }
        [ColumnDetail("1880", 40)]
        CensusColours US1880 { get; }
        [ColumnDetail("1890", 40)]
        CensusColours US1890 { get; }
        [ColumnDetail("1900", 40)]
        CensusColours US1900 { get; }
        [ColumnDetail("1910", 40)]
        CensusColours US1910 { get; }
        [ColumnDetail("1920", 40)]
        CensusColours US1920 { get; }
        [ColumnDetail("1930", 40)]
        CensusColours US1930 { get; }
        [ColumnDetail("1940", 40)]
        CensusColours US1940 { get; }

        [ColumnDetail("1901", 40)]
        CensusColours Ire1901 { get; }
        [ColumnDetail("1911", 40)]
        CensusColours Ire1911 { get; }

        [ColumnDetail("1851", 40)]
        CensusColours Can1851 { get; }
        [ColumnDetail("1861", 40)]
        CensusColours Can1861 { get; }
        [ColumnDetail("1871", 40)]
        CensusColours Can1871 { get; }
        [ColumnDetail("1881", 40)]
        CensusColours Can1881 { get; }
        [ColumnDetail("1991", 40)]
        CensusColours Can1891 { get; }
        [ColumnDetail("1901", 40)]
        CensusColours Can1901 { get; }
        [ColumnDetail("1906", 40)]
        CensusColours Can1906 { get; }
        [ColumnDetail("1911", 40)]
        CensusColours Can1911 { get; }
        [ColumnDetail("1916", 40)]
        CensusColours Can1916 { get; }
        [ColumnDetail("1921", 40)]
        CensusColours Can1921 { get; }

        [ColumnDetail("1865", 40)]
        CensusColours V1865 { get; }
        [ColumnDetail("1875", 40)]
        CensusColours V1875 { get; }
        [ColumnDetail("1885", 40)]
        CensusColours V1885 { get; }
        [ColumnDetail("1895", 40)]
        CensusColours V1895 { get; }
        [ColumnDetail("1905", 40)]
        CensusColours V1905 { get; }
        [ColumnDetail("1915", 40)]
        CensusColours V1915 { get; }
        [ColumnDetail("1920", 40)]
        CensusColours V1920 { get; }
        [ColumnDetail("1925", 40)]
        CensusColours V1925 { get; }

        [ColumnDetail("Birth Date", 170)]
        FactDate BirthDate { get; }
        [ColumnDetail("Birth Location", 250)]
        FactLocation BirthLocation { get; }
        [ColumnDetail("Death Date", 170)]
        FactDate DeathDate { get; }
        [ColumnDetail("Death Location", 250)]
        FactLocation DeathLocation { get; }
        [ColumnDetail("Location", 250)]
        FactLocation BestLocation(FactDate when);
        [ColumnDetail("Ahnentafel", 70, ColumnDetail.ColumnAlignment.Right)]
        BigInteger Ahnentafel { get; }
    }
}
