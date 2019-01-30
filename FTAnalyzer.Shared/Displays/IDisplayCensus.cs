using System.Numerics;
using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayCensus : IColumnComparer<IDisplayCensus>
    {
        [ColumnDetail("Family", 50)]
        string FamilyID { get; }
        [ColumnDetail("Position", 50)]
        int Position { get; }
        [ColumnDetail("Ref", 50)]
        string IndividualID { get; }
        [ColumnDetail("Likely Location", 250)]
        FactLocation CensusLocation { get; }
        [ColumnDetail("Name on Census", 250)]
        string CensusName { get; }
        [ColumnDetail("Age", 65)]
        Age Age { get; }
        [ColumnDetail("Occupation", 150)]
        string Occupation { get; }
        [ColumnDetail("Birth Date", 170)]
        FactDate BirthDate { get; }
        [ColumnDetail("Birth Location", 250)]
        FactLocation BirthLocation { get; }
        [ColumnDetail("Death Date", 170)]
        FactDate DeathDate { get; }
        [ColumnDetail("Death Location", 250)]
        FactLocation DeathLocation { get; }
        [ColumnDetail("Census", 75)]
        string Census { get; }
        [ColumnDetail("Census Status", 150)]
        string CensusStatus { get; }
        [ColumnDetail("Census Reference", 300)]
        string CensusRef { get; }
        [ColumnDetail("Relation", 120)]
        string Relation { get; }
        [ColumnDetail("Relation to Root", 160)]
        string RelationToRoot { get; }
        [ColumnDetail("Ahnentafel", 70, ColumnDetail.ColumnAlignment.Right)]
        BigInteger Ahnentafel { get; }
    }
}
