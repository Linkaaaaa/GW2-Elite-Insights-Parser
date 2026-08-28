using System;
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
    public event EventHandler? AutoAddFolderRequested;

    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
        viewModel.AutoAddFolderRequested += ViewModel_AutoAddFolderRequested;
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
            viewModel.AutoAdd = false;
            return;
        }

        viewModel.AutoAddPath = path;
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
        var messageWindow = new MessageWindow("Map List has been redone");
        await messageWindow.ShowDialog(this);
    }

    private async void ResetSkillButton_Click(object sender, RoutedEventArgs e)
    {
        ProgramHelper.APIController.WriteAPISkillsToFile(ProgramHelper.SkillAPICacheLocation);
        var messageWindow = new MessageWindow("Skill List has been redone");
        await messageWindow.ShowDialog(this);
    }

    private async void ResetTraitButton_Click(object sender, RoutedEventArgs e)
    {
        ProgramHelper.APIController.WriteAPITraitsToFile(ProgramHelper.TraitAPICacheLocation);
        var messageWindow = new MessageWindow("Trait List has been redone");
        await messageWindow.ShowDialog(this);
    }

    private async void ResetSpecButton_Click(object sender, RoutedEventArgs e)
    {
        ProgramHelper.APIController.WriteAPISpecsToFile(ProgramHelper.SpecAPICacheLocation);
        var messageWindow = new MessageWindow("Spec List has been redone");
        await messageWindow.ShowDialog(this);
    }
}
