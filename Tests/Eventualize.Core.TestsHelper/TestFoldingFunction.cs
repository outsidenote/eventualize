using Eventualize.Core.AggregateType;
using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests
{
    class TestFoldingFunction : IFoldingFunction<TestState>
    {
        public TestState Fold(TestState oldState, EventEntity SerializedEvent)
        {
            TestEventDataType data = TestEventType.ParseData(SerializedEvent);
            return new TestState(oldState.ACount + 1, oldState.BCount + 1, oldState.BSum + data.B);
        }
    }
}