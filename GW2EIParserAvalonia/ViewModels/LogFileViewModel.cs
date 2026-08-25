using System;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GW2EIParserAvalonia.Services;

namespace GW2EIParserAvalonia.ViewModels;

public partial class LogFileViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputFile;
    [ObservableProperty]
    private string status;
    [ObservableProperty]
    private string buttonText;
    [ObservableProperty]
    private string reParseText;
    [ObservableProperty]
    private OperationState state;
    public AvaloniaOperationController Operation { get; }
    public event EventHandler? ParseRequested;
    public event EventHandler? ReParseRequested;

    public LogFileViewModel(string fullPath)
    {
        inputFile = fullPath;
        Operation = new AvaloniaOperationController(fullPath);
        status = Operation.Status;
        buttonText = Operation.ButtonText;
        reParseText = Operation.ReParseText;
        state = Operation.State;
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
                RequestParse();
                break;
            case OperationState.Parsing:
                Operation.Cancel();
                UpdateFromOperation();
                break;
            case OperationState.Pending:
            case OperationState.Queued:
                Operation.ToReadyState();
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
            RequestReParse();
        }
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

    private static void OpenWithDefaultApplication(string path)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
    }

    public void RequestParse()
    {
        ParseRequested?.Invoke(this, EventArgs.Empty);
    }

    public void RequestReParse()
    {
        ReParseRequested?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateFromOperation()
    {
        Status = Operation.Status;
        ButtonText = Operation.ButtonText;
        ReParseText = Operation.ReParseText;
        State = Operation.State;
    }
}
