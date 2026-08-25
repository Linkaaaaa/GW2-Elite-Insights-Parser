using CommunityToolkit.Mvvm.ComponentModel;

namespace GW2EIParserAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "EI 4.0";
}
