using FTAnalyzer.Utilities;
using static FTAnalyzer.ColourValues;

namespace FTAnalyzer
{
    public interface IDisplayColourCensus
    {
        [ColumnDetail("Ref", 60)]
        string IndividualID { get; }
        [ColumnDetail("Forenames", 100)]
        string Forenames { get; }
        [ColumnDetail("Surnames", 75)]
        string Surname { get; }
        [ColumnDetail("Relation", 120)]
        string Relation { get; }
        [ColumnDetail("Relation to Root", 160)]
        string RelationToRoot { get; }

        [ColumnDetail("1841", 20)]
        CensusColour C1841 { get; }
        [ColumnDetail("1851", 20)]
        CensusColour C1851 { get; }
        [ColumnDetail("1861", 20)]
        CensusColour C1861 { get; }
        [ColumnDetail("1871", 20)]
        CensusColour C1871 { get; }
        [ColumnDetail("1881", 20)]
        CensusColour C1881 { get; }
        [ColumnDetail("1891", 20)]
        CensusColour C1891 { get; }
        [ColumnDetail("1901", 20)]
        CensusColour C1901 { get; }
        [ColumnDetail("1911", 20)]
        CensusColour C1911 { get; }
        [ColumnDetail("1939", 20)]
        CensusColour C1939 { get; }

        [ColumnDetail("1790", 20)]
        CensusColour US1790 { get; }
        [ColumnDetail("1800", 20)]
        CensusColour US1800 { get; }
        [ColumnDetail("1810", 20)]
        CensusColour US1810 { get; }
        [ColumnDetail("1820", 20)]
        CensusColour US1820 { get; }
        [ColumnDetail("1830", 20)]
        CensusColour US1830 { get; }
        [ColumnDetail("1840", 20)]
        CensusColour US1840 { get; }
        [ColumnDetail("1850", 20)]
        CensusColour US1850 { get; }
        [ColumnDetail("1860", 20)]
        CensusColour US1860 { get; }
        [ColumnDetail("1870", 20)]
        CensusColour US1870 { get; }
        [ColumnDetail("1880", 20)]
        CensusColour US1880 { get; }
        [ColumnDetail("1890", 20)]
        CensusColour US1890 { get; }
        [ColumnDetail("1900", 20)]
        CensusColour US1900 { get; }
        [ColumnDetail("1910", 20)]
        CensusColour US1910 { get; }
        [ColumnDetail("1920", 20)]
        CensusColour US1920 { get; }
        [ColumnDetail("1930", 20)]
        CensusColour US1930 { get; }
        [ColumnDetail("1940", 20)]
        CensusColour US1940 { get; }

        [ColumnDetail("1901", 20)]
        CensusColour Ire1901 { get; }
        [ColumnDetail("1911", 20)]
        CensusColour Ire1911 { get; }

        [ColumnDetail("1851", 20)]
        CensusColour Can1851 { get; }
        [ColumnDetail("1861", 20)]
        CensusColour Can1861 { get; }
        [ColumnDetail("1871", 20)]
        CensusColour Can1871 { get; }
        [ColumnDetail("1881", 20)]
        CensusColour Can1881 { get; }
        [ColumnDetail("1991", 20)]
        CensusColour Can1891 { get; }
        [ColumnDetail("1901", 20)]
        CensusColour Can1901 { get; }
        [ColumnDetail("1906", 20)]
        CensusColour Can1906 { get; }
        [ColumnDetail("1911", 20)]
        CensusColour Can1911 { get; }
        [ColumnDetail("1916", 20)]
        CensusColour Can1916 { get; }
        [ColumnDetail("1921", 20)]
        CensusColour Can1921 { get; }

        [ColumnDetail("1865", 20)]
        CensusColour V1865 { get; }
        [ColumnDetail("1875", 20)]
        CensusColour V1875 { get; }
        [ColumnDetail("1885", 20)]
        CensusColour V1885 { get; }
        [ColumnDetail("1895", 20)]
        CensusColour V1895 { get; }
        [ColumnDetail("1905", 20)]
        CensusColour V1905 { get; }
        [ColumnDetail("1915", 20)]
        CensusColour V1915 { get; }
        [ColumnDetail("1920", 20)]
        CensusColour V1920 { get; }
        [ColumnDetail("1925", 20)]
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
        decimal Ahnentafel { get; }
    }
}
