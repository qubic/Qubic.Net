namespace Qubic.Core.Entities;

/// <summary>
/// Mining solution submitted by a computor.
/// </summary>
public sealed class Solution
{
    /// <summary>
    /// The computor's public key that submitted the solution.
    /// </summary>
    public required byte[] ComputorPublicKey { get; init; }

    /// <summary>
    /// The mining seed used for this solution.
    /// </summary>
    public required byte[] MiningSeed { get; init; }

    /// <summary>
    /// The nonce that solves the mining puzzle.
    /// </summary>
    public required byte[] Nonce { get; init; }
}
