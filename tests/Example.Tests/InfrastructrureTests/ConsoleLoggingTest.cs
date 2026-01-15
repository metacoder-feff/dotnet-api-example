using Microsoft.Extensions.DependencyInjection;


//TODO: for single test only
// enable ITestOutputHelper
//[assembly: CaptureConsole]

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
    [Fact(
        Skip = "need '[assembly: CaptureConsole]'"
    )]
    public async Task Loging_should_satisfy_some_objectives()
    {
        // PREPARE
        // this config throws exception  at 'app.Run()'
        _appFactory.BuilderOverrider.ConfigureServices(
            b => b.AddOptions<TetsOpts>()
                .Validate(_ => false)
                .ValidateOnStart()
        );

        // ACT
        // just start app
        // use try-catch because app cannot be started
        try
        {
            _appFactory.StartServer();
        }
        catch { }


        // ASSERT
        var ll = GetLogLines();

        AssertEveryLineIsJson(ll);

        var line = GetErrorLogJson(ll);

        AssertErrorLogContent(line);
    }

    private static void AssertErrorLogContent(JToken line)
    {
        line.Should()
            .ContainSubtree("""
            {
                //"Timestamp": "2026-01-15T08:45:08.867427Z",
                "EventId": 0,
                "LogLevel": "Critical",
                "Category": "Example.Api",
                "Message": "Error at 'App Setup/Run' stage.",
                //"Exception": "Microsoft.Extensions.Options.OptionsValidationException: A validation error has occurred.\n   at Microsoft.Extensions.Options.OptionsFactory`1.Create(String name)\n   at Microsoft.Extensions.Options.OptionsMonitor`1.<>c.<Get>b__10_0(String name, IOptionsFactory`1 factory)\n   at Microsoft.Extensions.Options.OptionsCache`1.<>c__DisplayClass3_1`1.<GetOrAdd>b__2()\n   at System.Lazy`1.ViaFactory(LazyThreadSafetyMode mode)\n   at System.Lazy`1.ExecutionAndPublication(LazyHelper executionAndPublication, Boolean useDefaultConstructor)\n   at System.Lazy`1.CreateValue()\n   at Microsoft.Extensions.Options.OptionsCache`1.GetOrAdd[TArg](String name, Func`3 createOptions, TArg factoryArgument)\n   at Microsoft.Extensions.Options.OptionsMonitor`1.Get(String name)\n   at Microsoft.Extensions.DependencyInjection.OptionsBuilderExtensions.<>c__DisplayClass0_1`1.<ValidateOnStart>b__1()\n   at Microsoft.Extensions.Options.StartupValidator.Validate()\n--- End of stack trace from previous location ---\n   at Microsoft.Extensions.Options.StartupValidator.Validate()\n   at Microsoft.Extensions.Hosting.Internal.Host.StartAsync(CancellationToken cancellationToken)\n   at Microsoft.Extensions.Hosting.Internal.Host.<StartAsync>g__LogAndRethrow|14_3(<>c__DisplayClass14_0&)\n   at Microsoft.Extensions.Hosting.Internal.Host.StartAsync(CancellationToken cancellationToken)\n   at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(IHost host, CancellationToken token)\n   at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(IHost host, CancellationToken token)\n   at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.Run(IHost host)\n   at Program.<Main>$(String[] args) in /workspaces/dotnet-api-example2/src/Example.Api/Program.cs:line 17",
                //"State": {
                //    "{OriginalFormat}": "Error at 'App Setup/Run' stage."
                //},
                "Scopes": []
            }
            """);

        line.Should().HaveElement("Timestamp").Which.Type.Should().Be(JTokenType.String);
        line.Should().HaveElement("Exception").Which.Type.Should().Be(JTokenType.String);

        var timestamp = line["Timestamp"]!;
        var exception = line["Exception"]!;

        timestamp.Value<string>().Should().NotBeNullOrEmpty();
        exception.Value<string>().Should().NotBeNullOrEmpty();

//TODO: assert Timestamp format
        // var timestr = line["Timestamp"]!.Value<string>();
    }

    private static JToken GetErrorLogJson(string[] ll)
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