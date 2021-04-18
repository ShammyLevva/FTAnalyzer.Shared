using FTAnalyzer.Utilities;
using System.Numerics;

namespace FTAnalyzer
{
    public interface IDisplayIndividual : IColumnComparer<IDisplayIndividual>
    {
        [ColumnDetail("Ref", 50)]
        string IndividualID { get; }
        [ColumnDetail("Forenames", 100)]
        string Forenames { get; }
        [ColumnDetail("Surname", 75)]
        string Surname { get; }
        [ColumnDetail("Gender", 40, ColumnDetail.ColumnAlignment.Center)]
        string Gender { get; }
        [ColumnDetail("Birth Date", 170)]
        FactDate BirthDate { get; }
        [ColumnDetail("Birth Location", 250)]
        FactLocation BirthLocation { get; }
        [ColumnDetail("Death Date", 170)]
        FactDate DeathDate { get; }
        [ColumnDetail("Death Location", 250)]
        FactLocation DeathLocation { get; }
        [ColumnDetail("Occupation", 200)]
        string Occupation { get; }
        [ColumnDetail("Lifespan",65)]
        Age LifeSpan { get; }
        [ColumnDetail("Relation", 120)]
        string Relation { get; }
        [ColumnDetail("Relation to Root", 160)]
        string RelationToRoot { get; }
        [ColumnDetail("Title", 75)]
        string Title { get; }
        [ColumnDetail("Suffix", 75)]
        string Suffix { get; }
        [ColumnDetail("Alias", 75)]
        string Alias { get; }
        [ColumnDetail("FamilySearch ID", 120, ColumnDetail.ColumnAlignment.Left, ColumnDetail.ColumnType.LinkCell)]
        string FamilySearchID { get; }
        [ColumnDetail("Marriages", 60, ColumnDetail.ColumnAlignment.Right)]
        int MarriageCount { get; }
        [ColumnDetail("Children", 60, ColumnDetail.ColumnAlignment.Right)]
        int ChildrenCount { get; }
        [ColumnDetail("Budgie Code", 90)]
        string BudgieCode { get; }
        [ColumnDetail("Ahnentafel", 70, ColumnDetail.ColumnAlignment.Right)]
        BigInteger Ahnentafel { get; }
#if __PC__
        [ColumnDetail("Has Notes", 60, ColumnDetail.ColumnAlignment.Center)]
        bool HasNotes { get; }
#elif __MACOS__
        [ColumnDetail("Has Notes", 60, ColumnDetail.ColumnAlignment.Center)]
        string HasNotesMac { get; }
#endif
        [ColumnDetail("Num Facts", 60, ColumnDetail.ColumnAlignment.Right)]
        int FactsCount { get; }
        [ColumnDetail("Num Sources", 60, ColumnDetail.ColumnAlignment.Right)]
        int SourcesCount { get; }
    }
}
