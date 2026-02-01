namespace Qubic.Core.Entities;

/// <summary>
/// Asset record types in the Qubic universe.
/// </summary>
public enum AssetRecordType : byte
{
    /// <summary>
    /// Empty/unused record.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// Asset issuance record (defines the asset).
    /// </summary>
    Issuance = 1,

    /// <summary>
    /// Asset ownership record (who owns shares).
    /// </summary>
    Ownership = 2,

    /// <summary>
    /// Asset possession record (who holds shares).
    /// </summary>
    Possession = 3
}
