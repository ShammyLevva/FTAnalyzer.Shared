using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayDuplicateIndividual
    {
        [ColumnDetail("Ref", 50)]
        string IndividualID { get;}
        [ColumnDetail("Name", 150)]
        string Name { get;}
        [ColumnDetail("Forenames", 100)]
        string Forenames { get; }
        [ColumnDetail("Surname", 75)]
        string Surname { get; }
        [ColumnDetail("Birth Date", 170)]
        FactDate BirthDate { get;}
        [ColumnDetail("Birth Location", 250)]
        FactLocation BirthLocation { get;}
        [ColumnDetail("Match", 50)]
        string MatchIndividualID { get; }
        [ColumnDetail("Match Name", 150)]
        string MatchName { get; }
        [ColumnDetail("Match Birth Date", 170)]
        FactDate MatchBirthDate { get; }
        [ColumnDetail("Match Birth Location", 250)]
        FactLocation MatchBirthLocation { get; }
        [ColumnDetail("Score", 60)]
        int Score { get; }
        [ColumnDetail("Ignore", 75, ColumnDetail.ColumnAlignment.Center, ColumnDetail.ColumnType.CheckBox)]
        bool IgnoreNonDuplicate { get; }
    }
}
