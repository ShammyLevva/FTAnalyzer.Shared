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
        [ColumnWidth(60)]
        string IndividualID { get; }
        [ColumnWidth(75)]
        string Surname { get; }
        [ColumnWidth(100)]
        string Forenames { get; }
        [ColumnWidth(150)]
        FactDate DateofBirth { get; }
        [ColumnWidth(75)]
        string SurnameAtDate { get; }
        [ColumnWidth(85)]
        string TypeOfFact { get; }
        [ColumnWidth(150)]
        FactDate FactDate { get; }
        [ColumnWidth(105)]
        string Relation { get; }
        [ColumnWidth(150)]
        string RelationToRoot { get; }
        [ColumnWidth(250)]
        FactLocation Location { get; }
        [ColumnWidth(50)]
        Age AgeAtFact { get; }
#if __PC__
        Image LocationIcon { get; }
#endif
        [ColumnWidth(150)]
        string GeocodeStatus { get; }
        [ColumnWidth(250)]
        string FoundLocation { get; }
        [ColumnWidth(100)]
        string FoundResultType { get; }
        [ColumnWidth(250)]
        CensusReference CensusReference { get; }
        [ColumnWidth(100)]
        string CensusRefYear { get; }
        [ColumnWidth(250)]
        string Comment { get; }
        [ColumnWidth(300)]
        string SourceList { get; }
        [ColumnWidth(60)]
        double Latitude { get; }
        [ColumnWidth(60)]
        double Longitude { get; }
        [ColumnWidth(40)]
        bool Preferred { get; }
        [ColumnWidth(40)]
        bool IgnoreFact { get; }
    }
}
