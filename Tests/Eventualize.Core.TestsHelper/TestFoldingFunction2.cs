using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests
{
    class TestFoldingFunction2 : IFoldingFunction<TestState>
    {
        public TestState Fold(TestState oldState, EventualizeEvent serializedEvent)
        {
            TestEventDataType data = TestEventType.ParseData(serializedEvent);
            return new TestState(oldState.ACount + 10, oldState.BCount + 10, oldState.BSum + data.B*10);
        }
    }
}