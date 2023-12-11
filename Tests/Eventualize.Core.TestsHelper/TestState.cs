using System.Text.Json.Serialization;

namespace Eventualize.Core.Tests;

public class TestState : IEquatable<TestState>
{
    public int ACount { get; private set; }
    public int BCount { get; private set; }
    public int BSum { get; private set; }

    public TestState()
    {
        ACount = 0;
        BCount = 0;
        BSum = 0;

    }

    [JsonConstructor]
    public TestState(int aCount, int bCount, int bSum)
    {
        ACount = aCount;
        BCount = bCount;
        BSum = bSum;
    }

    public bool Equals(TestState? other)
    {
        if (other == null) return false;
        return ACount == other.ACount && BCount == other.BCount && BSum == other.BSum;
    }

    public override string ToString()
    {
        return $"(ACount={ACount}, BCount={BCount}, BSum={BSum})";
    }
};