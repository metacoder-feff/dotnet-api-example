using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace FEFF.Extentions.OpenApi.NodaTime;

// see also: MMonrad.OpenApi.NodaTime
//TODO: add complex tests
public static class OpenApiOptionsExtensions
{
    /// <summary>
    /// Adds OpenAPI schema for 'NodaTime' types serialized by custom 'JsonConverter'.
    /// <list type="bullet">
    ///     <item>
    ///         <description>Well known OpenAPI formats e.g. 'date-time' are inlined as string+format.</description>
    ///     </item>
    ///     <item>
    ///         <description>For all other types create 'Refs' containig description, pattern and example.</description>
    ///     </item>
    ///     <item>
    ///         <description>Using minimal-api JsonSerializerOptions (see ConfigureHttpJsonOptions) including 'NamingPolicy'.</description>
    ///     </item>
    /// </list>
    /// </summary>
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

        // Other string-like types are not inlined because they have no standard format in OpenAPI specs

        // not fully compatible with TimeSpan (see days)
        options.MapDuration();
        options.MapZonedDateTime();

        //examples
        var offset = Offset.FromHoursAndMinutes(-3, 30);
        var offsetDate = new OffsetDate(new LocalDate(2000, 01, 01), offset);
        var annualDate = new AnnualDate(10, 05);

//TODO: add noda-pattern to description
//TODO: schema.Pattern
        options.MapRefString<Offset>(
            "An offset from UTC in seconds. A positive value means that the local time is ahead of UTC (e.g. for Europe); a negative value means that the local time is behind UTC (e.g. for America).",
            null,
            offset
            );

//TODO: add noda-pattern to description
//TODO: schema.Pattern
        options.MapRefString<OffsetDate>(
            "'date' (local) + Offset",
            null,
            offsetDate
            );

//TODO: add noda-pattern to description
//TODO: schema.Pattern
        options.MapRefString<AnnualDate>(
            "Represents an annual date (month and day) in the ISO calendar but without a specific year, typically for recurrent events such as birthdays, anniversaries, and deadlines.",
            null,
            annualDate
            );

        // complex types
//TODO: different converters
        options.MapInterval();
        options.MapDateInterval();

//TODO: DateTimeZone

