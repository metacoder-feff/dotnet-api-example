using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
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
        options.MapInlinedString<Instant>("date-time");
        options.MapInlinedString<OffsetDateTime>("date-time");

        // https://spec.openapis.org/registry/format/date-time-local.html
        options.MapInlinedString<LocalDateTime>("date-time-local");

        // The time format represents a time as defined by full-time - RFC3339.
        options.MapInlinedString<OffsetTime>("time");

        // https://spec.openapis.org/registry/format/time-local.html
        options.MapInlinedString<LocalTime>("time-local");

        // The date format represents a date as defined by full-date - RFC3339.
        // OffsetDate: RFC does not allow to add offset to a date
        // it is already localized
        options.MapInlinedString<LocalDate>("date");

        // OpenAPI "duration"
        // https://spec.openapis.org/registry/format/duration
        // The duration format represents a duration as defined by duration - RFC3339.
        // https://www.rfc-editor.org/rfc/rfc3339.html#appendix-A
        options.MapInlinedString<Period>("duration");
        
        //TODO: refactor?
        MapInterval(options);

        //TODO: other types
        //Offset
        //OffsetDate
        //ZonedDateTime
        //Duration
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


    private static void MapInterval(OpenApiOptions options)
    {
        options.AddSingleSchemaTransformer<Interval>((schema, context) =>
        {
            var js = context.ApplicationServices.GetService<IOptions<JsonOptions>>();
            var nn = js?.Value?.SerializerOptions?.PropertyNamingPolicy;

            var nStart = nameof(Interval.Start);
            var nEnd = nameof(Interval.End);
            if(nn != null)
            {
                nStart = nn.ConvertName(nStart);
                nEnd = nn.ConvertName(nEnd);
            }

            schema.Type = JsonSchemaType.Object;
            schema.Description = "Represents a time interval between two 'date-time' values, expressed with start and end.";
            schema.Properties = new Dictionary<string, IOpenApiSchema>
            {
                {
                    nameof(Interval.Start),
                    new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "date-time",
                    }
                },
                {
                    nameof(Interval.End),
                    new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "date-time",
                    }
                }
            };
        });
    }

    // Use builtin primitive 'string' instead of ref to a new type
    public static OpenApiOptions MapInlinedString<T>(this OpenApiOptions options, string format)
    {
        options.AddSingleSchemaTransformer<T>((schema, _) =>
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = format;
            schema.Metadata?["x-schema-id"] = ""; // set inlined
        });
        return options;
    }
    public static OpenApiOptions AddSingleSchemaTransformer<T>(this OpenApiOptions options, Action<OpenApiSchema, OpenApiSchemaTransformerContext> action)
    {
        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (IsCurrentType<T>(context) == false)
                return Task.CompletedTask;
            
            action(schema, context);

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