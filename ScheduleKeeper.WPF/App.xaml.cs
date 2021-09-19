using System.Windows;

namespace ScheduleKeeper.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public Language Language => Settings<Language>.Current;
}