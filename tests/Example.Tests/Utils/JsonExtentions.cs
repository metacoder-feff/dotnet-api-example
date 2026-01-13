using Newtonsoft.Json;

namespace Utils.Testing;

public static class JsonExtentions
{
    public static JToken ParseJToken(this string src)
    {
        return JToken.Parse(src);

        // return ParseJToken2(src);
    }

    public static JToken ParseJToken2(string json)
    {
        using JsonReader jsonReader = new JsonTextReader(new StringReader(json));
        jsonReader.DateParseHandling = DateParseHandling.None;
        //jsonReader.FloatParseHandling = FloatParseHandling.Double;
        //jsonReader.DateTimeZoneHandling
        //jsonReader.DateFormatString
        //jsonReader.Culture
        //jsonReader.QuoteChar
        //jsonReader.SupportMultipleContent

        var settings = new JsonLoadSettings()
        {
            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
            LineInfoHandling = LineInfoHandling.Ignore,
            CommentHandling = CommentHandling.Ignore
        };

        JToken result = JToken.Load(jsonReader, settings);
        while (jsonReader.Read())
        {
        }

        return result;
    }
}