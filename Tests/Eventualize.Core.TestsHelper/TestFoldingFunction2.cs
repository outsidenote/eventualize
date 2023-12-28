namespace Eventualize.Core.Tests;

class TestFoldingFunction2 : IFoldingFunction<TestState>
{
    public TestState Fold(TestState oldState, IEventualizeEvent serializedEvent)
    {
        TestEventDataType data = serializedEvent.GetData<TestEventDataType>();
        return new TestState(oldState.ACount + 10, oldState.BCount + 10, oldState.BSum + data.B * 10);
    }
}