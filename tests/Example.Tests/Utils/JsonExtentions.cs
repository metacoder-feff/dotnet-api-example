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
}