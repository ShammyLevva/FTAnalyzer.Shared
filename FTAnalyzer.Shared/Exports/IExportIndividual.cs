using System;
using System.Numerics;

namespace FTAnalyzer
{
    public interface IExportIndividual : IDisplayIndividual
    {
        new string IndividualID { get; }
        new string Forenames { get; }
        new string Surname { get; }
        string Alias { get; }
        new string Gender { get; }
        new FactDate BirthDate { get; }
        new FactLocation BirthLocation { get; }
        new FactDate DeathDate { get; }
        new FactLocation DeathLocation { get; }
        new string Occupation { get; }
        new Age LifeSpan { get; }
        new string Relation { get; }
        new string BudgieCode { get; }
        new BigInteger Ahnentafel { get; }
        bool HasRangedBirthDate { get; }
        bool HasParents { get; }
        int CensusFactCount { get; }
        string MarriageDates { get; }
        string MarriageLocations { get; }
        new string RelationToRoot { get; set; }
        DateTime BirthStart { get; }
        DateTime BirthEnd { get; }
        DateTime DeathStart { get; }
        DateTime DeathEnd { get; }
        string FamilyIDsAsParent { get; }
        string FamilyIDsAsChild { get; }
        new int ChildrenCount { get; }
        new int MarriageCount { get; }
    }
}
