namespace FEFF.Extentions;

public class TypeHelper
{
    public static string GetTypeName<T>()
    {
//TODO: no assert?
        var name = typeof(T).FullName;
        ThrowHelper.Assert(name != null);
        return name;
    }
}