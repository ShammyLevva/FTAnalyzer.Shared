#if __PC__
using System.Drawing;
#endif

using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayFact
    {
#if __PC__
        Image Icon { get; }
#endif
        [ColumnDetail("Ref", 60)]
        string IndividualID { get; }
        [ColumnDetail("Surname", 75)]
        string Surname { get; }
        [ColumnDetail("Forenames", 100)]
        string Forenames { get; }
        [ColumnDetail("Birth Date", 170)]
        FactDate DateofBirth { get; }
        [ColumnDetail("Surname at date", 75)]
        string SurnameAtDate { get; }
        [ColumnDetail("Fact Type", 85)]
        string TypeOfFact { get; }
        [ColumnDetail("Fact Date", 170)]
        FactDate FactDate { get; }
        [ColumnDetail("Relation", 115)]
        string Relation { get; }
        [ColumnDetail("Relation to Root", 150)]
        string RelationToRoot { get; }
        [ColumnDetail("Location", 250)]
        FactLocation Location { get; }
        [ColumnDetail("Age at Fact", 70)]
        Age AgeAtFact { get; }
#if __PC__
        Image LocationIcon { get; }
#endif
        [ColumnDetail("Geocode Status", 150)]
        string GeocodeStatus { get; }
        [ColumnDetail("Found Location", 250)]
        string FoundLocation { get; }
        [ColumnDetail("Found Result Type", 130)]
        string FoundResultType { get; }
        [ColumnDetail("Census Reference", 250)]
        CensusReference CensusReference { get; }
        [ColumnDetail("Census Ref. Year", 100)]
        string CensusRefYear { get; }
        [ColumnDetail("Comments", 250)]
        string Comment { get; }
        [ColumnDetail("Sources", 300)]
        string SourceList { get; }
        [ColumnDetail("Lat", 60)]
        double Latitude { get; }
        [ColumnDetail("Long", 60)]
        double Longitude { get; }
        [ColumnDetail("Preferred Fact", 70, ColumnDetail.ColumnAlignment.Right)]
        string Preferred { get; }
        [ColumnDetail("Ignored Fact", 70, ColumnDetail.ColumnAlignment.Right)]
        string IgnoredFact { get; }
    }
}
