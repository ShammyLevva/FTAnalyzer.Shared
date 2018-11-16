using System;
using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayLooseBirth : IComparable<Individual>
    {
        [ColumnDetail("Ref", 60)]
        string IndividualID { get; }
        [ColumnDetail("Forenames", 100)]
        string Forenames { get; }
        [ColumnDetail("Surnames", 75)]
        string Surname { get; }
        [ColumnDetail("Birth Date", 170)]
        FactDate BirthDate { get; }
        [ColumnDetail("Birth Location", 250)]
        FactLocation BirthLocation { get; }
        [ColumnDetail("Can be updated to", 300)]
        string LooseBirth { get; }
    }
}
