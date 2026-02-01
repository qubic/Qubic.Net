using Qubic.Core.Entities;

namespace Qubic.Core;

/// <summary>
/// Well-known Qubic contract indices.
/// </summary>
public static class QubicContracts
{
    /// <summary>
    /// QX - Asset exchange contract.
    /// </summary>
    public const int Qx = 1;

    /// <summary>
    /// Quottery - Lottery contract.
    /// </summary>
    public const int Quottery = 2;

    /// <summary>
    /// Random - Random number generation contract.
    /// </summary>
    public const int Random = 3;

    /// <summary>
    /// QUTIL - Utility functions (SendToMany, etc).
    /// </summary>
    public const int Qutil = 4;

    /// <summary>
    /// MLM - Machine Learning Mining contract.
    /// </summary>
    public const int Mlm = 5;

    /// <summary>
    /// QVAULT - Vault contract.
    /// </summary>
    public const int Qvault = 6;

    /// <summary>
    /// QEARN - Earning/staking contract.
    /// </summary>
    public const int Qearn = 7;

    /// <summary>
    /// Gets the contract public key for a given contract index.
    /// Contract addresses are encoded as: contractIndex in first byte, rest zeros.
    /// </summary>
    public static byte[] GetContractPublicKey(int contractIndex)
    {
        var pubKey = new byte[32];
        pubKey[0] = (byte)contractIndex;
        return pubKey;
    }

    /// <summary>
    /// Gets the contract identity for a given contract index.
    /// </summary>
    public static QubicIdentity GetContractIdentity(int contractIndex)
    {
        return QubicIdentity.FromPublicKey(GetContractPublicKey(contractIndex));
    }
}
