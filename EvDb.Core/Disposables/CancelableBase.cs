// Based on [System.Reactive.Disposables]: https://github.com/dotnet/reactive
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

// Ignore Spelling: Cancelable

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

namespace EvDb.Core;

/// <summary>
/// Disposable resource with disposal state tracking.
/// </summary>
public abstract class CancelableBase<TState> : ICancelable
{
    #region TState State  { get; set; }

    /// <summary>
    /// Gets or Set the state.
    /// </summary>
    public abstract TState State { get; internal set; }

    #endregion // TState { get; set; }

    #region IsDisposed

    /// <summary>
    /// Gets a value that indicates whether the object is disposed.
    /// </summary>
    public virtual bool IsDisposed { get; private set; }

    #endregion // IsDisposed

    #region Casting overload

    /// <summary>
    /// Performs an implicit conversion from.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator TState(CancelableBase<TState> instance) => instance.State;

    #endregion // Casting overload

    #region Dispose Pattern

    /// <summary>
    /// Finalizes this instance.
    /// </summary>
    /// <returns></returns>
    ~CancelableBase() => OnDispose(false);

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">if set to <c>true</c> [disposing].</param>
    /// <returns></returns>
    private void OnDispose(bool disposing)
    {
        try
        {
            Dispose(disposing);
        }
        finally
        {
            IsDisposed = true;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources.
        OnDispose(true);
        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">if set to <c>true</c> [disposing].</param>
    /// <returns></returns>
#pragma warning disable S2953
    protected abstract void Dispose(bool disposing);
#pragma warning restore S2953 

    #endregion // Dispose Pattern
}