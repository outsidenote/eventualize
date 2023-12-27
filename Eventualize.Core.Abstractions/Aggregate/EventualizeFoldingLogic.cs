using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core
{
    public class EventualizeFoldingLogic<T> where T : notnull, new()
    {
        public readonly ImmutableDictionary<string, IFoldingFunction<T>> Logic;

        internal EventualizeFoldingLogic(ImmutableDictionary<string, IFoldingFunction<T>> logic)
        {
            Logic = logic;
        }

        public T FoldEvent(T oldState, EventualizeEvent someEvent)
        {
            T currentState = oldState;
            IFoldingFunction<T>? foldingFunction;
            if (!Logic.TryGetValue(someEvent.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(someEvent));
            currentState = foldingFunction.Fold(currentState, someEvent);
            return currentState;
        }

        public async Task<FoldingResult<T>> FoldEventsAsync(
            IAsyncEnumerable<EventualizeStoredEvent> events)
        {
            T state = new();
            var result = await FoldEventsAsync(state, events);
            return result;
        }

        public async Task<FoldingResult<T>> FoldEventsAsync(
            T oldState,
            IAsyncEnumerable<EventualizeStoredEvent> events)
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
            IEnumerable<EventualizeEvent> events)
        {
            T state = new();
            var result = FoldEvents(state, events);
            return result;
        }

        public FoldingResult<T> FoldEvents(
            T oldState,
            IEnumerable<EventualizeEvent> events)
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