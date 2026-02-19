namespace FEFF.Extentions;

public class InvalidMatchException : InvalidOperationException
{
    public InvalidMatchException(string reason)
        : base($"Invalid Match: '{reason}'. CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive).")
    {
    }

    public InvalidMatchException() : this("") {}

//     public InvalidMatchException(string message, Exception inner)
//         : base(message, inner)
//     {
//     }
}