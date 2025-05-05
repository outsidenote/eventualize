using System.Collections.Immutable;

namespace Funds.Abstractions;

/// <summary>
/// The common data of a transaction
/// </summary>
public readonly record struct FundsTransactionData
{
    /// <summary>
    /// Id of financial operation like moving money from one account to another (can related to multiple transactons)
    /// </summary>
    public required Guid PaymentId { get; init; }
    /// <summary>
    /// Id of a financial execution unit like withdrawal or deposit
    /// </summary>
    public required Guid TransactionId { get; init; }
    /// <summary>
    /// Date of a financial execution unit like withdrawal or deposit
    /// </summary>
    public required DateTimeOffset TransactionDate { get; init; }
    /// <summary>
    /// The currency of the transaction
    /// </summary>
    public required Currency Currency { get; init; }
    /// <summary>
    /// The money amount
    /// </summary>
    public required double Amount { get; init; }
}