using Avalonia.Controls;
using Avalonia.Interactivity;
using GW2EIParserAvalonia.ViewModels;

namespace GW2EIParserAvalonia.Views;

public partial class SettingsWindow : Window
{
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
}
