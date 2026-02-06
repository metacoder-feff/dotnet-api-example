using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

//TODO: for single test only
// enable ITestOutputHelper
[assembly: CaptureConsole]

namespace Example.Tests;

public class TetsOpts
{
    //public string? TestString {get; set;}
}

public class ConsoleLoggingTest(ITestOutputHelper testOutputHelper) : ApiTestBase
{
    /// <summary>
    /// Test objectives:
    /// 1. Logging subsystem should:
    /// - write to StdOut
    /// - use JSONL format (JSON per single line)
    /// - include scopes
    /// - use 'Timestamp' format : "yyyy-MM-ddTHH:mm:ss.ffffffZ"
    /// - write 'Timestamp' in UTC
    /// 2. Exceptions at 'app.Run()' should be logged
    /// </summary>
    [Fact]
    public async Task Loging__should__satisfy_a_number_of_objectives()
    {
        // PREPARE
        // this config throws exception  at 'app.Run()'
        AppBuilder.ConfigureServices(
            b => b.AddOptions<TetsOpts>()
                .Validate(_ => false)
                .ValidateOnStart()
        );

        // ACT
        // just start app
        // use try-catch because app cannot be started
        try
        {
            TestApplication.StartServer();
        }
        catch { }


        // ASSERT
        var ll = GetLogLines();

        AssertEveryLineIsJson(ll);

        var line = GetCriticalJson(ll);

        AssertErrorLogContent(line);
    }

    [Fact]
    public async Task Exception__at_TryCreateApp__should_be_logged()
    {
        // PREPARE
        // this config throws exception  at 'builder.Build()'
        AppBuilder.ConfigureServices(
            b => throw new InvalidOperationException("test err")
        );

        // ACT
        // just start app
        // use try-catch because app cannot be started
        try
        {
            TestApplication.StartServer();
        }
        catch { }

        // ASSERT
        var ll = GetLogLines();
        var line = GetCriticalJson(ll);
        AssertTryCreateAppErrorLogContent(line);
    }

    private static void AssertErrorLogContent(JToken line)
    {
        line.Should()
            .ContainSubtree("""
            {
                //"Timestamp": "2000-01-02T03:04:05.123456Z",
                "EventId": 0,
                "LogLevel": "Critical",
                "Category": "Example.Api",
                "Message": "Error at 'TrySetupAndRunApp' stage.",
                //"Exception": "Microsoft.Extensions.Options.OptionsValidationException: A validation error has occurred.\n   at Microsoft.Extensions.Options.OptionsFactory`1.Create(String name)\n   at Microsoft.Extensions.Options.OptionsMonitor`1.<>c.<Get>b__10_0(String name, IOptionsFactory`1 factory)\n   at Microsoft.Extensions.Options.OptionsCache`1.<>c__DisplayClass3_1`1.<GetOrAdd>b__2()\n   at System.Lazy`1.ViaFactory(LazyThreadSafetyMode mode)\n   at System.Lazy`1.ExecutionAndPublication(LazyHelper executionAndPublication, Boolean useDefaultConstructor)\n   at System.Lazy`1.CreateValue()\n   at Microsoft.Extensions.Options.OptionsCache`1.GetOrAdd[TArg](String name, Func`3 createOptions, TArg factoryArgument)\n   at Microsoft.Extensions.Options.OptionsMonitor`1.Get(String name)\n   at Microsoft.Extensions.DependencyInjection.OptionsBuilderExtensions.<>c__DisplayClass0_1`1.<ValidateOnStart>b__1()\n   at Microsoft.Extensions.Options.StartupValidator.Validate()\n--- End of stack trace from previous location ---\n   at Microsoft.Extensions.Options.StartupValidator.Validate()\n   at Microsoft.Extensions.Hosting.Internal.Host.StartAsync(CancellationToken cancellationToken)\n   at Microsoft.Extensions.Hosting.Internal.Host.<StartAsync>g__LogAndRethrow|14_3(<>c__DisplayClass14_0&)\n   at Microsoft.Extensions.Hosting.Internal.Host.StartAsync(CancellationToken cancellationToken)\n   at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(IHost host, CancellationToken token)\n   at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(IHost host, CancellationToken token)\n   at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.Run(IHost host)\n   at Program.<Main>$(String[] args) in /workspaces/dotnet-api-example2/src/Example.Api/Program.cs:line 17",
                //"State": {
                //    "{OriginalFormat}": "Error at 'TrySetupAndRunApp' stage."
                //},
                "Scopes": []
            }
            """);

        //assert Exception exists
        line.Should().HaveElement("Exception").Which.Type.Should().Be(JTokenType.String);
        var exception = line["Exception"]!.Value<string>();
        exception.Should().NotBeNullOrEmpty();

        //assert Timestamp exists
        line.Should().HaveElement("Timestamp").Which.Type.Should().Be(JTokenType.String);
        var timestamp = line["Timestamp"]!.Value<string>();
        timestamp.Should().NotBeNullOrEmpty();

        //assert Timestamp format
        var parsed = DateTimeOffset.TryParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.ffffffZ", null, System.Globalization.DateTimeStyles.None, out var _);
        parsed.Should().BeTrue();
    }


    private static void AssertTryCreateAppErrorLogContent(JToken line)
    {
        line.Should()
            .ContainSubtree("""
            {
                //"Timestamp": "2000-01-02T03:04:05.123456Z",
                "EventId": 0,
                "EventId": 0,
                "LogLevel": "Critical",
                "Category": "TryCreateApp",
                "Message": "Error at 'TryCreateApp' stage.",
                //"Exception": "System.InvalidOperationException: test err\n   at Example.Tests.ConsoleLoggingTest.<>c.<Exception__at_TryCreateApp__should_be_logged>b__2_0(IServiceCollection b) in /workspaces/dotnet-api-example2/tests/Example.Tests.Console/ConsoleLoggingTest.cs:line 25\n   at Microsoft.Extensions.Hosting.HostApplicationBuilder.HostBuilderAdapter.ApplyChanges()\n   at Microsoft.Extensions.Hosting.HostApplicationBuilder.Build()\n   at Microsoft.AspNetCore.Builder.WebApplicationBuilder.Build()\n   at Program.<<Main>$>g__TryCreateApp|0_0(String[] args) in /workspaces/dotnet-api-example2/src/Example.Api/Program.cs:line 42",
                "State": {
                    "{OriginalFormat}": "Error at 'TryCreateApp' stage."
                },
                "Scopes": []
            }
            """);

        //assert Exception
        line.Should().HaveElement("Exception").Which.Type.Should().Be(JTokenType.String);
        var exception = line["Exception"]!.Value<string>();
        exception.Should().StartWith("System.InvalidOperationException: test err");
    }

    private static JToken GetCriticalJson(string[] ll)
    {
        var l = ll.SingleOrDefault(x => x.Contains("Critical"));
        l.Should().NotBeNullOrEmpty();

        var line = l
            .ParseJToken(new JTokenParseOptions
            {
                DateParseHandling = Newtonsoft.Json.DateParseHandling.None,
                LoadSettings = new JsonLoadSettings
                {
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
                    LineInfoHandling = LineInfoHandling.Ignore,
                    CommentHandling = CommentHandling.Ignore
                }
            });
        return line;
    }

    private static void AssertEveryLineIsJson(string[] ll)
    {
        ll.Should().AllSatisfy(
            x => x.Should().BeValidJson()
        );
    }

    private string[] GetLogLines()
    {
        var str = testOutputHelper.Output;
        str.Should().NotBeNullOrEmpty();

        var ll = str.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        ll.Should().NotBeEmpty();
        return ll;
    }
}