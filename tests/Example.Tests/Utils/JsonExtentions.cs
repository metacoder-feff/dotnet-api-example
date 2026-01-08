using Newtonsoft.Json.Linq;

namespace Utils.Testing;

public static class JsonExtentions
{
    public static JToken ParseJToken(this string src)
    {
        return JToken.Parse(src);
    }
}