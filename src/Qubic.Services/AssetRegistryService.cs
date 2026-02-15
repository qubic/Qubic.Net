using System.Text.Json;

namespace Qubic.Services;

public sealed class AssetRegistryService
{
    private readonly string _storageDir;
    private readonly string _assetsFile;
    private List<AssetEntry> _assets = [];

    public AssetRegistryService(QubicSettingsService settings)
    {
        _storageDir = settings.StorageDirectory;
        _assetsFile = Path.Combine(_storageDir, "assets.json");
        LoadFromDisk();
        if (_assets.Count == 0)
            SeedDefaults();
    }

    public IReadOnlyList<AssetEntry> GetAll() => _assets.AsReadOnly();

    public IEnumerable<string> GetIssuers() => _assets.Select(a => a.Issuer).Distinct();

    public IEnumerable<AssetEntry> GetAssetsForIssuer(string issuer) =>
        _assets.Where(a => a.Issuer.Equals(issuer, StringComparison.OrdinalIgnoreCase));

    public bool Add(string issuer, string name, string? label = null)
    {
        issuer = issuer.Trim().ToUpperInvariant();
        name = name.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(name)) return false;
        if (_assets.Any(a => a.Issuer == issuer && a.Name == name)) return false;

        _assets.Add(new AssetEntry { Issuer = issuer, Name = name, Label = label });
        SaveToDisk();
        OnChanged?.Invoke();
        return true;
    }

    public bool Remove(string issuer, string name)
    {
        var removed = _assets.RemoveAll(a =>
            a.Issuer.Equals(issuer, StringComparison.OrdinalIgnoreCase) &&
            a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
        {
            SaveToDisk();
            OnChanged?.Invoke();
        }
        return removed > 0;
    }

    public event Action? OnChanged;

    private void SeedDefaults()
    {
        // Well-known Qubic assets
        _assets =
        [
            new() { Issuer = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFNHB", Name = "QX", Label = "QX Token" },
            new() { Issuer = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFNHB", Name = "RANDOM", Label = "Random Token" },
            new() { Issuer = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFNHB", Name = "QUTIL", Label = "QUtil Token" },
            new() { Issuer = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFNHB", Name = "QTRY", Label = "Quottery Token" },
            new() { Issuer = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFNHB", Name = "MLM", Label = "MLM Token" },
            new() { Issuer = "CFBMEMZOIDEXQAUXYYSZIURADQLAPWPMNJXQSNVQZAHYVOPYUKKJBJQOFID", Name = "CFB", Label = "CFB" },
            new() { Issuer = "QABORECBFTFKFGCRUJDSNPTQWBAAOVDMGABDHMFIBOKXLGJXOKCHECRFGJL", Name = "QCAP", Label = "QCAP" },
            new() { Issuer = "QWALLETSGQVAGBHUCVVXWZLKMOIICE4ROIDNPAQBGMMFQQPPCNDNQHBMDL", Name = "QWALLET", Label = "QWallet" },
            new() { Issuer = "TFUYVBXYIYBVTEMJHAJGEJOOZHJBQFVQLTBBKMEHPEVIZFXZRPEYFUWGTIWG", Name = "QFT", Label = "QFT" },
        ];
        SaveToDisk();
    }

    private void SaveToDisk()
    {
        try
        {
            Directory.CreateDirectory(_storageDir);
            File.WriteAllText(_assetsFile, JsonSerializer.Serialize(_assets, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* best effort */ }
    }

    private void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_assetsFile)) return;
            var json = File.ReadAllText(_assetsFile);
            _assets = JsonSerializer.Deserialize<List<AssetEntry>>(json) ?? [];
        }
        catch { _assets = []; }
    }

    public sealed class AssetEntry
    {
        public string Issuer { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Label { get; set; }

        public override string ToString() => Label != null ? $"{Name} ({Label})" : Name;
    }
}
