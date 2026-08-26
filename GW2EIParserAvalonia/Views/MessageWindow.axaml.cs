using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GW2EIParserAvalonia.Views;

public partial class MessageWindow : Window
{
    public string Message { get; }

    public MessageWindow()
    {
        InitializeComponent();
    }

    public MessageWindow(string title, string message)
    {
        InitializeComponent();

        Title = title;
        Message = message;

        DataContext = this;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
