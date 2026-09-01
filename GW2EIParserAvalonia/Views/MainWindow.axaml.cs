using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GW2EIParserAvalonia.Services;
using GW2EIParserAvalonia.ViewModels;
using GW2EIParserCommons.Properties;
using GW2EIUpdater;

namespace GW2EIParserAvalonia.Views;

public partial class MainWindow : Window
{
    private FileSystemWatcher? _logFileWatcher;

    public MainWindow()
    {
        InitializeComponent();

        UpdateFileWatcher();
        UpdaterInitialCheck();
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
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        viewModel.ParseAll();
        viewModel.SettingsViewModel.AddApplicationTraceMessage("UI: Parse all files");
    }

    private void CancelAllButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        viewModel.CancelAll();
        viewModel.SettingsViewModel.AddApplicationTraceMessage("UI: Cancelling all pending and ongoing parsing operations");
    }

    private void ClearAllButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        viewModel.ClearAll();
        viewModel.SettingsViewModel.AddApplicationTraceMessage("UI: Clearing all logs");
    }

    private void ClearUncompletedButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        viewModel.ClearUncompleted();
        viewModel.SettingsViewModel.AddApplicationTraceMessage("UI: Clearing uncompleted logs (failed to parse)");
    }

    private void UpdateFileWatcher()
    {
        _logFileWatcher?.Dispose();

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
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.HandleCreatedFile(e.FullPath);
            }
        });
    }

    private void LogFileWatcher_Renamed(object sender, RenamedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.HandleRenamedFile(e.OldFullPath, e.FullPath);
            }
        });
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

        var files = await picker.PickLogFilesAsync();

        foreach (var path in files)
        {
            viewModel.AddFile(path);
        }
    }

    private void UpdaterInitialCheck()
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

#if DEBUG
        long time = 0;
#else 
        long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
#endif
        if (time - Settings.Default.UpdateLastChecked > 3600)
        {
            Settings.Default.UpdateLastChecked = time;
            Task.Factory.StartNew(async () =>
            {
                List<string> traces = [];
                Updater.UpdateInfo? info = await Updater.CheckForUpdate("GW2EI.zip", traces);
                if (info != null)
                {
                    Settings.Default.UpdateAvailable = info.Value.UpdateAvailable;
                    viewModel.UpdateVersionLabel(info.Value.UpdateAvailable);
                }
                traces.ForEach(x => viewModel.SettingsViewModel.AddApplicationTraceMessage("Updater: " + x));
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }
        viewModel.UpdateVersionLabel(Settings.Default.UpdateAvailable);
    }

    private async void CheckUpdatesButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        viewModel.SettingsViewModel.AddApplicationTraceMessage("Updater: Checking for updates");

        var traces = new List<string>();
        Updater.UpdateInfo? info = await Updater.CheckForUpdate("GW2EI.zip", traces);
        traces.ForEach(x => viewModel.SettingsViewModel.AddApplicationTraceMessage("Updater: " + x));
#if DEBUG
        var force = true;
#else
        var force = false;
#endif
        if (info is null)
        {
            viewModel.SettingsViewModel.AddApplicationTraceMessage("Updater: UpdateInfo is null");
            return;
        }

        if (info.Value.UpdateAvailable || force)
        {
            viewModel.SettingsViewModel.AddApplicationTraceMessage("Updater: Update found, opening UI");
            Settings.Default.UpdateAvailable = info.Value.UpdateAvailable;
            Settings.Default.Save();
            viewModel.UpdateVersionLabel(info.Value.UpdateAvailable);

            var updaterWindow = new UpdaterWindow(info.Value);
            updaterWindow.UpdateStarted += (_, _) =>
            {
                updaterWindow.Close();
            };

            await updaterWindow.ShowDialog(this);
        }
        else
        {
            viewModel.SettingsViewModel.AddApplicationTraceMessage("Updater: Up to date");
            Settings.Default.UpdateAvailable = false;
            Settings.Default.Save();
            viewModel.UpdateVersionLabel(false);

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
