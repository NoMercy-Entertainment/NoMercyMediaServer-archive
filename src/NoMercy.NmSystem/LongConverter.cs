using Newtonsoft.Json;

namespace NoMercy.NmSystem;
public class LongConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(long) || objectType == typeof(long?);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is long longValue)
        {
            writer.WriteValue(longValue);
        }
        else
        {
            writer.WriteNull();
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonToken.Integer)
        {
            return Convert.ToInt64(reader.Value);
        }

        if (reader.TokenType == JsonToken.String && long.TryParse((string)reader.Value, out long result))
        {
            return result;
        }

        return null;
    }
}