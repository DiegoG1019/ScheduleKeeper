using ScheduleKeeper.Base.Models;
using System.Windows;
using System.Windows.Controls;

namespace ScheduleKeeper.WPF.Controls;
/// <summary>
/// Interaction logic for ScheduleView.xaml
/// </summary>
public partial class ScheduleView : UserControl
{
    private Schedule? _dat;
    private Schedule Context => _dat ??= (DataContext as Schedule) ?? throw new InvalidOperationException($"DataContext is not a {nameof(Schedule)}");

    private void DataContextChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        => _dat = null;

    public ScheduleView()
    {
        InitializeComponent();
        DataContextChanged += DataContextChangedHandler;
    }
}
