using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using GW2EIParserAvalonia.ViewModels;

namespace GW2EIParserAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void AddFilesButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Select GW2 combat logs",
                AllowMultiple = true,

                FileTypeFilter =
                [
                    new FilePickerFileType("GW2 Combat Logs")
                    {
                        Patterns =
                        [
                            "*.evtc",
                            "*.evtc.zip",
                            "*.zevtc"
                        ]
                    }
                ]
            });

        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();

            if (path is not null)
            {
                viewModel.AddFile(path);
            }
        }
    }

    private async void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var window = new SettingsWindow(viewModel.SettingsViewModel);
        await window.ShowDialog(this);
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
}
