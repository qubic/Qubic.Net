namespace Qubic.Core.Entities;

/// <summary>
/// IPO bid included in a transaction.
/// </summary>
public sealed class ContractIpoBid
{
    /// <summary>
    /// The bid price per share.
    /// </summary>
    public required long Price { get; init; }

    /// <summary>
    /// The quantity of shares to bid for.
    /// </summary>
    public required ushort Quantity { get; init; }
}
