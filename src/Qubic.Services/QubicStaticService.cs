using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Qubic.Services;

/// <summary>
/// Downloads and parses the static data bundle from static.qubic.org.
/// Exposes parsed data (labels, contract fees, etc.) for consumption by other services.
/// </summary>
public sealed class QubicStaticService
{
    private const string BundleUrl = "https://static.qubic.org/v1/general/data/bundle.min.json";

    private readonly Dictionary<string, string> _exchangeLabels = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _addressLabels = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _tokenIssuerLabels = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _contractLabels = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, Dictionary<string, ulong>> _contractFees = new();

    /// <summary>Raised when bundle data is loaded or cleared.</summary>
    public event Action? OnDataChanged;

    /// <summary>Whether a fetch is currently in progress.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>Error message from the last fetch attempt, if any.</summary>
    public string? LastError { get; private set; }

    /// <summary>Timestamp of the last successful fetch.</summary>
    public DateTime? LastFetched { get; private set; }

    /// <summary>Exchange address labels (address → name).</summary>
    public IReadOnlyDictionary<string, string> ExchangeLabels => _exchangeLabels;

    /// <summary>General address labels (address → label).</summary>
    public IReadOnlyDictionary<string, string> AddressLabels => _addressLabels;

    /// <summary>Token issuer labels (issuer address → "NAME (Issuer)").</summary>
    public IReadOnlyDictionary<string, string> TokenIssuerLabels => _tokenIssuerLabels;

    /// <summary>Smart contract labels (address → name).</summary>
    public IReadOnlyDictionary<string, string> ContractLabels => _contractLabels;

    /// <summary>Contract procedure fees. Key = contractIndex, Value = dict of procedureName → fee.</summary>
    public IReadOnlyDictionary<int, Dictionary<string, ulong>> ContractFees => _contractFees;

    /// <summary>Gets the fee for a specific contract procedure, or null if unknown.</summary>
    public ulong? GetContractFee(int contractIndex, string procedureName)
    {
        if (_contractFees.TryGetValue(contractIndex, out var procedures) &&
            procedures.TryGetValue(procedureName, out var fee))
            return fee;
        return null;
    }

    /// <summary>
    /// Fetches and parses the static data bundle from static.qubic.org.
    /// </summary>
    public async Task<bool> LoadAsync(CancellationToken ct = default)
    {
        if (IsLoading) return false;

        IsLoading = true;
        LastError = null;
        OnDataChanged?.Invoke();

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var bundle = await http.GetFromJsonAsync<BundleData>(BundleUrl, ct);
            if (bundle == null)
            {
                LastError = "Empty response from static data registry.";
                return false;
            }

            ClearDictionaries();

            if (bundle.Exchanges != null)
                foreach (var e in bundle.Exchanges)
                    if (!string.IsNullOrEmpty(e.Address) && !string.IsNullOrEmpty(e.Name))
                        _exchangeLabels.TryAdd(e.Address, e.Name);

            if (bundle.AddressLabels != null)
                foreach (var a in bundle.AddressLabels)
                    if (!string.IsNullOrEmpty(a.Address) && !string.IsNullOrEmpty(a.Label))
                        _addressLabels.TryAdd(a.Address, a.Label);

            if (bundle.Tokens != null)
                foreach (var t in bundle.Tokens)
                    if (!string.IsNullOrEmpty(t.Issuer) && !string.IsNullOrEmpty(t.Name))
                        _tokenIssuerLabels.TryAdd(t.Issuer, $"{t.Name} (Issuer)");

            if (bundle.SmartContracts != null)
            {
                foreach (var c in bundle.SmartContracts)
                {
                    if (!string.IsNullOrEmpty(c.Address) && !string.IsNullOrEmpty(c.Name))
                        _contractLabels.TryAdd(c.Address, c.Name);

                    if (c.ContractIndex > 0 && c.Procedures != null)
                    {
                        var fees = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
                        foreach (var p in c.Procedures)
                            if (!string.IsNullOrEmpty(p.Name) && p.Fee.HasValue)
                                fees[p.Name] = (ulong)p.Fee.Value;
                        if (fees.Count > 0)
                            _contractFees[c.ContractIndex] = fees;
                    }
                }
            }

            LastFetched = DateTime.Now;
            OnDataChanged?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            OnDataChanged?.Invoke();
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Clears all loaded data.</summary>
    public void Clear()
    {
        ClearDictionaries();
        LastFetched = null;
        OnDataChanged?.Invoke();
    }

    private void ClearDictionaries()
    {
        _exchangeLabels.Clear();
        _addressLabels.Clear();
        _tokenIssuerLabels.Clear();
        _contractLabels.Clear();
        _contractFees.Clear();
    }

    // ── Bundle JSON model ──

    private sealed class BundleData
    {
        [JsonPropertyName("exchanges")]
        public List<ExchangeEntry>? Exchanges { get; set; }

        [JsonPropertyName("address_labels")]
        public List<AddressLabelEntry>? AddressLabels { get; set; }

        [JsonPropertyName("tokens")]
        public List<TokenEntry>? Tokens { get; set; }

        [JsonPropertyName("smart_contracts")]
        public List<SmartContractEntry>? SmartContracts { get; set; }
    }

    private sealed class ExchangeEntry
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }

    private sealed class AddressLabelEntry
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }

    private sealed class TokenEntry
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }
    }

    private sealed class SmartContractEntry
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("contractIndex")]
        public int ContractIndex { get; set; }

        [JsonPropertyName("procedures")]
        public List<ProcedureEntry>? Procedures { get; set; }
    }

    private sealed class ProcedureEntry
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("fee")]
        public long? Fee { get; set; }
    }
}
