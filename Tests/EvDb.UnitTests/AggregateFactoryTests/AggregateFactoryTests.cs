namespace EvDb.Core.Tests;

public sealed class AggregateFactoryTests
{
    [Fact]
    public void AggregateFactory_WhenFoldingEvents_Succeed()
    {
        throw new NotImplementedException();

        //var events = TestAggregateConfigs.GetPendingEvents(3);
        //var (foldedState, count) = TestAggregateFactoryConfigs
        //    .GetAggregateFactory()
        //    .FoldingLogic
        //    .FoldEvents(events);
        //TestState expectedState = new TestState(3, 3, 30);
        //Assert.Equal(expectedState, foldedState);
        //Assert.Equal(3, count);
    }
}