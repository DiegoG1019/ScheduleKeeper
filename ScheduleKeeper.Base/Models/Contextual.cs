using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleKeeper.Base.Models;

public abstract class Contextual : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Notify([CallerMemberName] string? caller = null) => PropertyChanged?.Invoke(this, new(caller));
}

public abstract class DescribedContextual : Contextual
{
    private string _title;
    private string? _description;
    private ObservableCollection<MutableKeyValuePair<string, string>> _notes = new();

    public string Title
    {
        get => _title;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), $"{nameof(Title)} cannot be set to null.");
            if (value == _title)
                return;
            _title = value;
            Notify();
        }
    }

    public string? Description
    {
        get => _description;
        set
        {
            if (value == _description)
                return;
            _description = value;
            Notify();
        }
    }

    public ObservableCollection<MutableKeyValuePair<string, string>> Notes
    {
        get => _notes;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), $"{nameof(Notes)} cannot be set to null.");
            if (ReferenceEquals(_notes, value))
                return;
            _notes = value;
            Notify();
        }
    }

    public DescribedContextual(string title) => _title = title;
}
