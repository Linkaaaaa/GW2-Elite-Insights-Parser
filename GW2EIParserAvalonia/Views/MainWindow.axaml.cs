using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using GW2EIParserAvalonia.Services;
using GW2EIParserAvalonia.ViewModels;
using GW2EIParserCommons.Properties;

namespace GW2EIParserAvalonia.Views;

public partial class MainWindow : Window
{
    private FileSystemWatcher? _logFileWatcher;

    public MainWindow()
    {
        InitializeComponent();

        UpdateFileWatcher();
    }

    private async void AddFilesButton_Click(object? sender, RoutedEventArgs e)
    {
        FilePicker();
    }

    private async void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var window = new SettingsWindow(viewModel.SettingsViewModel);
        await window.ShowDialog(this);

        UpdateFileWatcher();
    }

    private async void PopulateButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select log directory",
                AllowMultiple = false
            });

        var folder = folders.FirstOrDefault();

        if (folder is null)
        {
            return;
        }

        var path = folder.TryGetLocalPath();

        if (!string.IsNullOrWhiteSpace(path))
        {
            viewModel.AddFilesFromDirectory(path);
        }
    }

    private void ParseAllButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.ParseAll();
        }
    }

    private void CancelAllButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.CancelAll();
        }
    }

    private void ClearAllButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.ClearAll();
        }
    }

    private void ClearFailedButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.ClearFailed();
        }
    }

    private void UpdateFileWatcher()
    {
        _logFileWatcher?.Dispose();
        _logFileWatcher = null;

        if (!Settings.Default.AutoAdd || !Directory.Exists(Settings.Default.AutoAddPath))
        {
            return;
        }

        _logFileWatcher = new FileSystemWatcher(Settings.Default.AutoAddPath)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        _logFileWatcher.Created += LogFileWatcher_Created;
        _logFileWatcher.Renamed += LogFileWatcher_Renamed;
    }

    private void LogFileWatcher_Created(object sender, FileSystemEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.HandleCreatedFile(e.FullPath);
        }
    }

    private void LogFileWatcher_Renamed(object sender, RenamedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.HandleRenamedFile(e.OldFullPath, e.FullPath);
        }
    }

    private void MainWindow_DragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Formats.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void MainWindow_Drop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();

        if (files is null)
        {
            return;
        }

        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();

            if (path is not null)
            {
                viewModel.AddFile(path);
            }
        }
    }

    private async void LogFilesGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        FilePicker();
    }

    public async void FilePicker()
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var picker = new FilePickerService(StorageProvider);

        var files = await picker.PickCombatLogsAsync();

        foreach (var path in files)
        {
            viewModel.AddFile(path);
        }
    }

    private async void CheckUpdatesButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var traces = new List<string>();
        var info = await GW2EIUpdater.Updater.CheckForUpdate("GW2EI.zip", traces);

        foreach (var trace in traces)
        {
            //viewModel.AddTraceMessage("Updater: " + trace);
        }

        if (info is null)
        {
            return;
        }

        Settings.Default.UpdateAvailable = info.Value.UpdateAvailable;
        Settings.Default.Save();

        if (info.Value.UpdateAvailable)
        {
            var updaterWindow = new UpdaterWindow(info.Value);
            updaterWindow.UpdateStarted += (_, _) =>
            {
                updaterWindow.Close();
                Close();
            };

            await updaterWindow.ShowDialog(this);
        }
        else
        {
            Settings.Default.UpdateAvailable = false;
            Settings.Default.Save();

            var messageWindow = new MessageWindow("Elite Insights is up to date.");

            await messageWindow.ShowDialog(this);
        }
    }

    private async void SendAllToDiscordButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var message = await viewModel.SendAllToDiscordAsync();

        var messageWindow = new MessageWindow(message);

        await messageWindow.ShowDialog(this);
    }
}
