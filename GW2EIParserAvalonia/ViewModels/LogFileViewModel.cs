using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GW2EIParserAvalonia.Services;

namespace GW2EIParserAvalonia.ViewModels;

public partial class LogFileViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputFilePath;
    [ObservableProperty]
    private string logStatus;
    [ObservableProperty]
    private string buttonText;
    [ObservableProperty]
    private string reParseText;
    [ObservableProperty]
    private bool removeEnabled;
    [ObservableProperty]
    private bool reParseEnabled;
    [ObservableProperty]
    private bool logTracesEnabled;
    [ObservableProperty]
    private OperationState state;
    public AvaloniaOperationController Operation { get; }
    public event EventHandler? ParseRequested;
    public event EventHandler? ReParseRequested;
    public event EventHandler? PendingCancellationRequested;
    public event EventHandler? RemoveRequested;

    public LogFileViewModel(string fullPath)
    {
        inputFilePath = fullPath;
        Operation = new AvaloniaOperationController(inputFilePath);
        logStatus = Operation.Status;
        buttonText = Operation.ButtonText;
        reParseText = Operation.ReParseText;
        state = Operation.State;
        removeEnabled = true;
        reParseEnabled = Operation.ReParseEnabled;
        logTracesEnabled = Operation.LogTracesEnabled;
        Operation.ProgressUpdated += Operation_ProgressUpdated;
    }

    public bool IsBusy()
    {
        return State is
            OperationState.Queued
            or OperationState.Pending
            or OperationState.Parsing
            or OperationState.Cancelling
            or OperationState.ClearOnCancel;
    }

    [RelayCommand]
    private void Action()
    {
        switch (State)
        {
            case OperationState.Ready:
            case OperationState.UnComplete:
                ParseRequested?.Invoke(this, EventArgs.Empty);
                break;
            case OperationState.Parsing:
                Operation.ToCancelState();
                UpdateFromOperation();
                break;
            case OperationState.Pending:
                PendingCancellationRequested?.Invoke(this, EventArgs.Empty);
                break;
            case OperationState.Queued:
                Operation.ToRemovalFromQueueState();
                UpdateFromOperation();
                break;
            case OperationState.Complete:
                OpenGeneratedFiles();
                break;
        }
    }

    [RelayCommand]
    private void ReParse()
    {
        if (State == OperationState.Complete)
        {
            ReParseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void Remove()
    {
        if (!RemoveEnabled)
        {
            return;
        }

        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void OpenTracesCommand()
    {
        OpenGeneratedLogTracesFiles();
    }

    private void OpenGeneratedFiles()
    {
        foreach (var path in Operation.OpenableFiles)
        {
            if (File.Exists(path))
            {
                OpenWithDefaultApplication(path);
            }
        }

        if (Operation.OpenableFiles.Count < Operation.GeneratedFiles.Count && Operation.OutLocation != null && Directory.Exists(Operation.OutLocation))
        {
            OpenWithDefaultApplication(Operation.OutLocation);
        }
    }

    private void OpenGeneratedLogTracesFiles()
    {
        foreach (var path in Operation.OpenableLogTracesFiles)
        {
            if (File.Exists(path))
            {
                OpenWithDefaultApplication(path);
            }
        }

        if (Operation.OpenableLogTracesFiles.Count < Operation.OpenableLogTracesFiles.Count && Operation.OutLocation != null && Directory.Exists(Operation.OutLocation))
        {
            OpenWithDefaultApplication(Operation.OutLocation);
        }
    }

    private static void OpenWithDefaultApplication(string path)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
    }

    public void UpdateFromOperation()
    {
        LogStatus = Operation.Status;
        ButtonText = Operation.ButtonText;
        ReParseText = Operation.ReParseText;
        State = Operation.State;
        ReParseEnabled = Operation.ReParseEnabled;
        LogTracesEnabled = Operation.LogTracesEnabled;
        RemoveEnabled = !IsBusy();
    }

    private void Operation_ProgressUpdated(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            LogStatus = Operation.Status;
        });
    }
}
