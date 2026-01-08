namespace Utils.Testing;

//TODO: renew
// may be there is a better way to stub random...
public class FakeRandom : Random
{
//TODO: split strategies
//- NoFake
//-AutoIncrement
//-List
//-Single
    public bool AutoIncrementInt  { get; set; } = false;
    public int? FakeNextInt { get; set; }

    private int _idx = -1;
    public List<int> FakeIntValues { get; set; } = [];

    public double? FakeNextDouble { get; set; }

    public FakeRandom() : base(1)
    {
    }

    public override int Next(int maxValue)
    {
        //assert params
        _ = base.Next(maxValue);

        if(FakeIntValues.Count > 0)
        {
            _idx++;
            _idx %= FakeIntValues.Count;
            return FakeIntValues[_idx] % maxValue;
        }

        if(AutoIncrementInt && FakeNextInt.HasValue == false)
            FakeNextInt = 0;
        else if(AutoIncrementInt && FakeNextInt == int.MaxValue)
            FakeNextInt = int.MinValue;
        else if(AutoIncrementInt && FakeNextInt.HasValue == true)
            FakeNextInt++;
        

        if (FakeNextInt.HasValue)
            return FakeNextInt.Value % maxValue;

        return base.Next(maxValue);
    }

    public override int Next(int minValue, int maxValue)
    {
        //assert params
        _ = base.Next(minValue, maxValue);

        if (minValue == maxValue)
            return minValue;

        var d = maxValue - minValue;
        var r = minValue + Next(d);
        return r;
    }

    public override double NextDouble()
    {
        if (FakeNextDouble.HasValue)
            return FakeNextDouble.Value;

        return base.NextDouble();
    }
}