// Based on [System.Reactive.Disposables]: https://github.com/dotnet/reactive
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

namespace EvDb.Core;


/// <summary>
/// Represents an Action-based disposable.
/// </summary>
internal sealed class AnonymousDisposable : ICancelable
{
    private volatile Action? _dispose;

    /// <summary>
    /// Constructs a new disposable with the given action used for disposal.
    /// </summary>
    /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
    /// <param name="useFinalizerTrigger">if set to <c>true</c> [use finalizer trigger].</param>
    public AnonymousDisposable(Action dispose, bool useFinalizerTrigger = false)
    {
        _dispose = dispose;
        if (!useFinalizerTrigger)
        {
#pragma warning disable S3971
            GC.SuppressFinalize(this);
#pragma warning restore S3971
        }
    }

    /// <summary>
    /// Gets a value that indicates whether the object is disposed.
    /// </summary>
    public bool IsDisposed => _dispose == null;

    ~AnonymousDisposable() => Dispose();

    /// <summary>
    /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}

/// <summary>
/// Represents a Action-based disposable that can hold onto some state.
/// </summary>
internal sealed class AnonymousDisposable<TState> : CancelableBase<TState>
{
    /// <summary>
    /// The dispose
    /// </summary>
    private volatile Action<TState>? _dispose;

    #region State

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public override TState State { get; internal set; }

    #endregion // State

    #region Ctor

    /// <summary>
    /// Constructs a new disposable with the given action used for disposal.
    /// </summary>
    /// <param name="state">The state to be passed to the disposal action.</param>
    /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
    /// <param name="useFinalizerTrigger">if set to <c>true</c> [use finalizer trigger].</param>
    public AnonymousDisposable(TState state, Action<TState>? dispose, bool useFinalizerTrigger = false)
    {
        if (!useFinalizerTrigger)
        {
#pragma warning disable S3971
            GC.SuppressFinalize(this);
#pragma warning restore S3971
        }
        State = state;
        _dispose = dispose;
    }

    #endregion // Ctor

    #region Dispose

    /// <summary>
    /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        Interlocked.Exchange(ref _dispose, null)?.Invoke(State);
    }

    #endregion // Dispose
}