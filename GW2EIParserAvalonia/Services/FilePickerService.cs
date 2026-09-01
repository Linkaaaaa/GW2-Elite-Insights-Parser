using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using GW2EIEvtcParser.ParserHelpers;

namespace GW2EIParserAvalonia.Services;

public class FilePickerService
{
    private readonly IStorageProvider _storageProvider;

    public FilePickerService(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<IReadOnlyList<string>> PickLogFilesAsync()
    {
        var files = await _storageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Select GW2 EVTC Combat Logs",
                AllowMultiple = true,
                FileTypeFilter =
                [
                    new FilePickerFileType("GW2 EVTC Combat Logs")
                    {
                        Patterns = SupportedFileFormats.SupportedFormats.Select(format => $"*{format}").ToList()
                    }
                ]
            });

        return files.Select(file => file.TryGetLocalPath()).OfType<string>().ToList();
    }

    public async Task<string?> PickFolderAsync()
    {
        var folders = await _storageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select auto-add directory",
                AllowMultiple = false
            });

        return folders.FirstOrDefault()?.TryGetLocalPath();
    }
}
