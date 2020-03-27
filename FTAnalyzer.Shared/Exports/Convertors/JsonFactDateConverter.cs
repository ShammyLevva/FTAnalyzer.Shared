using Newtonsoft.Json;
using System;

namespace FTAnalyzer.Exports
{
    public class JsonFactDateConverter : JsonConverter<FactDate>
    {
        public override FactDate ReadJson(JsonReader reader, Type objectType, FactDate existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, FactDate value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("DateString");
            writer.WriteValue(value.DateString);
            writer.WritePropertyName("StartDate");
            writer.WriteValue(value.StartDate.ToString("yyyy/MM/dd"));
            writer.WritePropertyName("EndDate");
            writer.WriteValue(value.EndDate.ToString("yyyy/MM/dd"));
            writer.WriteEndObject();
        }
    }
}
