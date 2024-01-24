//namespace EvDb.Core.Tests;

//// TODO: options

//[Obsolete("Deprecated")]
//class TestFoldingFunction : IFoldingFunction<TestState>
//{
//    TestState IFoldingFunction<TestState>.Fold(TestState oldState, IEvDbEvent serializedEvent)
//    {
//        TestEventDataType data = serializedEvent.GetData<TestEventDataType>();
//        return new TestState(oldState.ACount + 1, oldState.BCount + 1, oldState.BSum + data.B);
//    }
//}