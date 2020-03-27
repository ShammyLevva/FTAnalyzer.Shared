using Newtonsoft.Json;

namespace FTAnalyzer.Exports
{
    [JsonObject(MemberSerialization.OptIn)]
    public interface IJsonIndividual
    {
        [JsonProperty]
        string IndividualID { get; }
        [JsonProperty]
        string Forenames { get; }
        [JsonProperty]
        string Surname { get; }
        [JsonProperty]
        string Alias { get; }
        [JsonProperty]
        string Gender { get; }
        [JsonProperty, JsonConverter(typeof(JsonFactDateConverter))]
        FactDate BirthDate { get; }
        [JsonProperty]
        FactLocation BirthLocation { get; }
        [JsonProperty, JsonConverter(typeof(JsonFactDateConverter))]
        FactDate DeathDate { get; }
        [JsonProperty]
        FactLocation DeathLocation { get; }
        [JsonProperty]
        string Occupation { get; }

    }
}
