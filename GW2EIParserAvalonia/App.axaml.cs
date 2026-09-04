using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GW2EIParserAvalonia.Services;
using GW2EIParserAvalonia.ViewModels;
using GW2EIParserAvalonia.Views;

namespace GW2EIParserAvalonia;

public partial class App : Application
{
    public CommandLineOptions? CommandLineOptions { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            CommandLineOptions = new CommandWindow(desktop.Args).Initialize();
            if (CommandLineOptions is null)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Dispatcher.UIThread.Post(() => desktop.Shutdown(0));

                base.OnFrameworkInitializationCompleted();
                return;
            }

            ApplicationTrace trace = new();

            desktop.MainWindow = new MainWindow(trace)
            {
                DataContext = new MainWindowViewModel(trace, CommandLineOptions),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
