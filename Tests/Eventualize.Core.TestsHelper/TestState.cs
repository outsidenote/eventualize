using Generator.Equals;
using System.Text.Json.Serialization;

namespace Eventualize.Core.Tests;

[Equatable]
public partial class TestState
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

    public override string ToString()
    {
        return $"(ACount={ACount}, BCount={BCount}, BSum={BSum})";
    }
};