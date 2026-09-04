using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GW2EIParserAvalonia.Services;
using GW2EIParserAvalonia.ViewModels;
using GW2EIParserAvalonia.Views;

namespace GW2EIParserAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ApplicationTrace trace = new();

            desktop.MainWindow = new MainWindow(trace)
            {
                DataContext = new MainWindowViewModel(trace),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
