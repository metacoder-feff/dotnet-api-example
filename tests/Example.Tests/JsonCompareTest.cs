namespace Example.Tests;

public class JsonCompareTest
{
    [Fact
    (Skip = "unskip to see error message (first diff of JSONs)")
    ]
    public void Test_JSON_compare__2()
    {
        var expected = JToken.Parse("{\"foo\":\"bar\"}");
        var actual = JToken.Parse("{\"baz\":\"qux\", \"foo\":\"bar3\"}");

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact
    //(Skip = "tmp")
    ]
    public void Test_JSON_compare__3()
    {
        var actual = JToken.Parse("""
        {
          "timestamp": "2000-01-01T00:00:00Z"
        }
        """);
        
        var other = JToken.Parse("""
        {
          "timestamp": "2000-01-01T00:00:00+01:00"
        }
        """);

        actual.Should().NotBeEquivalentTo(other);
    }

    [Fact
    //(Skip = "tmp")
    ]
    public void Test_JSON_compare__4()
    {
        var actual = JToken.Parse("""
        {
          "timestamp": "2000-01-01T12:00:00+01"
        }
        """);
        
        var other = JToken.Parse("""
        {
          "timestamp": "2000-01-01T11:00:00+02:00"
        }
        """);

        actual.Should().NotBeEquivalentTo(other);
    }

    [Fact
    //(Skip = "tmp")
    ]
    public void Test_JSON_compare__5()
    {
        var actual = JToken.Parse("""
        {
           // comment1
          "qq": 1.0
        }
        """);

        actual.Should().BeEquivalentTo("""
        {
           // comment2
          "qq": 1.0
        }
        """);
    }

    [Fact
    //(Skip = "tmp")
    ]
    public void Test_JSON_compare__6()
    {
        var actual = """
        {
          "timestamp": "2000-01-01T12:00:00+01"
        }
        """;

        actual
            .ParseJToken()
            .Should()
            .BeEquivalentTo("""
        {
          "timestamp": "2000-01-01T12:00:00+01"
        }
        """);
    }
}