global using ScheduleKeeper.WPF.Types;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScheduleKeeper.WPF.Types;

public class ScheduleKeeperConfig : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Notify([CallerMemberName] string? caller = null)
        => PropertyChanged?.Invoke(this, new(caller));
    
    /// <summary>
    /// Throws if null, returns true if not the same
    /// </summary>
    protected static bool VerifyChange(object property, object value)
        => value is not null && !ReferenceEquals(property, value);


    /// <summary>
    /// Throws if null, returns true if not the same and thus should notify a change
    /// </summary>
    protected static bool SetAndVerifyChange<T>(ref T property, T value)
        => value is not null && !ReferenceEquals(property, property = value);
}
