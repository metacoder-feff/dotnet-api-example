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

        // RFC for "date-time" requires 'time-offset' or 'Z'
        // ZonedDateTime: RFC does not allow ZONE
        options.MapToString<Instant>("date-time");
        options.MapToString<OffsetDateTime>("date-time");
        
        // https://spec.openapis.org/registry/format/date-time-local.html
        options.MapToString<LocalDateTime>("date-time-local");

        // The time format represents a time as defined by full-time - RFC3339.
        options.MapToString<OffsetTime>("time");

        // https://spec.openapis.org/registry/format/time-local.html
        options.MapToString<LocalTime>("time-local");

        // The date format represents a date as defined by full-date - RFC3339.
        // OffsetDate: RFC does not allow to add offset to a date
        // it is already localized
        options.MapToString<LocalDate>("date");

        // OpenAPI "duration"
        // https://spec.openapis.org/registry/format/duration
        // The duration format represents a duration as defined by duration - RFC3339.
        // https://www.rfc-editor.org/rfc/rfc3339.html#appendix-A
        options.MapToString<Period>("duration");

//TODO: other types
        //Offset
        //OffsetDate
        //ZonedDateTime
        //Duration
        //Interval
        //DateInterval

        // HINTS:

        // TimeSpan is mapped inlined as:
        //   "ts": {
        //     "pattern": "^-?(\\d+\\.)?\\d{2}:\\d{2}:\\d{2}(\\.\\d{1,7})?$",
        //     "type": "string"
        //   },

        // Duration is serialized without 'Day'
        //   "dr": "511:55:11.999999999",

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