﻿using System.Diagnostics;

namespace EvDb.Core;

/// <summary>
/// Represent Semaphore scoping factory.
/// The semaphore scope will enable the usage of semaphore easy like lock.
/// </summary>
public sealed class AsyncLock : IDisposable
{
    private readonly SemaphoreSlim _gate;

    #region Ctor

    public AsyncLock()
    {
        _gate = new SemaphoreSlim(1);
    }

    #endregion // Ctor

    #region AcquireAsync

    /// <summary>
    /// Acquire async lock,
    /// when it will throw TimeoutException
    /// </summary>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>
    /// lock disposal
    /// </returns>
    /// <exception cref="System.TimeoutException"></exception>
    /// <exception cref="TimeoutException">when acquire lock fail</exception>
    public async Task<IDisposable> AcquireAsync(
                                    CancellationToken cancellation = default)
    {
        try
        {
            await _gate.WaitAsync(cancellation);
            return new Locker(this);
        }
        catch (OperationCanceledException)
        {
            try
            {
                _gate.Release();
            }
            catch
            {
                Debug.WriteLine("AsyncLock cancellation release failure");
            }
            throw;
        }
    }

    #endregion // AcquireAsync

    #region struct Locker : IDisposable

    /// <summary>
    /// Disposable implementation
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    private struct Locker : IDisposable
    {
        private readonly AsyncLock _lock;

        public Locker(AsyncLock @lock)
        {
            _lock = @lock;
        }

        public void Dispose()
        {
            _lock._gate.Release();
        }
    }

    #endregion //  struct Locker : IDisposable

    #region Dispose Pattern

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="AsyncLock"/> class.
    /// </summary>
    ~AsyncLock()
    {
        Dispose();
    }

    #endregion // Dispose Pattern
}
