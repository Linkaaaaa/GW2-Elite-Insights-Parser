using System;
using System.IO;
using GW2EIParserCommons;
using GW2EIParserCommons.Properties;

namespace GW2EIParserAvalonia.Services;

public sealed class ApplicationTrace : IApplicationTrace
{
    private readonly string _traceFileName;

    public ApplicationTrace()
    {
        _traceFileName = $"{ProgramHelper.EILogPath}EILogs-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt";
    }

    public void Add(string message)
    {
        if (!Settings.Default.ApplicationTraces)
        {
            return;
        }

        if (!Directory.Exists(ProgramHelper.EILogPath))
        {
            Directory.CreateDirectory(ProgramHelper.EILogPath);
        }

        File.AppendAllText(_traceFileName, message + Environment.NewLine);
    }
}
