using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;

namespace FEFF.Extentions.Testing;

public class JTokenParseOptions
{
    public JsonLoadSettings? LoadSettings {get; set;} = null;
    public DateParseHandling? DateParseHandling {get; set;} = null;
//TODO:    
    //jsonReader.FloatParseHandling
    //jsonReader.DateTimeZoneHandling
    //jsonReader.DateFormatString
    //jsonReader.Culture
    //jsonReader.QuoteChar
    //jsonReader.SupportMultipleContent
}

public static class JsonExtentions
{
    public static JToken ParseJToken(this string src)
    {
        return JToken.Parse(src);
    }

    public static JToken ParseJToken(this string json, JTokenParseOptions options)
    {
        using var jsonReader = new JsonTextReader(new StringReader(json));
        
        if(options.DateParseHandling != null)
            jsonReader.DateParseHandling = options.DateParseHandling.Value;

        var result = JToken.Load(jsonReader, options.LoadSettings);
        while (jsonReader.Read())
        {
        }

        return result;
    }

//TODO: JTokenParseOptions
    [return: NotNullIfNotNull(nameof(src))]
    public static JToken? ToJToken(this object? src)//, bool parseIfString = true)
    {
        if (src == null)
            return null;

        // if(src is string str && parseIfString)
        //     return str.ParseJToken();

        var o = new JsonSerializerSettings
        {
            Converters = { 
                new Newtonsoft.Json.Converters.StringEnumConverter(), 
                new TimeSpanConverter(), 
                new SystemJsonConverter() 
            },
            DateParseHandling = DateParseHandling.None,
        };
        o.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        var ser = JsonSerializer.CreateDefault(o);

        var js = JToken.FromObject(src, ser);
        return js;
    }

    public static JToken? Sort(this JToken? src, string path)
    {
        if (src == null)
            return null;

//TODO: generalize        
        if (src.SelectToken(path) is not JArray toc)
            return src;

        var nn = toc.OrderBy(x => x.ToString()).ToList();

        for (int i = 0; i < nn.Count; i++)
            toc[i] = nn[i];

        return src;
    }

    public static JToken? ReplaceValue<TValue>(this JToken? src, string path, TValue newValue, bool validateValueType = true)
    {
        if (src == null)
            return null;


        var tt = src.SelectTokens(path)
                    .Where(x => x is JValue)
                    .Select(x => (JValue)x);

        foreach (var t in tt)
        {
            if (validateValueType)
                ValidateValueType(t.Type, typeof(TValue));

            t.Value = newValue;
        }

        return src;
    }

    private static void ValidateValueType(JTokenType src, Type dst)
    {
        ThrowHelper.Assert(src != JTokenType.None);

        var dstCode = Type.GetTypeCode(dst);
        var dstJTokenType = GetJTokenType(dstCode);

        ThrowHelper.Assert(dstJTokenType != JTokenType.None);

        if(src != dstJTokenType)
            throw new InvalidOperationException($"ValidateValueType error: dst={src}, dst={dst}");
    }

    private static JTokenType GetJTokenType(TypeCode dstCode) =>
    dstCode switch
    {
        //
        TypeCode.Byte   or
        TypeCode.SByte  or

        TypeCode.UInt16 or
        TypeCode.UInt32 or
        TypeCode.UInt64 or

        TypeCode.Int16  or
        TypeCode.Int32  or
        TypeCode.Int64  => JTokenType.Integer,

        //
        TypeCode.Single  or
        TypeCode.Double  or
        TypeCode.Decimal => JTokenType.Float,

        //
        TypeCode.String => JTokenType.String,

        //
        _ => JTokenType.None
    };
}

/// <summary>
/// TimeSpans are not serialized consistently depending on what properties are present. So this 
/// serializer will ensure the format is maintained no matter what.
/// </summary>
public class TimeSpanConverter : JsonConverter<TimeSpan>
{
    /// <summary>
    /// Format: Days.Hours:Minutes:Seconds:Milliseconds
    /// </summary>
    // public const string TimeSpanFormatString = @"d\.hh\:mm\:ss\:FFF";

    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        // var timespanFormatted = $"{value.ToString(TimeSpanFormatString)}";
        // writer.WriteValue(timespanFormatted);
        writer.WriteValue(value.ToString());
    }

    public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        // TimeSpan.TryParseExact(reader.Value as string, TimeSpanFormatString, null, out TimeSpan parsedTimeSpan);
        // return parsedTimeSpan;
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convert 'System.Text.Json.Nodes.JsonNode' to 'Newtonsoft.Json.JToken'
/// </summary>
public class SystemJsonConverter : JsonConverter<JsonNode>
{
    public override void WriteJson(JsonWriter writer, JsonNode? value, JsonSerializer serializer)
    {
        if(value == null)
            writer.WriteNull();
        else
        {
//TODO: custom reader without string?
            var str = value.ToString();
            var reader = new JsonTextReader(new StringReader(str))
            {
                DateParseHandling = serializer.DateParseHandling
//TODO: test dates
//TODO: other settings
            };
            writer.WriteToken(reader);
        }
    }

    public override JsonNode? ReadJson(JsonReader reader, Type objectType, JsonNode? existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        // TimeSpan.TryParseExact(reader.Value as string, TimeSpanFormatString, null, out TimeSpan parsedTimeSpan);
        // return parsedTimeSpan;
        throw new NotImplementedException();
    }
}