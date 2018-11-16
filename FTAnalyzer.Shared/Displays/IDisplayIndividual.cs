using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayIndividual
    {
        [ColumnDetail("Ref", 60)]
        string IndividualID { get; }
        [ColumnDetail("Forenames", 100)]
        string Forenames { get; }
        [ColumnDetail("Surnames", 75)]
        string Surname { get; }
        [ColumnDetail("Gender", 50, ColumnDetail.ColumnAlignment.Center)]
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
        [ColumnDetail("Relation", 115)]
        string Relation { get; }
        [ColumnDetail("Relation to Root", 150)]
        string RelationToRoot { get; }
        [ColumnDetail("Num Marriages", 90, ColumnDetail.ColumnAlignment.Right)]
        int MarriageCount { get; }
        [ColumnDetail("Num Children", 80, ColumnDetail.ColumnAlignment.Right)]
        int ChildrenCount { get; }
        [ColumnDetail("Budgie Code", 90)]
        string BudgieCode { get; }
        [ColumnDetail("Ahnentafel", 70, ColumnDetail.ColumnAlignment.Right)]
        long Ahnentafel { get; }
        [ColumnDetail("Has Notes", 600, ColumnDetail.ColumnAlignment.Center)]
        string HasNotes { get; }
    }
}
