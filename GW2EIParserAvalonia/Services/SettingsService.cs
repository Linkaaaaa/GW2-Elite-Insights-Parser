using System;
using System.IO;
using System.Text.Json;
using GW2EIParserCommons;

namespace GW2EIParserAvalonia.Services;

public sealed class SettingsService
{
    private readonly string _filePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsService()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GW2EliteInsights");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "settings.json");
    }

    public ProgramSettings Load()
    {
        if (!File.Exists(_filePath))
        {
            return new ProgramSettings();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<ProgramSettings>(json, JsonOptions) ?? new ProgramSettings();
        }
        catch
        {
            return new ProgramSettings();
        }
    }

    public void Save(ProgramSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        var temporaryPath = _filePath + ".tmp";

        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, _filePath, true);
    }
}
