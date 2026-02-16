namespace Qubic.Services;

/// <summary>
/// Provides human-readable labels for Qubic addresses/identities.
/// Merges static built-in labels, remote labels (from QubicStaticService), and user-defined labels.
/// </summary>
public sealed class LabelService
{
    private readonly QubicStaticService _staticService;
    private readonly Dictionary<string, string> _labels = new(_staticLabels, StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _remoteLabels = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _userLabels = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> _staticLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFXIB"] = "Null",
    };

    public LabelService(QubicStaticService staticService)
    {
        _staticService = staticService;
        _staticService.OnDataChanged += OnStaticDataChanged;
    }

    /// <summary>Raised when labels are added, removed, or changed.</summary>
    public event Action? OnLabelsChanged;

    /// <summary>Number of labels loaded from the remote registry.</summary>
    public int RemoteLabelCount => _remoteLabels.Count;

    /// <summary>Gets the label for an address, or null if not found. User labels take priority, then remote, then static.</summary>
    public string? GetLabel(string address)
    {
        if (_userLabels.TryGetValue(address, out var userLabel))
            return userLabel;
        return _labels.TryGetValue(address, out var label) ? label : null;
    }

    /// <summary>Gets the static built-in labels.</summary>
    public static IReadOnlyDictionary<string, string> StaticLabels => _staticLabels;

    /// <summary>Sets a single user-defined label.</summary>
    public void SetLabel(string address, string label)
    {
        _userLabels[address] = label;
        RebuildMerged();
    }

    /// <summary>Sets multiple user-defined labels at once.</summary>
    public void SetLabels(IEnumerable<KeyValuePair<string, string>> labels)
    {
        foreach (var kv in labels)
            _userLabels[kv.Key] = kv.Value;
        RebuildMerged();
    }

    /// <summary>Removes a user-defined label.</summary>
    public void RemoveLabel(string address)
    {
        if (_userLabels.Remove(address))
            RebuildMerged();
    }

    /// <summary>Gets all effective labels (static + remote + user overrides).</summary>
    public IReadOnlyDictionary<string, string> GetAll() => _labels;

    private void OnStaticDataChanged()
    {
        _remoteLabels.Clear();

        foreach (var kv in _staticService.ExchangeLabels)
            _remoteLabels.TryAdd(kv.Key, kv.Value);
        foreach (var kv in _staticService.AddressLabels)
            _remoteLabels.TryAdd(kv.Key, kv.Value);
        foreach (var kv in _staticService.TokenIssuerLabels)
            _remoteLabels.TryAdd(kv.Key, kv.Value);
        foreach (var kv in _staticService.ContractLabels)
            _remoteLabels.TryAdd(kv.Key, kv.Value);

        RebuildMerged();
    }

    private void RebuildMerged()
    {
        _labels.Clear();
        foreach (var kv in _staticLabels)
            _labels[kv.Key] = kv.Value;
        foreach (var kv in _remoteLabels)
            _labels[kv.Key] = kv.Value;
        foreach (var kv in _userLabels)
            _labels[kv.Key] = kv.Value;
        OnLabelsChanged?.Invoke();
    }
}
