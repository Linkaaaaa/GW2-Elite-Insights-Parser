using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GW2EIParserCommons;
using static GW2EIParserCommons.ProgramHelper;

namespace GW2EIParserAvalonia.Services;

public sealed class ParserService
{
    private readonly ProgramHelper _programHelper;
    public ProgramSettings Settings { get; }

    public ParserService(ProgramSettings settings)
    {
        Settings = settings;
        var version = typeof(App).Assembly.GetName().Version ?? new Version(0, 0, 0, 0);
        _programHelper = new ProgramHelper(version, Settings);
    }

    public void ApplySettings()
    {
        _programHelper.ApplySettings(Settings);
    }

    public Task ParseAsync(AvaloniaOperationController operation)
    {
        return Task.Run(() => _programHelper.DoWork(operation), operation.CancellationToken);
    }

    public bool ParseMultipleLogs()
    {
        return _programHelper.ParseMultipleLogs();
    }

    public int GetMaxParallelRunning()
    {
        return _programHelper.GetMaxParallelRunning();
    }

    public void ExecuteMemoryCheckTask()
    {
        _programHelper.ExecuteMemoryCheckTask();
    }

    public void GenerateTraceFile(AvaloniaOperationController operation)
    {
        _programHelper.GenerateTraceFile(operation);
    }

    public string HandleBatchedDiscordEmbed(List<ulong> ids, List<OperationController> operations, BatchedDiscordTraceHandler traceHandler)
    {
        return _programHelper.HandleBatchedDiscordEmbed(ids, operations, traceHandler);
    }
}
