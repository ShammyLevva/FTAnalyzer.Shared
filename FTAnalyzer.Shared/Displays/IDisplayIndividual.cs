using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayIndividual
    {
        [ColumnWidth(60)]
        string IndividualID { get; }
        [ColumnWidth(75)]
        string Forenames { get; }
        [ColumnWidth(100)]
        string Surname { get; }
        [ColumnWidth(50)]
        string Gender { get; }
        [ColumnWidth(150)]
        FactDate BirthDate { get; }
        [ColumnWidth(250)]
        FactLocation BirthLocation { get; }
        [ColumnWidth(150)]
        FactDate DeathDate { get; }
        [ColumnWidth(250)]
        FactLocation DeathLocation { get; }
        [ColumnWidth(200)]
        string Occupation { get; }
        [ColumnWidth(60)]
        Age LifeSpan { get; }
        [ColumnWidth(105)]
        string Relation { get; }
        [ColumnWidth(150)]
        string RelationToRoot { get; }
        [ColumnWidth(60)]
        int MarriageCount { get; }
        [ColumnWidth(60)]
        int ChildrenCount { get; }
        [ColumnWidth(80)]
        string BudgieCode { get; }
        [ColumnWidth(60)]
        long Ahnentafel { get; }
        [ColumnWidth(40)]
        bool HasNotes { get; }
    }
}
