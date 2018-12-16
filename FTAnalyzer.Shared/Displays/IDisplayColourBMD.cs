using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayColourBMD
    {
        [ColumnDetail("Ref", 60)]
        string IndividualID { get; }
        [ColumnDetail("Forenames", 100)]
        string Forenames { get; }
        [ColumnDetail("Surname", 75)]
        string Surname { get; }
        [ColumnDetail("Relation", 120)]
        string Relation { get; }
        [ColumnDetail("Relation to Root", 160)]
        string RelationToRoot { get; }
        
        [ColumnDetail("Birth", 60)]
        ColourValues.BMDColour Birth { get; }
        [ColumnDetail("Baptism", 60)]
        ColourValues.BMDColour BaptChri { get; }
        [ColumnDetail("Marriage 1", 60)]
        ColourValues.BMDColour Marriage1 { get; }
        [ColumnDetail("Marriage 2", 60)]
        ColourValues.BMDColour Marriage2 { get; }
        [ColumnDetail("Marriage 3", 60)]
        ColourValues.BMDColour Marriage3 { get; }
        [ColumnDetail("Death", 60)]
        ColourValues.BMDColour Death { get; }
        [ColumnDetail("Burial", 60)]
        ColourValues.BMDColour CremBuri { get; }

        [ColumnDetail("Birth date", 170)]
        FactDate BirthDate { get; }
        [ColumnDetail("Death date", 170)]
        FactDate DeathDate { get; }
        [ColumnDetail("First Marriage", 200)]
        string FirstMarriage { get; }
        [ColumnDetail("Second Marriage", 200)]
        string SecondMarriage { get; }
        [ColumnDetail("Third Marriage", 200)]
        string ThirdMarriage { get; }
        [ColumnDetail("Birth Location", 150)]
        FactLocation BirthLocation { get; }
        [ColumnDetail("Death Location", 150)]
        FactLocation DeathLocation { get; }
        [ColumnDetail("Best Location", 150)]
        FactLocation BestLocation(FactDate when);
        [ColumnDetail("Ahnentafel", 70, ColumnDetail.ColumnAlignment.Right)]
        decimal Ahnentafel { get; }
    }
}
