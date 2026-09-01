using System;
using System.Collections.Generic;
using System.Threading;
using GW2EIParserAvalonia.ViewModels;
using GW2EIParserCommons;

namespace GW2EIParserAvalonia.Services;

public sealed class AvaloniaOperationController : OperationController
{
    public string ButtonText { get; private set; } = "Parse";
    public string ReParseText { get; private set; } = "N/A";
    public bool ReParseEnabled { get; private set; } = false;
    public bool LogTracesEnabled { get; private set; } = false;
    public OperationState State { get; private set; } = OperationState.Ready;
    private CancellationTokenSource _cancellationTokenSource = new();
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;
    public event EventHandler? ProgressUpdated;

    private static readonly Dictionary<OperationState, (string ActionButton, string ReParseButton, bool ReParseEnabled, bool LogTraces)> _states = new()
    {
        [OperationState.Ready] = ("Parse", "N/A", false, false),
        [OperationState.Queued] = ("Cancel", "N/A", false, false),
        [OperationState.Pending] = ("Cancel", "N/A", false, false),
        [OperationState.Parsing] = ("Cancel", "N/A", false, false),
        [OperationState.Cancelling] = ("Cancelling", "N/A", false, false),
        [OperationState.ClearOnCancel] = ("Cancelling", "N/A", false, false),
        [OperationState.Complete] = ("Open", "Re-Parse", true, true),
        [OperationState.UnComplete] = ("Parse", "N/A", false, false),
    };

    public AvaloniaOperationController(string location) : base(location, "Ready to parse")
    {

    }

    private void SetState(OperationState operationState)
    {
        State = operationState;
        var dictState = _states[operationState];

        ButtonText = dictState.ActionButton;
        ReParseText = dictState.ReParseButton;
        ReParseEnabled = dictState.ReParseEnabled;
        LogTracesEnabled = dictState.LogTraces;
    }

    public void ToReadyState()
    {
        SetState(OperationState.Ready);
    }

    public void ToQueuedState()
    {
        SetState(OperationState.Queued);
    }

    public void ToPendingState()
    {
        SetState(OperationState.Pending);
    }

    public void ToRunState()
    {
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        SetState(OperationState.Parsing);
        UpdateProgress("Parsing");
    }

    public void ToCancelState()
    {
        SetState(OperationState.Cancelling);
        _cancellationTokenSource.Cancel();
    }

    public void ToCancelAndClearState()
    {
        SetState(OperationState.ClearOnCancel);
        _cancellationTokenSource.Cancel();
    }

    public void ToCompleteState()
    {
        SetState(OperationState.Complete);
        FinalizeStatus(true);
    }

    public void ToUnCompleteState()
    {
        SetState(OperationState.UnComplete);
        FinalizeStatus(false);
    }

    public override void Reset()
    {
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        base.Reset();
        ToReadyState();
    }

    protected override void ThrowIfCanceled()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
        }
    }

    public override void UpdateProgress(string status)
    {
        base.UpdateProgress(status);
        Status = status;
        ProgressUpdated?.Invoke(this, EventArgs.Empty);
    }
}
