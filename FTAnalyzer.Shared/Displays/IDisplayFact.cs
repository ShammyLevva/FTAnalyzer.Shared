﻿#if __PC__
using System.Drawing;
#endif

using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayFact : IColumnComparer<IDisplayFact>
    {
#if __PC__
        [ColumnDetail("Icon", 50, ColumnDetail.ColumnAlignment.Center, ColumnDetail.ColumnType.Icon)]
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
        [ColumnDetail("Surname at date", 100)]
        string SurnameAtDate { get; }
        [ColumnDetail("Fact Type", 85)]
        string TypeOfFact { get; }
        [ColumnDetail("Fact Date", 170)]
        FactDate FactDate { get; }
        [ColumnDetail("Relation", 120)]
        string Relation { get; }
        [ColumnDetail("Relation to Root", 160)]    
        string RelationToRoot { get; }
        [ColumnDetail("Location", 250)]
        FactLocation Location { get; }
        [ColumnDetail("Age at Fact", 70)]
        Age AgeAtFact { get; }
#if __PC__
        [ColumnDetail("Location Icon", 50, ColumnDetail.ColumnAlignment.Center, ColumnDetail.ColumnType.Icon)]
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
        [ColumnDetail("Num Sources", 75, ColumnDetail.ColumnAlignment.Right)]
        int SourcesCount { get; }
        [ColumnDetail("Sources", 300)]
        string SourceList { get; }
        [ColumnDetail("Lat", 60)]
        double Latitude { get; }
        [ColumnDetail("Long", 60)]
        double Longitude { get; }
#if __PC__
        [ColumnDetail("Preferred Fact", 100, ColumnDetail.ColumnAlignment.Right)]
        bool Preferred { get; }
        [ColumnDetail("Ignored Fact", 80, ColumnDetail.ColumnAlignment.Right)]
        bool IgnoredFact { get; }
#elif __MACOS__
        [ColumnDetail("Preferred Fact", 100, ColumnDetail.ColumnAlignment.Right)]
        string Preferred { get; }
        [ColumnDetail("Ignored Fact", 80, ColumnDetail.ColumnAlignment.Right)]
        string IgnoredFact { get; }
#endif
        [ColumnDetail("Error Comment", 2500, ColumnDetail.ColumnAlignment.Left)]
        string ErrorComment { get; }
    }
}
