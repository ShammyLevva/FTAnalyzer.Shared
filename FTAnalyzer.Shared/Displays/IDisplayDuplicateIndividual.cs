using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayDuplicateIndividual
    {
        [ColumnDetail("Ignore", 75, ColumnDetail.ColumnAlignment.Center, ColumnDetail.ColumnType.CheckBox)]
        bool IgnoreNonDuplicate { get; }
        [ColumnDetail("Score", 120)]
        int Score { get; }
        [ColumnDetail("Ref", 100)]
        string IndividualID { get;}
        [ColumnDetail("Name", 250)]
        string Name { get;}
        [ColumnDetail("Forenames", 200)]
        string Forenames { get; }
        [ColumnDetail("Surname", 150)]
        string Surname { get; }
        [ColumnDetail("Birth Date", 225)]
        FactDate BirthDate { get;}
        [ColumnDetail("Birth Location", 400)]
        FactLocation BirthLocation { get;}
        [ColumnDetail("Gender", 100)]
        string Gender { get; }
        [ColumnDetail("Match", 125)]
        string MatchIndividualID { get; }
        [ColumnDetail("Match Name", 250)]
        string MatchName { get; }
        [ColumnDetail("Match Birth Date", 225)]
        FactDate MatchBirthDate { get; }
        [ColumnDetail("Match Birth Location", 400)]
        FactLocation MatchBirthLocation { get; }
        [ColumnDetail("Match Gender", 100)]
        string MatchGender { get; }
    }
}
