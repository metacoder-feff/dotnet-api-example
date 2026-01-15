using Newtonsoft.Json;

namespace Utils.Testing;

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