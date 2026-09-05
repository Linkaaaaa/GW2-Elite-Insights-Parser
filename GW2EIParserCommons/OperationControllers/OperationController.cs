using System.Diagnostics;
using GW2EIEvtcParser;
using GW2EIEvtcParser.Exceptions;
using GW2EIEvtcParser.LogLogic;

namespace GW2EIParserCommons;


public abstract class OperationController : ParserController
{

    public class OperationBasicMetaData
    {
        public OperationBasicMetaData(ParsedEvtcLog log)
        {
            LogDuration = log.LogData.DurationString;
            LogName = log.LogData.LogName;
            Success = log.LogData.GetMainPhase(log).Success;
            LogCategory = log.LogData.Logic.LogCategoryInformation;
            Icon = log.LogData.Logic.Icon;
            LogStart = log.LogMetadata.DateStartStd;
            LogEnd = log.LogMetadata.DateEndStd;
        }

        public readonly string LogDuration;
        public readonly string LogName;
        public bool Success { get; set; }
        public readonly LogCategories LogCategory;
        public readonly string Icon;
        public readonly string LogStart;
        public readonly string LogEnd;
    }

    public enum FailureReason
    {
        NotApplicable,
        // Parsing settings related
        Setting,
        // User interaction related
        User,
        // Evtc file content related
        FileContent,
        Fatal,
    }

    public static FailureReason GetReasonFromException(Exception? exception)
    {
        if (exception == null)
        {
            return FailureReason.Fatal;
        }
        var finalException = ParserHelper.GetFinalException(exception);
        FailureReason reason = FailureReason.Fatal;
        if (finalException is EINonFatalException)
        {
            reason = FailureReason.Setting;
        }
        else if (finalException is OperationCanceledException)
        {
            reason = FailureReason.User;
        }
        else if (finalException is EvtcContentException)
        {
            reason = FailureReason.FileContent;
        }
        return reason;
    }

    /// <summary>
    /// Status of the parse operation
    /// </summary>
    public string Status { get; protected set; }
    /// <summary>
    /// Wether file was successfully parsed or not.
    /// Only relevant when <see cref="Executed"/> is true.
    /// </summary>
    public bool Parsed { get; protected set; }
    /// <summary>
    /// The reason for which parsing could not finish.
    /// Only relevant when <see cref="Executed"/> is true && <see cref="Parsed" is false/>.
    /// </summary>
    public FailureReason Reason { get; protected set; }
    /// <summary>
    /// Indicates that this operation has already executed.
    /// </summary>
    public bool Executed { get; protected set; }

    /// <summary>
    /// Location of the file being parsed
    /// </summary>
    public string InputFile { get; }
    /// <summary>
    /// Location of the output
    /// </summary>
    public string? OutLocation { get; internal set; }

    private readonly List<string> _GeneratedFiles;
    /// <summary>
    /// Location of the generated files
    /// </summary>
    public IReadOnlyList<string> GeneratedFiles => _GeneratedFiles;

    private readonly List<string> _OpenableFiles;
    /// <summary>
    /// Location of the openable files
    /// </summary>
    public IReadOnlyList<string> OpenableFiles => _OpenableFiles;
    private readonly List<string> _OpenableLogTracesFiles;
    /// <summary>
    /// Location of the openable log trace files
    /// </summary>
    public IReadOnlyList<string> OpenableLogTracesFiles => _OpenableLogTracesFiles;
    /// <summary>
    /// Link to dps.report
    /// </summary>
    public string? DPSReportLink { get; internal set; }

    public bool DPSReportUploadTentative { get; internal set; }
    public bool DPSReportUploadFailed { get; internal set; }

    public bool WingmanUploadTentative { get; internal set; }
    public bool WingmanUploadFailed { get; internal set; }

    public bool WingmanUploadRefused { get; internal set; }

    public OperationBasicMetaData? BasicMetaData { get; set; }

    /// <summary>
    /// Time elapsed parsing
    /// </summary>
    public long Elapsed { get; private set; } = 0;


    private readonly Stopwatch _stopWatch = new();

    protected OperationController(string location, string status)
    {
        Status = status;
        InputFile = location;
        _GeneratedFiles = [];
        _OpenableFiles = [];
        _OpenableLogTracesFiles = [];
    }
    public override void ResetContent()
    {
        base.ResetContent();
        BasicMetaData = null;
        DPSReportLink = null;
        OutLocation = null;
        DPSReportUploadTentative = false;
        DPSReportUploadFailed = false;
        WingmanUploadTentative = false;
        WingmanUploadFailed = false;
        WingmanUploadRefused = false;
        _GeneratedFiles.Clear();
        _OpenableFiles.Clear();
        _OpenableLogTracesFiles.Clear();
    }

    public override void ResetState()
    {
        base.ResetState();
        Elapsed = 0;
        Executed = false;
        Parsed = false;
        Reason = FailureReason.NotApplicable;
    }

    public void Start()
    {
        _stopWatch.Restart();
        _stopWatch.Start();
    }

    public void Stop()
    {
        _stopWatch.Stop();
        Elapsed = _stopWatch.ElapsedMilliseconds;
        _stopWatch.Restart();
    }

    public void AddOpenableFile(string path)
    {
        _GeneratedFiles.Add(path);
        _OpenableFiles.Add(path);
    }

    public void AddOpenableLogTraceFile(string path)
    {
        _GeneratedFiles.Add(path);
        _OpenableLogTracesFiles.Add(path);
    }

    public void AddFile(string path)
    {
        _GeneratedFiles.Add(path);
    }

    public virtual void FinalizeStatus(bool parsed, FailureReason reason)
    {
        StatusList.Insert(0, ("Elapsed " + Elapsed + " ms"));
        Status = StatusList.LastOrDefault() ?? "";
        Parsed = parsed;
        Reason = reason;
        Executed = true;
        string prefix = parsed ? "Parsing Successful - " : "Parsing Failure - ";
        foreach (string generatedFile in GeneratedFiles)
        {
            Console.WriteLine("Generated" + $": {generatedFile}" + Environment.NewLine);
        }
        Console.WriteLine(prefix + $"{InputFile}: {Status}" + Environment.NewLine);
    }
}
