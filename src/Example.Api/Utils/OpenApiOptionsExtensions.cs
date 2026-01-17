using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Utils.OpenApi;

// see also: MMonrad.OpenApi.NodaTime
public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions ConfigureNodaTime(this OpenApiOptions options)
    {
        // OpenAPI specs:
        // https://spec.openapis.org/registry/format/date-time
        // https://spec.openapis.org/registry/format/date
        // https://spec.openapis.org/registry/format/time
        // based on RFC3339
        // https://www.rfc-editor.org/rfc/rfc3339#section-5.6

        // RFC "date-time"
        // the requires allows time offset or Z
        // ZonedDateTime: RFC does not allow ZONE
        // LocalDateTime: RFC does not allow missing offset
        options.MapToString<Instant>("date-time");
        options.MapToString<OffsetDateTime>("date-time");

        // OpenAPI "time"
        // The time format represents a time as defined by full-time - RFC3339.
        // LocalTime: RFC does not allow missing offset
        options.MapToString<OffsetTime>("time");

        // OpenAPI "date"
        // The date format represents a date as defined by full-date - RFC3339. 
        options.MapToString<LocalDate>("date");

//TODO: other types
        //LocalTime
        //LocalDateTime
        //ZonedDateTime
        //Period
        //Duration
        //Interval
        //DateInterval
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