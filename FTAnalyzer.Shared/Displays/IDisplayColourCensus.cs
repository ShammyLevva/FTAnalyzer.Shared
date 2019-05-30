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
        CensusColour C1841 { get; }
        [ColumnDetail("1851", 40)]
        CensusColour C1851 { get; }
        [ColumnDetail("1861", 40)]
        CensusColour C1861 { get; }
        [ColumnDetail("1871", 40)]
        CensusColour C1871 { get; }
        [ColumnDetail("1881", 40)]
        CensusColour C1881 { get; }
        [ColumnDetail("1891", 40)]
        CensusColour C1891 { get; }
        [ColumnDetail("1901", 40)]
        CensusColour C1901 { get; }
        [ColumnDetail("1911", 40)]
        CensusColour C1911 { get; }
        [ColumnDetail("1939", 40)]
        CensusColour C1939 { get; }

        [ColumnDetail("1790", 40)]
        CensusColour US1790 { get; }
        [ColumnDetail("1800", 40)]
        CensusColour US1800 { get; }
        [ColumnDetail("1810", 40)]
        CensusColour US1810 { get; }
        [ColumnDetail("1820", 40)]
        CensusColour US1820 { get; }
        [ColumnDetail("1830", 40)]
        CensusColour US1830 { get; }
        [ColumnDetail("1840", 40)]
        CensusColour US1840 { get; }
        [ColumnDetail("1850", 40)]
        CensusColour US1850 { get; }
        [ColumnDetail("1860", 40)]
        CensusColour US1860 { get; }
        [ColumnDetail("1870", 40)]
        CensusColour US1870 { get; }
        [ColumnDetail("1880", 40)]
        CensusColour US1880 { get; }
        [ColumnDetail("1890", 40)]
        CensusColour US1890 { get; }
        [ColumnDetail("1900", 40)]
        CensusColour US1900 { get; }
        [ColumnDetail("1910", 40)]
        CensusColour US1910 { get; }
        [ColumnDetail("1920", 40)]
        CensusColour US1920 { get; }
        [ColumnDetail("1930", 40)]
        CensusColour US1930 { get; }
        [ColumnDetail("1940", 40)]
        CensusColour US1940 { get; }

        [ColumnDetail("1901", 40)]
        CensusColour Ire1901 { get; }
        [ColumnDetail("1911", 40)]
        CensusColour Ire1911 { get; }

        [ColumnDetail("1851", 40)]
        CensusColour Can1851 { get; }
        [ColumnDetail("1861", 40)]
        CensusColour Can1861 { get; }
        [ColumnDetail("1871", 40)]
        CensusColour Can1871 { get; }
        [ColumnDetail("1881", 40)]
        CensusColour Can1881 { get; }
        [ColumnDetail("1991", 40)]
        CensusColour Can1891 { get; }
        [ColumnDetail("1901", 40)]
        CensusColour Can1901 { get; }
        [ColumnDetail("1906", 40)]
        CensusColour Can1906 { get; }
        [ColumnDetail("1911", 40)]
        CensusColour Can1911 { get; }
        [ColumnDetail("1916", 40)]
        CensusColour Can1916 { get; }
        [ColumnDetail("1921", 40)]
        CensusColour Can1921 { get; }

        [ColumnDetail("1865", 40)]
        CensusColour V1865 { get; }
        [ColumnDetail("1875", 40)]
        CensusColour V1875 { get; }
        [ColumnDetail("1885", 40)]
        CensusColour V1885 { get; }
        [ColumnDetail("1895", 40)]
        CensusColour V1895 { get; }
        [ColumnDetail("1905", 40)]
        CensusColour V1905 { get; }
        [ColumnDetail("1915", 40)]
        CensusColour V1915 { get; }
        [ColumnDetail("1920", 40)]
        CensusColour V1920 { get; }
        [ColumnDetail("1925", 40)]
        CensusColour V1925 { get; }

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
