using System.Threading;
using GW2EIParserAvalonia.ViewModels;
using GW2EIParserCommons;

namespace GW2EIParserAvalonia.Services;

public sealed class AvaloniaOperationController : OperationController
{
    public string ButtonText { get; private set; } = "Parse";
    public string ReParseText { get; private set; } = "N/A";
    public OperationState State { get; private set; } = OperationState.Ready;
    private CancellationTokenSource _cancellationTokenSource = new();
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public AvaloniaOperationController(string location) : base(location, "Ready to parse")
    {

    }

    public void ToQueuedState()
    {
        State = OperationState.Queued;
        ButtonText = "Cancel";
        ReParseText = "N/A";
    }

    public void ToPendingState()
    {
        State = OperationState.Pending;
        ButtonText = "Cancel";
        ReParseText = "N/A";
    }

    public void ToReadyState()
    {
        State = OperationState.Ready;
        ButtonText = "Parse";
        ReParseText = "N/A";
    }

    public void ToRunState()
    {
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        State = OperationState.Parsing;
        ButtonText = "Cancel";
        ReParseText = "N/A";

        UpdateProgress("Parsing");
    }

    public void ToCancelState()
    {
        State = OperationState.Cancelling;
        ButtonText = "Cancelling";
        ReParseText = "N/A";

        _cancellationTokenSource.Cancel();
    }

    public void ToCancelAndClearState()
    {
        State = OperationState.ClearOnCancel;
        ButtonText = "Cancelling";
        ReParseText = "N/A";

        _cancellationTokenSource.Cancel();
    }

    public void ToCompleteState()
    {
        State = OperationState.Complete;
        ButtonText = "Open";
        ReParseText = "Re-Parse";

        FinalizeStatus(true);
    }

    public void ToUnCompleteState()
    {
        State = OperationState.UnComplete;
        ButtonText = "Parse";
        ReParseText = "N/A";

        FinalizeStatus(false);
    }

    public void ToCancelledState()
    {
        State = OperationState.UnComplete;
        ButtonText = "Parse";
        ReParseText = "N/A";
    }

    public override void Reset()
    {
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        base.Reset();

        State = OperationState.Ready;
        ButtonText = "Parse";
        ReParseText = "N/A";
    }

    protected override void ThrowIfCanceled()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
        }
    }

    public void ToRemovalFromQueueState()
    {
        ToCancelState();
    }
}
