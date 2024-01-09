using System.Collections.Immutable;

namespace EvDb.Core;

// TODO: [bnaya 2024-01-04] TBD Task<FoldingResult<T>> FoldEventsAsync might be infinite therefore callback might be better

public class EvDbFoldingLogic<T> : IEvDbFoldingLogic<T> where T : notnull, new()
{
    // TODO: replace it with an encapsulated version
    public readonly IImmutableDictionary<string, IFoldingFunction<T>> Logic;

    #region Ctor

    internal EvDbFoldingLogic(IImmutableDictionary<string, IFoldingFunction<T>> logic)
    {
        Logic = logic;
    }

    #endregion // Ctor

    #region FoldEvent

    /// <summary>
    /// Folds the event into a state.
    /// </summary>
    /// <param name="oldState">The old state.</param>
    /// <param name="someEvent">Some event.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException">someEvent</exception>
    public T FoldEvent(T oldState, IEvDbEvent someEvent)
    {
        T currentState = oldState;
        IFoldingFunction<T>? foldingFunction;
        if (!Logic.TryGetValue(someEvent.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(someEvent));
        currentState = foldingFunction.Fold(currentState, someEvent);
        return currentState;
    }

    #endregion // FoldEvent

    #region FoldEventsAsync

    /// <summary>
    /// Folds the events into a state.
    /// </summary>
    /// <param name="events">The events.</param>
    /// <returns></returns>
    public async Task<FoldingResult<T>> FoldEventsAsync(
        IAsyncEnumerable<EvDbStoredEvent> events)
    {
        T state = new();
        var result = await FoldEventsAsync(state, events);
        return result;
    }

    /// <summary>
    /// Folds the events into a state.
    /// </summary>
    /// <param name="oldState">The old state.</param>
    /// <param name="events">The events.</param>
    /// <returns></returns>
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

    #endregion // FoldEventsAsync

    #region FoldEvents

    /// <summary>
    /// Folds the events into a state.
    /// </summary>
    /// <param name="events">The events.</param>
    /// <returns></returns>
    public FoldingResult<T> FoldEvents(
        IEnumerable<IEvDbEvent> events)
    {
        T state = new();
        var result = FoldEvents(state, events);
        return result;
    }

    /// <summary>
    /// Folds the events into a state.
    /// </summary>
    /// <param name="oldState">The old state.</param>
    /// <param name="events">The events.</param>
    /// <returns></returns>
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

    #endregion // FoldEvents
}