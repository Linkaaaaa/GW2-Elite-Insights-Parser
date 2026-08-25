using System;
using System.Threading.Tasks;
using GW2EIParserCommons;

namespace GW2EIParserAvalonia.Services;

public sealed class ParserService
{
    private readonly ProgramHelper _programHelper;
    public ProgramSettings Settings { get; }

    public ParserService(ProgramSettings settings)
    {
        Settings = settings;
        _programHelper = new ProgramHelper(new Version(3, 26, 0, 0), Settings);
    }

    public void ApplySettings()
    {
        _programHelper.ApplySettings(Settings);
    }

    public Task ParseAsync(AvaloniaOperationController operation)
    {
        return Task.Run(() => _programHelper.DoWork(operation), operation.CancellationToken);
    }
}
