namespace Qubic.Toolkit;

/// <summary>
/// Provides human-readable labels for Qubic addresses/identities.
/// Labels are populated externally (e.g. from a file or API).
/// </summary>
public sealed class LabelService
{
    private readonly Dictionary<string, string> _labels = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Raised when labels are added, removed, or changed.</summary>
    public event Action? OnLabelsChanged;

    /// <summary>Gets the label for an address, or null if not found.</summary>
    public string? GetLabel(string address)
    {
        return _labels.TryGetValue(address, out var label) ? label : null;
    }

    /// <summary>Sets a single label.</summary>
    public void SetLabel(string address, string label)
    {
        _labels[address] = label;
        OnLabelsChanged?.Invoke();
    }

    /// <summary>Sets multiple labels at once.</summary>
    public void SetLabels(IEnumerable<KeyValuePair<string, string>> labels)
    {
        foreach (var kv in labels)
            _labels[kv.Key] = kv.Value;
        OnLabelsChanged?.Invoke();
    }

    /// <summary>Removes a label.</summary>
    public void RemoveLabel(string address)
    {
        if (_labels.Remove(address))
            OnLabelsChanged?.Invoke();
    }

    /// <summary>Gets all labels.</summary>
    public IReadOnlyDictionary<string, string> GetAll() => _labels;
}
