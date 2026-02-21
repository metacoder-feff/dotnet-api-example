namespace FEFF.Extentions.Tests;

public class ThrowHelperTest
{
    #region ThrowHelper.Assert(bool)
    [Fact]
    public void InvalidOperation_Assert__positive()
    {
        Action act = () => ThrowHelper.Assert(true);

        act.Should().NotThrow();
    }

    [Fact]
    public void InvalidOperation_Assert__should_throw()
    {
        Action act = () => ThrowHelper.Assert(false);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Assertion violated: 'false'");
    }

    [Fact]
    public void InvalidOperation_Assert__should_throw__with_complex_message()
    {
        var a = 7;
        Action act = () => ThrowHelper.Assert(a > 8);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Assertion violated: 'a > 8'");
    }

    [Fact]
    public void InvalidOperation_Assert__should_NOT_throw_if_ok()
    {
        var a = 7;
        ThrowHelper.Assert(true);
        ThrowHelper.Assert(a < 8);
    }
    #endregion
    
    #region System.ArgumentException.ThrowIfNullOrEmpty(string?)
    private static Action ThrowIfNullOrEmptyString(string? str)
    {
        return () => ArgumentException.ThrowIfNullOrEmpty(str);
    }

    [Fact]
    public void Argument_String_ThrowIfNullOrEmpty__positive()
    {
        ThrowIfNullOrEmptyString("1")
            .Should()
            .NotThrow();
    }
    [Fact]
    public void Argument_String_ThrowIfNullOrEmpty__empty()
    {
        ThrowIfNullOrEmptyString("")
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty string. (Parameter 'str')");
    }
    
    [Fact]
    public void Argument_String_ThrowIfNullOrEmpty__null()
    {
        ThrowIfNullOrEmptyString(null)
        .Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'str')");
    }
    #endregion

    #region ThrowHelper.Argument.ThrowIfNullOrEmpty<T>(IEnumerable<T>?)
    private static Action ThrowIfNullOrEmptyList(List<int>? list)
    {
        return () => ThrowHelper.Argument.ThrowIfNullOrEmpty(list);
    }

    [Fact]
    public void Argument_Enumerable_ThrowIfNullOrEmpty__positive()
    {
        ThrowIfNullOrEmptyList([1])
            .Should()
            .NotThrow();
    }
    
    [Fact]
    public void Argument_Enumerable_ThrowIfNullOrEmpty__null()
    {
        ThrowIfNullOrEmptyList(null)
            .Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'list')");
    }

    [Fact]
    public void Argument_Enumerable_ThrowIfNullOrEmpty__empty()
    {
        ThrowIfNullOrEmptyList([])
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty collection. (Parameter 'list')");
    }
    #endregion
}