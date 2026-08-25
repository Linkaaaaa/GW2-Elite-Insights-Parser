using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GW2EIParserAvalonia.Services;
using GW2EIParserCommons;

namespace GW2EIParserAvalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<LogFileViewModel> logFiles = [];
    [ObservableProperty]
    private string status = "Waiting";
    private readonly ParserService _parserService;
    private readonly SettingsService _settingsService;
    public ProgramSettings Settings { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public MainWindowViewModel()
    {
        _settingsService = new SettingsService();
        Settings = _settingsService.Load();
        SettingsViewModel = new SettingsViewModel(Settings);
        SettingsViewModel.LoadFromSettings();
        _parserService = new ParserService(Settings);
        SettingsViewModel.SettingsApplied += SettingsViewModel_SettingsApplied;
    }

    private void SettingsViewModel_SettingsApplied(object? sender, EventArgs e)
    {
        _settingsService.Save(Settings);
        _parserService.ApplySettings();
    }

    public void AddFile(string path)
    {
        if (!IsValidLogFile(path))
        {
            return;
        }

        if (LogFiles.Any(x => string.Equals(x.InputFile, path, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var logFile = new LogFileViewModel(path);

        logFile.ParseRequested += LogFile_ParseRequested;
        logFile.ReParseRequested += LogFile_ReParseRequested;

        LogFiles.Add(logFile);
        Status = $"{LogFiles.Count} log(s) queued";
    }

    private async void LogFile_ParseRequested(object? sender, EventArgs e)
    {
        if (sender is LogFileViewModel logFile)
        {
            await ParseFileAsync(logFile);
        }
    }

    private async void LogFile_ReParseRequested(object? sender, EventArgs e)
    {
        if (sender is LogFileViewModel logFile)
        {
            await ParseFileAsync(logFile);
        }
    }

    private static bool IsValidLogFile(string path)
    {
        return path.EndsWith(".evtc", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".evtc.zip", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".zevtc", StringComparison.OrdinalIgnoreCase);
    }

    public async Task ParseFileAsync(LogFileViewModel logFile)
    {
        logFile.Operation.ToRunState();
        logFile.UpdateFromOperation();

        try
        {
            await _parserService.ParseAsync(logFile.Operation);
            logFile.Operation.ToCompleteState();
        }
        catch (OperationCanceledException)
        {
            logFile.Operation.ToCancelledState();
        }
        catch (Exception)
        {
            logFile.Operation.ToUnCompleteState();
        }

        logFile.UpdateFromOperation();
    }

    public void ApplySettings()
    {
        SettingsViewModel.ApplyToSettings();
    }
}
