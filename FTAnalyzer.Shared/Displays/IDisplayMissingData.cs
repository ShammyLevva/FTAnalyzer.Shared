using System.Numerics;

namespace FTAnalyzer
{
    public interface IDisplayMissingData
    {
        string IndividualID { get; }
        string Forenames { get; }
        string Surname { get; }
        string Relation { get; }
        string RelationToRoot { get; }

        ColourValues.BMDColours Birth { get; }
        ColourValues.BMDColours BaptChri { get; }
        ColourValues.BMDColours Marriage1 { get; }
        ColourValues.BMDColours Marriage2 { get; }
        ColourValues.BMDColours Marriage3 { get; }
        ColourValues.BMDColours Death { get; }
        ColourValues.BMDColours CremBuri { get; }

        FactDate BirthDate { get; }
        FactDate DeathDate { get; }
        string FirstMarriage { get; }
        string SecondMarriage { get; }
        string ThirdMarriage { get; }
        FactLocation BirthLocation { get; }
        FactLocation DeathLocation { get; }
        FactLocation BestLocation(FactDate when);
        BigInteger Ahnentafel { get; }
        float Score { get; }
    }
}
