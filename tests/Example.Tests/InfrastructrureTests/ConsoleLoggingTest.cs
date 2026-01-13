//TODO: for single test only
// enable ITestOutputHelper
//[assembly: CaptureConsole]

namespace Example.Tests;

public class ConsoleLoggingTest(ITestOutputHelper testOutputHelper) : ApiTestBase
{
    [Fact(
        Skip = "need '[assembly: CaptureConsole]'"
    )]
    public async Task Logs_should_be_written_to_stdout_in_jsonl()
    {
        // ACT
        // just start app
        _appFactory.StartServer();
        // Console.WriteLine("xxxx");
        
        // ASSERT
        var str = testOutputHelper.Output;
        str.Should().NotBeNullOrEmpty();

        var ll = str.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        ll.Should().NotBeEmpty();
        
        //every line should be a valid Json
        ll.Should().AllSatisfy(
            x => x
                .ParseJToken()
                .Should()
                .HaveElement("Timestamp")
        );

        var line = ll
            .First()
            .ParseJToken();

        // {
        //     "Timestamp": "2000-01-01T11:11:11.123456Z",
        //     "LogLevel": "Information",
        //     "Category": "Microsoft.Hosting.Lifetime",
        //     "Message": "Content root path: /workspaces/dotnet-api-example2/src/Example.Api",
        //      ...
        // }

        line.Should().HaveElement("Timestamp");
        line.Should().HaveElement("LogLevel").Which.Type.Should().Be(JTokenType.String);
        line.Should().HaveElement("Category").Which.Type.Should().Be(JTokenType.String);
        line.Should().HaveElement("Message").Which.Type.Should().Be(JTokenType.String);

//TODO: assert Timestamp format
    }
}