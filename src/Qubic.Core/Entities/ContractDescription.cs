namespace Qubic.Core.Entities;

/// <summary>
/// Contract description/metadata.
/// </summary>
public sealed class ContractDescription
{
    /// <summary>
    /// The asset name associated with this contract (8 chars max).
    /// </summary>
    public required string AssetName { get; init; }

    /// <summary>
    /// The epoch when this contract was constructed.
    /// </summary>
    public required ushort ConstructionEpoch { get; init; }

    /// <summary>
    /// The epoch when this contract will be destructed (0 = never).
    /// </summary>
    public required ushort DestructionEpoch { get; init; }

    /// <summary>
    /// The size of the contract's state in bytes.
    /// </summary>
    public required ulong StateSize { get; init; }
}
