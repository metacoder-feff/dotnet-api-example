namespace Utils.Tests;

public class ThrowHelperTest
{
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
    
    [Fact]
    public void Argument_String_ThrowIfNullOrEmpty__empty()
    {
        var str = "";
        Action act = () => ArgumentException.ThrowIfNullOrEmpty(str);
        
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty string. (Parameter 'str')");
        ;
    }
    
    [Fact]
    public void Argument_String_ThrowIfNullOrEmpty__null()
    {
        var str = null as string;
        Action act = () => ArgumentException.ThrowIfNullOrEmpty(str);
        
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'str')");
        ;
    }
    [Fact]
    public void Argument_Null_ThrowIfNull__null()
    {
        var str = null as string;
        Action act = () => ArgumentNullException.ThrowIfNullOrEmpty(str);
        
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'str')");
        ;
    }
}