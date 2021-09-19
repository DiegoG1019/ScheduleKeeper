using ScheduleKeeper.Base.Models;
using System.Windows;

namespace ScheduleKeeper.WPF.Controls;
/// <summary>
/// Interaction logic for TimeFramePlan.xaml
/// </summary>
public partial class TimeFramePlanView 
{
    private bool _showDuration;
    public bool ShowDuration
    {
        get => _showDuration;
        set
        {
            if (_showDuration != value)
                GridDuration.Height = (_showDuration = value) ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Star);
        }
    }

    public TimeFramePlanView()
    {
        InitializeComponent();
    }
}
