using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using GW2EIParserAvalonia.Services;
using GW2EIParserAvalonia.ViewModels;
using GW2EIParserCommons;

namespace GW2EIParserAvalonia.Views;

public partial class SettingsWindow : Window
{
    private readonly IApplicationTrace _trace = null!;

    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(SettingsViewModel viewModel, IApplicationTrace trace)
    {
        InitializeComponent();

        DataContext = viewModel;
        _trace = trace;
        viewModel.AutoAddFolderRequested += ViewModel_AutoAddFolderRequested;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        _trace.Add("UI: Settings closed");
        Close();
    }

    private async void LoadButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel viewModel)
        {
            return;
        }

        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Load a Configuration file",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Conf file")
                    {
                        Patterns = [ "*.conf" ]
                    }
                }
            });

        if (files.Count > 0)
        {
            viewModel.ApplyLoadedSettings(files[0].Path.LocalPath);
            _trace.Add($"UI: Settings loaded from {files[0].Path.LocalPath}");
        }
    }

    private async void ViewModel_AutoAddFolderRequested(object? sender, EventArgs e)
    {
        if (DataContext is not SettingsViewModel viewModel)
        {
            return;
        }

        var picker = new FilePickerService(StorageProvider);

        var path = await picker.PickFolderAsync();

        if (string.IsNullOrWhiteSpace(path))
        {
            viewModel.AutoAddPath = string.Empty;
            viewModel.AutoAdd = false;
            return;
        }
        viewModel.AutoAddPath = path;
        viewModel.AutoAdd = true;
    }

    private async void SelectFolder_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel viewModel)
        {
            return;
        }

        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select output directory",
                AllowMultiple = false
            });

        var folder = folders.FirstOrDefault();

        if (folder != null)
        {
            viewModel.OutLocation = folder.Path.LocalPath;
        }
    }

    private async void ResetMapButton_Click(object sender, RoutedEventArgs e)
    {
        ProgramHelper.APIController.WriteAPIMapsToFile(ProgramHelper.MapAPICacheLocation);
        var messageWindow = new MessageWindow("Map List has been redone", _trace);
        await messageWindow.ShowDialog(this);
    }

    private async void ResetSkillButton_Click(object sender, RoutedEventArgs e)
    {
        ProgramHelper.APIController.WriteAPISkillsToFile(ProgramHelper.SkillAPICacheLocation);
        var messageWindow = new MessageWindow("Skill List has been redone", _trace);
        await messageWindow.ShowDialog(this);
    }

    private async void ResetTraitButton_Click(object sender, RoutedEventArgs e)
    {
        ProgramHelper.APIController.WriteAPITraitsToFile(ProgramHelper.TraitAPICacheLocation);
        var messageWindow = new MessageWindow("Trait List has been redone", _trace);
        await messageWindow.ShowDialog(this);
    }

    private async void ResetSpecButton_Click(object sender, RoutedEventArgs e)
    {
        ProgramHelper.APIController.WriteAPISpecsToFile(ProgramHelper.SpecAPICacheLocation);
        var messageWindow = new MessageWindow("Spec List has been redone", _trace);
        await messageWindow.ShowDialog(this);
    }
}