        return options;
    }

    // Do not inline because OpenApi has no format for this type.
    // Add description instead.
    private static void MapDuration(this OpenApiOptions options)
    {
        // HINTS:
        // 1. Timespan 'Day' delimiter is '.' 
        //
        // 2. Duration can store nanoseconds - has more digits in fraction
        //    so they are not interchangable
        //
        // 3. TimeSpan is mapped inlined as:
        //   "ts": {
        //     "pattern": "^-?(\\d+\\.)?\\d{2}:\\d{2}:\\d{2}(\\.\\d{1,7})?$",
        //     "type": "string"
        //   },
        // 4. Duration C# regexp is:
        //      private const string DurationPattern = @"-?[0-9]{1,8}:[0-9]{2}:[0-9]{2}:[0-9]{2}(\.[0-9]{1,9})?";
        //
        // 5. Duration is serialized without 'Day'
        //   "dr": "511:55:11.999999999",
        //    j: Round-trip pattern used by NodaTime.Serialization.JsonNet, which always uses the invariant culture and a pattern string of -H:mm:ss.FFFFFFFFF
        //    https://nodatime.org/3.2.x/userguide/duration-patterns#:~:text=This%20is%20the%20default%20format%20pattern.%20j,invariant%20culture%20and%20a%20pattern%20string%20of


        var example = Duration.FromHours(-123.333);

        options.MapRefString(
            "An elapsed time measured in nanoseconds. Format: '-H:mm:ss.FFFFFFFFF'",
            "^-?\\d*:\\d{2}:\\d{2}(\\.\\d{1,9})?$",
            example
            );
    }

    internal static void MapZonedDateTime(this OpenApiOptions options)
    {
//TODO: add noda-pattern to description
//TODO: schema.Pattern
//TODO: better example logic??
        var zonedDateTime = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Africa/Johannesburg")
                    ?.AtLeniently(new LocalDateTime(2000, 01, 01, 12, 13, 14, 100));

        if(zonedDateTime.HasValue)
        {
            options.MapRefString(
                "A ZonedDateTime is a LocalDateTime within a specific time zone - with the added information of the exact Offset, in case of ambiguity. (During daylight saving transitions, the same local date/time can occur twice.)",
                null,
                zonedDateTime.Value
                );
        }
        else
        {
            string? exStr = null;// "2000-01-01T12:13:14.1+02 Africa/Johannesburg"
            options.MapRefString<ZonedDateTime>(
                "A ZonedDateTime is a LocalDateTime within a specific time zone - with the added information of the exact Offset, in case of ambiguity. (During daylight saving transitions, the same local date/time can occur twice.)",
                null,
                exStr
                );
        }
    }

    private static void MapInterval(this OpenApiOptions options)
    {
        options.MapInterval<Interval>(
            nameof(Interval.Start), 
            nameof(Interval.End),
            "Represents a time interval between two 'date-time' (with zone information) values, expressed with start and end.",
            "date-time"
        );
    }

    private static void MapDateInterval(this OpenApiOptions options)
    {
        options.MapInterval<DateInterval>(
            nameof(DateInterval.Start), 
            nameof(DateInterval.End),
            "Represents a time interval between two 'date' (without zone information) values, expressed with start and end.",
            "date"
        );
    }

    private static void MapInterval<T>(this OpenApiOptions options, string nameStart, string nameEnd, string description, string format)
    {
//TODO: force invoke defult schema creator inspite of custom JsonConverter ??
//TODO: different converters ??
        options.AddSingleSchemaTransformer<T>((schema, context) =>
        {
            var js = context.ApplicationServices.GetService<IOptions<JsonOptions>>();
            var nn = js?.Value?.SerializerOptions?.PropertyNamingPolicy;

            if (nn != null)
            {
                nameStart = nn.ConvertName(nameStart);
                nameEnd = nn.ConvertName(nameEnd);
            }

            schema.Type = JsonSchemaType.Object;
            schema.Description = description;
            schema.Properties = new Dictionary<string, IOpenApiSchema>
            {
                {
                    nameStart,
                    new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = format,
                    }
                },
                {
                    nameEnd,
                    new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = format,
                    }
                }
            };
            AddNodaExternalDocs(schema);
        });
    }
    
    internal static void MapRefString<T>(this OpenApiOptions options, string description, string? pattern, string? example)
    {
        options.AddSingleSchemaTransformer<T>((schema, _) =>
        {
            SetRefString(schema, description, pattern, example);
        });
    }

    internal static OpenApiOptions MapRefString<T>(this OpenApiOptions options, string description, string? pattern, T example)
    {
        return options.AddSingleSchemaTransformer<T>((schema, ctx) =>
        {
            var js = ctx.ApplicationServices.GetService<IOptions<JsonOptions>>()
                    ?.Value
                    ?.SerializerOptions;

            var exStr = JsonSerializer.Serialize(example, js);
            if (exStr.StartsWith('"') && exStr.EndsWith('"'))
                exStr = exStr.Substring(1, exStr.Length - 2);

            SetRefString(schema, description, pattern, exStr);
        });
    }

    private static void SetRefString(OpenApiSchema schema, string description, string? pattern, string? example)
    {
        schema.Type = JsonSchemaType.String;
        schema.Description = description;
        schema.Pattern = pattern;
        schema.Example = example;

        AddNodaExternalDocs(schema);
    }

    private static void AddNodaExternalDocs(OpenApiSchema schema)
    {
        schema.ExternalDocs = new OpenApiExternalDocs()
        {
            Description = "Noda Time: Core types quick reference",
            Url = new Uri("https://nodatime.org/userguide/core-types"),
        };
    }

    // Use builtin primitive 'string' instead of ref to a new type.
    // Description and example of a type "T" are parts of OpenApi spec and are defined by "format" parameter
    public static OpenApiOptions MapInlinedString<T>(this OpenApiOptions options, string format)
    {
        return options.AddSingleSchemaTransformer<T>((schema, _) =>
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = format;
            schema.Metadata?["x-schema-id"] = ""; // set inlined
        });
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
        var type = typeof(T);
        var type1 = Nullable.GetUnderlyingType(type) ?? type;

        var jsonType = context.JsonTypeInfo.Type;
        var type2 = Nullable.GetUnderlyingType(jsonType) ?? jsonType;
        
        return type1 == type2;
    }
}

//TODO: check converter
/*
NodaTime default converters:
https://github.com/nodatime/nodatime.serialization/blob/main/src/NodaTime.Serialization.SystemTextJson/NodaJsonSettings.cs

    internal void AddConverters(IList<JsonConverter> converters)
    {
        MaybeAdd(InstantConverter);
        MaybeAdd(IntervalConverter);
        MaybeAdd(LocalDateConverter);
        MaybeAdd(LocalDateTimeConverter);
        MaybeAdd(LocalTimeConverter);
        MaybeAdd(AnnualDateConverter);
        MaybeAdd(DateIntervalConverter);
        MaybeAdd(OffsetConverter);
        MaybeAdd(DateTimeZoneConverter);
        MaybeAdd(DurationConverter);
        MaybeAdd(PeriodConverter);
        MaybeAdd(OffsetDateTimeConverter);
        MaybeAdd(OffsetDateConverter);
        MaybeAdd(OffsetTimeConverter);
        MaybeAdd(ZonedDateTimeConverter);

        void MaybeAdd(JsonConverter converter)
        {
            if (converter is not null)
            {
                converters.Add(converter);
            }
        }
    }

NodaTime optional converters:
https://github.com/nodatime/nodatime.serialization/blob/main/src/NodaTime.Serialization.SystemTextJson/Extensions.cs

...
            ReplaceExistingConverters<Interval>(options.Converters, NodaConverters.IsoIntervalConverter);
...
            ReplaceExistingConverters<DateInterval>(options.Converters, NodaConverters.IsoDateIntervalConverter);

*/