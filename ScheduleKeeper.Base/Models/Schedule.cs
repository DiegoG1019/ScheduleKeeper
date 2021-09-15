using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleKeeper.Base.Models;

public class Schedule : DescribedContextual
{
    public IEnumerable<ScheduledEvent> this[DayOfWeek dayOfWeek] => GetEvents(dayOfWeek);

    private ObservableCollection<ScheduledEvent> _events = new();
    private IEnumerable<ScheduledEvent>? _sundayEvents;
    private IEnumerable<ScheduledEvent>? _mondayEvents;
    private IEnumerable<ScheduledEvent>? _tuesdayEvents;
    private IEnumerable<ScheduledEvent>? _wednesdayEvents;
    private IEnumerable<ScheduledEvent>? _thursdayEvents;
    private IEnumerable<ScheduledEvent>? _fridayEvents;
    private IEnumerable<ScheduledEvent>? _saturdayEvents;

    public ObservableCollection<ScheduledEvent> Events
    {
        get => _events;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), $"{nameof(Events)} cannot be set to null.");
            if (ReferenceEquals(_events, value))
                return;

            _events.CollectionChanged -= EventsCollectionChanged;
            _events = value;
            _events.CollectionChanged += EventsCollectionChanged;
            ClearEvents();
            Notify();
            foreach (var p in Enum.GetValues<DayOfWeek>())
                Notify(GetEventsDay(p));
        }
    }

    public IEnumerable<ScheduledEvent> SundayEvents => _sundayEvents ?? FetchEvents(DayOfWeek.Sunday, ref _sundayEvents);
    public IEnumerable<ScheduledEvent> MondayEvents => _mondayEvents ?? FetchEvents(DayOfWeek.Monday, ref _mondayEvents);
    public IEnumerable<ScheduledEvent> TuesdayEvents => _tuesdayEvents ?? FetchEvents(DayOfWeek.Tuesday, ref _tuesdayEvents);
    public IEnumerable<ScheduledEvent> WednesdayEvents => _wednesdayEvents ?? FetchEvents(DayOfWeek.Wednesday, ref _wednesdayEvents);
    public IEnumerable<ScheduledEvent> ThursdayEvents => _thursdayEvents ?? FetchEvents(DayOfWeek.Thursday, ref _thursdayEvents);
    public IEnumerable<ScheduledEvent> FridayEvents => _fridayEvents ?? FetchEvents(DayOfWeek.Friday, ref _fridayEvents);
    public IEnumerable<ScheduledEvent> SaturdayEvents => _saturdayEvents ?? FetchEvents(DayOfWeek.Saturday, ref _saturdayEvents);

    public IEnumerable<ScheduledEvent> GetEvents(DayOfWeek dayOfWeek)
        => dayOfWeek switch
        {
            DayOfWeek.Sunday => SundayEvents,
            DayOfWeek.Monday => MondayEvents,
            DayOfWeek.Tuesday => TuesdayEvents,
            DayOfWeek.Wednesday => WednesdayEvents,
            DayOfWeek.Thursday => ThursdayEvents,
            DayOfWeek.Friday => FridayEvents,
            DayOfWeek.Saturday => SaturdayEvents,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek),
                $"The allowed range is between 0 and 6, or from {nameof(DayOfWeek.Sunday)} to {nameof(DayOfWeek.Saturday)}"),
        };

    public Schedule(string title) : base(title) => Events.CollectionChanged += Events_CollectionChanged;

    protected string GetEventsDay(DayOfWeek dayOfWeek)
        => dayOfWeek switch
        {
            DayOfWeek.Sunday => nameof(SundayEvents),
            DayOfWeek.Monday => nameof(MondayEvents),
            DayOfWeek.Tuesday => nameof(TuesdayEvents),
            DayOfWeek.Wednesday => nameof(WednesdayEvents),
            DayOfWeek.Thursday => nameof(ThursdayEvents),
            DayOfWeek.Friday => nameof(FridayEvents),
            DayOfWeek.Saturday => nameof(SaturdayEvents),
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek),
                $"The allowed range is between 0 and 6, or from {nameof(DayOfWeek.Sunday)} to {nameof(DayOfWeek.Saturday)}"),
        };

    private readonly Queue<DayOfWeek> DaysToInvalidate = new();
    protected void EventsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var p in e.NewItems!.Cast<ScheduledEvent>().SelectMany(x => x.ActiveDays).Distinct())
                    DaysToInvalidate.Enqueue(p);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var p in e.OldItems!.Cast<ScheduledEvent>().SelectMany(x => x.ActiveDays).Distinct())
                    DaysToInvalidate.Enqueue(p);
                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (var p in e.OldItems!.Cast<ScheduledEvent>().Concat(e.NewItems!.Cast<ScheduledEvent>()).SelectMany(x => x.ActiveDays).Distinct())
                    DaysToInvalidate.Enqueue(p);
                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (var p in Enum.GetValues<DayOfWeek>())
                    DaysToInvalidate.Enqueue(p);
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            default:
                break;
        }

        while (DaysToInvalidate.Count > 0)
        {
            var p = DaysToInvalidate.Dequeue();
            SetEvents(p, null);
            Notify(GetEventsDay(p));
        }
    }

    protected IEnumerable<ScheduledEvent> FetchEvents(DayOfWeek dayOfWeek, ref IEnumerable<ScheduledEvent>? plans)
        => plans = Events.Where(s => s.ActiveDays.Any(x => x == dayOfWeek));

    protected ref IEnumerable<ScheduledEvent>? GetEventsRef(DayOfWeek dayOfWeek)
    {
        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                return ref _sundayEvents;
            case DayOfWeek.Monday:
                return ref _mondayEvents;
            case DayOfWeek.Tuesday:
                return ref _tuesdayEvents;
            case DayOfWeek.Wednesday:
                return ref _wednesdayEvents;
            case DayOfWeek.Thursday:
                return ref _thursdayEvents;
            case DayOfWeek.Friday:
                return ref _fridayEvents;
            case DayOfWeek.Saturday:
                return ref _saturdayEvents;
            default:
                throw new ArgumentOutOfRangeException(nameof(dayOfWeek),
                    $"The allowed range is between 0 and 6, or from {nameof(DayOfWeek.Sunday)} to {nameof(DayOfWeek.Saturday)}");
        }
    }

    protected void ClearEvents()
    {
        foreach (var p in Enum.GetValues<DayOfWeek>())
            SetEvents(p, null);
    }

    protected void SetEvents(DayOfWeek dayOfWeek, IEnumerable<ScheduledEvent>? plans)
        => GetEventsRef(dayOfWeek) = plans;
}
