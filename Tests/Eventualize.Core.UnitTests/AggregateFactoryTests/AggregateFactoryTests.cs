using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests;

public sealed class AggregateFactoryTests
{
    [Fact]
    public void AggregateFactory_WhenFoldingEvents_Succeed()
    {
        var events = TestAggregateConfigs.GetPendingEvents(3);
        var (foldedState, count) = TestAggregateFactoryConfigs
            .TestAggregateFactory
            .FoldingLogic
            .FoldEvents(events);
        TestState expectedState = new TestState(3, 3, 30);
        Assert.Equal(expectedState, foldedState);
        Assert.Equal(3, count);
    }
}