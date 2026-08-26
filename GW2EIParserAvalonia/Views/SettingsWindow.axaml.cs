using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using GW2EIParserAvalonia.ViewModels;

namespace GW2EIParserAvalonia.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }

    private void ApplyButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel viewModel)
        {
            viewModel.ApplyToSettings();
        }

        Close();
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void AutoAddPathButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel viewModel)
        {
            return;
        }

        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select auto-add directory",
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
            viewModel.AutoAddPath = path;
        }
    }
}
