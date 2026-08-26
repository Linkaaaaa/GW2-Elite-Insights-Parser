using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    [ObservableProperty]
    private bool parseEnabled;
    [ObservableProperty]
    private bool cancelAllEnabled;
    [ObservableProperty]
    private bool clearAllEnabled;
    [ObservableProperty]
    private bool clearFailedEnabled;
    [ObservableProperty]
    private bool discordBatchEnabled = true;
    [ObservableProperty]
    private bool autoDiscordBatchEnabled = true;
    [ObservableProperty]
    private bool checkUpdatesEnabled = true;
    [ObservableProperty]
    private bool settingsEnabled = true;
    [ObservableProperty]
    private bool traces;
    [ObservableProperty]
    private bool autoDiscordBatch;
    [ObservableProperty]
    private decimal populateHourLimit;
    [ObservableProperty]
    private string watchingDirectory = string.Empty;
    [ObservableProperty]
    private bool watchingDirectoryVisible;
    [ObservableProperty]
    private string version = string.Empty;
    private readonly ParserService _parserService;
    private readonly SettingsService _settingsService;
    private readonly Queue<LogFileViewModel> _logQueue = new();
    private int _runningCount;
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

        Traces = SettingsViewModel.ApplicationTraces;
        AutoDiscordBatch = SettingsViewModel.AutoDiscordBatch;
        PopulateHourLimit = SettingsViewModel.PopulateHourLimit;

        UpdateWatchDirectory();

        ClearAllEnabled = false;
        ParseEnabled = false;
        CancelAllEnabled = false;

        Version = typeof(MainWindowViewModel).Assembly.GetName().Version?.ToString() ?? string.Empty;
    }

    public bool AnyRunning => _runningCount > 0;

    private void SettingsViewModel_SettingsApplied(object? sender, EventArgs e)
    {
        _settingsService.Save(Settings);
        _parserService.ApplySettings();

        Traces = SettingsViewModel.ApplicationTraces;
        AutoDiscordBatch = SettingsViewModel.AutoDiscordBatch;
        PopulateHourLimit = SettingsViewModel.PopulateHourLimit;

        UpdateWatchDirectory();
    }

    public void AddFile(string path)
    {
        if (LogFiles.Any(x => string.Equals(x.InputFile, path, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var logFile = new LogFileViewModel(path);

        logFile.ParseRequested += LogFile_ParseRequested;
        logFile.ReParseRequested += LogFile_ReParseRequested;
        logFile.PendingCancellationRequested += LogFile_PendingCancellationRequested;

        LogFiles.Add(logFile);

        Status = $"{LogFiles.Count} log(s) queued";
        ParseEnabled = !AnyRunning;
        ClearAllEnabled = true;
        ClearFailedEnabled = true;

        if (SettingsViewModel.AutoParse)
        {
            QueueOrRunOperation(logFile);
        }
    }

    private async void LogFile_ParseRequested(object? sender, EventArgs e)
    {
        if (sender is LogFileViewModel logFile)
        {
            QueueOrRunOperation(logFile);
        }

        await Task.CompletedTask;
    }

    private async void LogFile_ReParseRequested(object? sender, EventArgs e)
    {
        if (sender is LogFileViewModel logFile)
        {
            QueueOrRunOperation(logFile);
        }

        await Task.CompletedTask;
    }

    public void ParseAll()
    {
        _logQueue.Clear();

        if (LogFiles.Count == 0)
        {
            return;
        }

        ParseEnabled = false;
        CancelAllEnabled = true;

        foreach (var logFile in LogFiles)
        {
            if (!logFile.IsBusy())
            {
                QueueOrRunOperation(logFile);
            }
        }
    }

    public void CancelAll()
    {
        var queued = new HashSet<LogFileViewModel>(_logQueue);

        _logQueue.Clear();

        foreach (var logFile in LogFiles)
        {
            if (logFile.IsBusy())
            {
                logFile.Operation.ToCancelState();
                logFile.UpdateFromOperation();
            }
            else if (queued.Contains(logFile))
            {
                logFile.Operation.ToReadyState();
                logFile.UpdateFromOperation();
            }
        }

        ClearAllEnabled = true;
        ParseEnabled = true;
        CancelAllEnabled = false;
        DiscordBatchEnabled = true;
        AutoDiscordBatchEnabled = true;
        CheckUpdatesEnabled = true;
    }

    public void ClearAll()
    {
        _logQueue.Clear();

        for (var i = LogFiles.Count - 1; i >= 0; i--)
        {
            var logFile = LogFiles[i];

            if (logFile.IsBusy())
            {
                logFile.Operation.ToCancelAndClearState();
                logFile.UpdateFromOperation();
            }
            else
            {
                LogFiles.RemoveAt(i);
            }
        }

        ParseEnabled = false;
        CancelAllEnabled = false;
        ClearAllEnabled = false;
        ClearFailedEnabled = LogFiles.Any(x => x.State == OperationState.UnComplete);
    }

    public void ClearFailed()
    {
        for (var i = LogFiles.Count - 1; i >= 0; i--)
        {
            var logFile = LogFiles[i];

            if (!logFile.IsBusy() && logFile.State == OperationState.UnComplete)
            {
                LogFiles.RemoveAt(i);
            }
        }
        
        ClearFailedEnabled = false;
    }

    public void AddFilesFromDirectory(string path)
    {
        var files = ProgramHelper.FetchSupportedFormatsFrom(path, SettingsViewModel.PopulateHourLimit, DateTime.Now);

        foreach (var file in files)
        {
            AddFile(file);
        }
    }

    public void UpdateWatchDirectory()
    {
        if (SettingsViewModel.AutoAdd && Directory.Exists(SettingsViewModel.AutoAddPath))
        {
            WatchingDirectory = SettingsViewModel.AutoAddPath;
            WatchingDirectoryVisible = true;
        }
        else
        {
            WatchingDirectory = string.Empty;
            WatchingDirectoryVisible = false;
        }
    }

    public void HandleCreatedFile(string path)
    {
        if (!ProgramHelper.IsSupportedFormat(path))
        {
            return;
        }

        AddDelayed(path);
    }

    public void HandleRenamedFile(string oldPath, string newPath)
    {
        if (ProgramHelper.IsTemporaryCompressedFormat(oldPath) && ProgramHelper.IsCompressedFormat(newPath))
        {
            AddDelayed(newPath);
        }
        else if (ProgramHelper.IsTemporaryFormat(oldPath) && ProgramHelper.IsSupportedFormat(newPath))
        {
            AddDelayed(newPath);
        }
    }

    private async void AddDelayed(string path)
    {
        await Task.Delay(3000);

        if (File.Exists(path))
        {
            AddFile(path);
        }
    }

    private void QueueOrRunOperation(LogFileViewModel logFile)
    {
        ClearAllEnabled = true;
        ParseEnabled = false;
        CancelAllEnabled = true;
        DiscordBatchEnabled = false;
        AutoDiscordBatchEnabled = false;
        CheckUpdatesEnabled = false;

        if (_parserService.ParseMultipleLogs() && _runningCount < _parserService.GetMaxParallelRunning())
        {
            _ = RunOperationAsync(logFile);
        }
        else if (AnyRunning)
        {
            _logQueue.Enqueue(logFile);

            logFile.Operation.ToPendingState();
            logFile.UpdateFromOperation();
        }
        else
        {
            _ = RunOperationAsync(logFile);
        }
    }

    private async Task RunOperationAsync(LogFileViewModel logFile)
    {
        _parserService.ExecuteMemoryCheckTask();
        _runningCount++;

        logFile.Operation.ToQueuedState();
        logFile.UpdateFromOperation();

        logFile.Operation.ToRunState();
        logFile.UpdateFromOperation();

        try
        {
            await _parserService.ParseAsync(logFile.Operation);

            if (logFile.State != OperationState.ClearOnCancel)
            {
                logFile.Operation.ToCompleteState();
            }
        }
        catch (OperationCanceledException)
        {
            logFile.Operation.ToCancelledState();
        }
        catch (Exception)
        {
            logFile.Operation.ToUnCompleteState();
        }
        finally
        {
            _runningCount--;

            if (logFile.State == OperationState.ClearOnCancel)
            {
                LogFiles.Remove(logFile);
            }
            else
            {
                logFile.UpdateFromOperation();
            }

            _parserService.GenerateTraceFile(logFile.Operation);

            RunNextOperation();
        }
    }

    private void RunNextOperation()
    {
        if (_logQueue.Count > 0 && (_parserService.ParseMultipleLogs() || !AnyRunning))
        {
            _ = RunOperationAsync(_logQueue.Dequeue());
            return;
        }

        if (!AnyRunning)
        {
            ParseEnabled = true;
            ClearAllEnabled = true;
            CancelAllEnabled = false;
            DiscordBatchEnabled = true;
            AutoDiscordBatchEnabled = true;
            ClearFailedEnabled = LogFiles.Any(x => x.State == OperationState.UnComplete);
        }
    }

    private void LogFile_PendingCancellationRequested(object? sender, EventArgs e)
    {
        if (sender is not LogFileViewModel logFile)
        {
            return;
        }

        var operations = new HashSet<LogFileViewModel>(_logQueue);
        _logQueue.Clear();
        operations.Remove(logFile);

        foreach (var operation in operations)
        {
            _logQueue.Enqueue(operation);
        }

        logFile.Operation.ToReadyState();
        logFile.UpdateFromOperation();
    }
}
