namespace GW2EIParserAvalonia.ViewModels;

public enum OperationState
{
    Ready,
    Parsing,
    Cancelling,
    Complete,
    Pending,
    ClearOnCancel,
    Queued,
    UnComplete
}
