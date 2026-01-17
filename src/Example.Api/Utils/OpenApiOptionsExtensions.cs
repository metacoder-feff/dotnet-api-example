using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Utils.OpenApi;

// see also: MMonrad.OpenApi.NodaTime
public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions ConfigureNodaTime(this OpenApiOptions options)
    {
        options.MapToString<Instant>("date-time");
        options.MapToString<LocalDate>("date");
        //todo: continue

        return options;
    }

    // Use builtin primitive 'string' instead of ref to a new type
    public static OpenApiOptions MapToString<T>(this OpenApiOptions options, string format)
    {
        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (IsCurrentType<T>(context) == false)
                return Task.CompletedTask;
            
            schema.Type =  JsonSchemaType.String;
            schema.Format = format;
            schema.Metadata?["x-schema-id"] = "";

            return Task.CompletedTask;
        });
        return options;
    }

    private static bool IsCurrentType<T>(OpenApiSchemaTransformerContext context)
    {
        var jsonType = context.JsonTypeInfo.Type;
        var type = Nullable.GetUnderlyingType(jsonType) ?? jsonType;
        return type == typeof(T);
    }
}