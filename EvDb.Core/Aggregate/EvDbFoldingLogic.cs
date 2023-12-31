using System.Collections.Immutable;

namespace EvDb.Core
{
    public class EvDbFoldingLogic<T> where T : notnull, new()
    {
        public readonly IImmutableDictionary<string, IFoldingFunction<T>> Logic;

        internal EvDbFoldingLogic(IImmutableDictionary<string, IFoldingFunction<T>> logic)
        {
            Logic = logic;
        }

        public T FoldEvent(T oldState, IEvDbEvent someEvent)
        {
            T currentState = oldState;
            IFoldingFunction<T>? foldingFunction;
            if (!Logic.TryGetValue(someEvent.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(someEvent));
            currentState = foldingFunction.Fold(currentState, someEvent);
            return currentState;
        }

        public async Task<FoldingResult<T>> FoldEventsAsync(
            IAsyncEnumerable<EvDbStoredEvent> events)
        {
            T state = new();
            var result = await FoldEventsAsync(state, events);
            return result;
        }

        public async Task<FoldingResult<T>> FoldEventsAsync(
            T oldState,
            IAsyncEnumerable<IEvDbStoredEvent> events)
        {
            long count = 0;
            T currentState = oldState;
            await foreach (var e in events)
            {
                currentState = FoldEvent(currentState, e);
                count++;
            }
            return new FoldingResult<T>(currentState, count);
        }

        public FoldingResult<T> FoldEvents(
            IEnumerable<IEvDbEvent> events)
        {
            T state = new();
            var result = FoldEvents(state, events);
            return result;
        }

        public FoldingResult<T> FoldEvents(
            T oldState,
            IEnumerable<IEvDbEvent> events)
        {
            long count = 0;
            T currentState = oldState;
            foreach (var e in events)
            {
                currentState = FoldEvent(currentState, e);
                count++;
            }
            return new FoldingResult<T>(currentState, count);
        }

    }
}