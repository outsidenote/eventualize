namespace Eventualize.Core
{
    public class EventualizeFoldingLogic<TState> where TState : notnull, new()
    {
        public readonly Dictionary<string, IFoldingFunction<TState>> Logic;

        public EventualizeFoldingLogic(Dictionary<string, IFoldingFunction<TState>> logic)
        {
            Logic = logic;
        }

        public TState FoldEvent(TState oldState, IEventualizeEvent someEvent)
        {
            TState currentState = oldState;
            IFoldingFunction<TState>? foldingFunction;
            if (!Logic.TryGetValue(someEvent.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(someEvent));
            currentState = foldingFunction.Fold(currentState, someEvent);
            return currentState;
        }

        public async Task<FoldingResult<TState>> FoldEventsAsync(
            IAsyncEnumerable<IEventualizeStoredEvent> events)
        {
            TState state = new();
            var result = await FoldEventsAsync(state, events);
            return result;
        }

        public async Task<FoldingResult<TState>> FoldEventsAsync(
            TState oldState,
            IAsyncEnumerable<IEventualizeStoredEvent> events)
        {
            long count = 0;
            TState currentState = oldState;
            await foreach (IEventualizeStoredEvent e in events)
            {
                currentState = FoldEvent(currentState, e);
                count++;
            }
            return new FoldingResult<TState>(currentState, count);
        }

        public FoldingResult<TState> FoldEvents(
            IEnumerable<IEventualizeEvent> events)
        {
            TState state = new();
            var result = FoldEvents(state, events);
            return result;
        }

        public FoldingResult<TState> FoldEvents(
            TState oldState,
            IEnumerable<IEventualizeEvent> events)
        {
            long count = 0;
            TState currentState = oldState;
            foreach (var e in events)
            {
                currentState = FoldEvent(currentState, e);
                count++;
            }
            return new FoldingResult<TState>(currentState, count);
        }

    }
}