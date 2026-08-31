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
    private string queueStatus;
    [ObservableProperty]
    private bool parseEnabled = false;
    [ObservableProperty]
    private bool cancelAllEnabled = false;
    [ObservableProperty]
    private bool clearAllEnabled = false;
    [ObservableProperty]
    private bool clearUncompletedEnabled = false;
    [ObservableProperty]
    private bool discordBatchEnabled = true;
    [ObservableProperty]
    private bool autoDiscordBatchEnabled = true;
    [ObservableProperty]
    private bool checkUpdatesEnabled = true;
    [ObservableProperty]
    private bool settingsEnabled = true;
    [ObservableProperty]
    private bool autoDiscordBatch;
    [ObservableProperty]
    private long populateHourLimit;
    [ObservableProperty]
    private string watchingDirectory = string.Empty;
    [ObservableProperty]
    private bool watchingDirectoryVisible;
    [ObservableProperty]
    private bool logTracesVisible;
    [ObservableProperty]
    private string version = string.Empty;
    private readonly ParserService _parserService;
    private readonly SettingsService _settingsService;
    private readonly Queue<LogFileViewModel> _logQueue = new();
    private int _runningCount;
    public ProgramSettings Settings { get; }
    public SettingsViewModel SettingsViewModel { get; }
    public bool AnyRunning => _runningCount > 0;

    public MainWindowViewModel()
    {
        _settingsService = new SettingsService();
        Settings = _settingsService.Load();
        _parserService = new ParserService(Settings);

        SettingsViewModel = new SettingsViewModel(Settings);
        SettingsViewModel.LoadFromSettings();
        SettingsViewModel.SettingsApplied += SettingsViewModel_SettingsApplied;

        AutoDiscordBatch = SettingsViewModel.AutoDiscordBatch;
        PopulateHourLimit = SettingsViewModel.PopulateHourLimit;
        LogTracesVisible = SettingsViewModel.SaveOutTrace;

        Version = $"v{typeof(MainWindowViewModel).Assembly.GetName().Version?.ToString() ?? string.Empty}";

        UpdateWatchDirectory();
        UpdateButtonStates();
    }

    private void SettingsViewModel_SettingsApplied(object? sender, EventArgs e)
    {
        _settingsService.Save(Settings);

        AutoDiscordBatch = SettingsViewModel.AutoDiscordBatch;
        PopulateHourLimit = SettingsViewModel.PopulateHourLimit;
        LogTracesVisible = SettingsViewModel.SaveOutTrace;

        UpdateWatchDirectory();
    }

    public void AddFile(string path)
    {
        if (LogFiles.Any(x => string.Equals(x.InputFilePath, path, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var logFileViewModel = new LogFileViewModel(path);

        logFileViewModel.ParseRequested += LogFile_ParseRequested;
        logFileViewModel.ReParseRequested += LogFile_ParseRequested;
        logFileViewModel.PendingCancellationRequested += LogFile_PendingCancellationRequested;
        logFileViewModel.RemoveRequested += LogFile_RemoveRequested;

        LogFiles.Add(logFileViewModel);
        UpdateQueueStatus();
        SettingsViewModel.AddApplicationTraceMessage("UI: Added " + logFileViewModel.InputFilePath);

        ParseEnabled = !AnyRunning;
        ClearAllEnabled = true;
        ClearUncompletedEnabled = LogFiles.Any(x => x.State == OperationState.UnComplete);

        if (SettingsViewModel.AutoParse)
        {
            QueueOrRunOperation(logFileViewModel);
        }
        UpdateButtonStates();
    }

    private void LogFile_ParseRequested(object? sender, EventArgs e)
    {
        if (sender is LogFileViewModel logFile)
        {
            QueueOrRunOperation(logFile);
        }
    }

    public void ParseAll()
    {
        _logQueue.Clear();

        if (LogFiles.Count == 0)
        {
            return;
        }

        UpdateButtonStates();

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
                UpdateLogState(logFile);
            }
            else if (queued.Contains(logFile))
            {
                logFile.Operation.ToReadyState();
                UpdateLogState(logFile);
            }
        }

        UpdateButtonStates();
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
                UpdateLogState(logFile);
            }
            else
            {
                LogFiles.RemoveAt(i);
                UpdateQueueStatus();
            }
        }

        UpdateButtonStates();
    }

    public void ClearUncompleted()
    {
        for (var i = LogFiles.Count - 1; i >= 0; i--)
        {
            var logFile = LogFiles[i];

            if (!logFile.IsBusy() && logFile.State == OperationState.UnComplete)
            {
                LogFiles.RemoveAt(i);
            }
        }

        UpdateButtonStates();
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
            WatchingDirectory = $"Watching for log files in '{SettingsViewModel.AutoAddPath}'";
            WatchingDirectoryVisible = true;

            AddFilesFromDirectory(SettingsViewModel.AutoAddPath);
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

        _ = AddDelayed(path);
    }

    public void HandleRenamedFile(string oldPath, string newPath)
    {
        if (ProgramHelper.IsTemporaryCompressedFormat(oldPath) && ProgramHelper.IsCompressedFormat(newPath))
        {
            _ = AddDelayed(newPath);
        }
        else if (ProgramHelper.IsTemporaryFormat(oldPath) && ProgramHelper.IsSupportedFormat(newPath))
        {
            _ = AddDelayed(newPath);
        }
    }

    private async Task AddDelayed(string path)
    {
        await Task.Delay(3000);

        if (File.Exists(path))
        {
            AddFile(path);
        }
    }

    private void QueueOrRunOperation(LogFileViewModel logFile)
    {
        UpdateButtonStates();

        if (!AnyRunning || (_parserService.ParseMultipleLogs() && _runningCount < _parserService.GetMaxParallelRunning()))
        {
            _ = RunOperationAsync(logFile);
        }
        else
        {
            _logQueue.Enqueue(logFile);
            logFile.Operation.ToPendingState();
            UpdateLogState(logFile);
        }
    }

    private async Task RunOperationAsync(LogFileViewModel logFile)
    {
        _parserService.ExecuteMemoryCheckTask();
        _runningCount++;

        logFile.Operation.ToQueuedState();
        UpdateLogState(logFile);
        SettingsViewModel.AddApplicationTraceMessage("Operation: Queued " + logFile.InputFilePath);

        logFile.Operation.ToRunState();
        UpdateLogState(logFile);
        SettingsViewModel.AddApplicationTraceMessage("Operation: Parsing " + logFile.InputFilePath);

        try
        {
            await _parserService.ParseAsync(logFile.Operation);

            if (logFile.State != OperationState.ClearOnCancel)
            {
                logFile.Operation.ToCompleteState();
                SettingsViewModel.AddApplicationTraceMessage("Operation: Parsed " + logFile.InputFilePath);
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
                UpdateQueueStatus();
            }
            else
            {
                UpdateLogState(logFile);
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

        UpdateButtonStates();
    }

    private void LogFile_PendingCancellationRequested(object? sender, EventArgs e)
    {
        if (sender is not LogFileViewModel logFile)
        {
            return;
        }

        var remainingOperations = _logQueue.Where(x => !ReferenceEquals(x, logFile)).ToList();
        _logQueue.Clear();

        foreach (var operation in remainingOperations)
        {
            _logQueue.Enqueue(operation);
        }

        logFile.Operation.ToReadyState();
        UpdateLogState(logFile);
    }

    private void LogFile_RemoveRequested(object? sender, EventArgs e)
    {
        if (sender is not LogFileViewModel logFile || logFile.IsBusy())
        {
            return;
        }

        LogFiles.Remove(logFile);
        UpdateQueueStatus();
        SettingsViewModel.AddApplicationTraceMessage("UI: Removed " + logFile.InputFilePath);
        ClearAllEnabled = LogFiles.Count > 0;
        ClearUncompletedEnabled = LogFiles.Any(x => x.State == OperationState.UnComplete);
        ParseEnabled = !AnyRunning && LogFiles.Count > 0;
    }

    public async Task<string> SendAllToDiscordAsync()
    {
        SettingsViewModel.AddApplicationTraceMessage("UI: Manual Discord Batch");

        DiscordBatchEnabled = false;

        try
        {
            var ids = new List<ulong>();
            var operations = LogFiles.Select(x => (OperationController)x.Operation).ToList();

            return await Task.Run(() =>
                _parserService.HandleBatchedDiscordEmbed(ids, operations, SettingsViewModel.AddApplicationTraceMessage));
        }
        finally
        {
            DiscordBatchEnabled = !AnyRunning;
        }
    }

    public void VersionLabelUpdate(bool isAvailable)
    {
        Version = isAvailable ? Version + " (Update Available)" : Version;
    }

    private void UpdateLogState(LogFileViewModel logFile)
    {
        logFile.UpdateFromOperation();
        UpdateQueueStatus();
    }

    private void UpdateQueueStatus()
    {
        var status = new List<string>();

        var queued = LogFiles.Count(x => x.State is OperationState.Pending or OperationState.Queued);
        var parsing = LogFiles.Count(x => x.State == OperationState.Parsing);
        var completed = LogFiles.Count(x => x.State == OperationState.Complete);

        if (queued > 0)
        {
            status.Add($"{queued} log(s) queued");
        }
        if (parsing > 0)
        {
            status.Add($"{parsing} log(s) parsing");
        }
        if (completed > 0)
        {
            status.Add($"{completed} log(s) completed");
        }

        QueueStatus = status.Count > 0 ? string.Join(", ", status) : "Waiting";
    }

    private void UpdateButtonStates()
    {
        bool hasLogs = LogFiles.Count > 0;
        bool hasUncompleted = LogFiles.Any(x => x.State == OperationState.UnComplete);
        bool running = AnyRunning;
        bool queued = _logQueue.Count > 0;

        ParseEnabled = hasLogs && !running;
        CancelAllEnabled = running || queued;
        ClearAllEnabled = hasLogs;
        ClearUncompletedEnabled = hasUncompleted;

        DiscordBatchEnabled = !running;
        AutoDiscordBatchEnabled = !running;
        CheckUpdatesEnabled = !running;
    }
}
