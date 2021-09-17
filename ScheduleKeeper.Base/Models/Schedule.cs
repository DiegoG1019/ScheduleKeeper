using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
    private IEnumerable<DayOfWeek>? _activeDays;
    private int? eventCount;
    private int? planCount;

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
        }
    }

    public IEnumerable<ScheduledEvent> SundayEvents => _sundayEvents ??= FetchEvents(DayOfWeek.Sunday);
    public IEnumerable<ScheduledEvent> MondayEvents => _mondayEvents ??= FetchEvents(DayOfWeek.Monday);
    public IEnumerable<ScheduledEvent> TuesdayEvents => _tuesdayEvents ??= FetchEvents(DayOfWeek.Tuesday);
    public IEnumerable<ScheduledEvent> WednesdayEvents => _wednesdayEvents ??= FetchEvents(DayOfWeek.Wednesday);
    public IEnumerable<ScheduledEvent> ThursdayEvents => _thursdayEvents ??= FetchEvents(DayOfWeek.Thursday);
    public IEnumerable<ScheduledEvent> FridayEvents => _fridayEvents ??= FetchEvents(DayOfWeek.Friday);
    public IEnumerable<ScheduledEvent> SaturdayEvents => _saturdayEvents ??= FetchEvents(DayOfWeek.Saturday);

    public IEnumerable<DayOfWeek> ActiveDays => _activeDays ??= _events.SelectMany(x => x.ActiveDays).Distinct();

    public int EventCount => eventCount ??= Events.Count;
    public int PlanCount => planCount ??= Events.Sum(x => x.Plans.Count);

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

    public IEnumerable<TimeFrame> GetTimeFrames(TimeSpan? step = null)
        => step is null ? GetCompactTimeFrames() : GetSteppedTimeFrames((TimeSpan)step);

    protected virtual IEnumerable<TimeFrame> GetSteppedTimeFrames(TimeSpan step)
    {
        _ = _events.SelectMany(x => x.Plans).Aggregate(
        new
        {
            EarliestStart = TimeOnly.MinValue,
            LatestEnd = TimeOnly.MinValue
        },
        (accumulator, o) => new
        {
            EarliestStart = DGHelper.Min(o.StartTime, accumulator.EarliestStart),
            LatestEnd = DGHelper.Max(o.EndTime, accumulator.LatestEnd)
        }) is { EarliestStart: var estart, LatestEnd: var latend };

        var cstep = estart.ToTimeSpan() + step;
        var cstept = TimeOnly.FromTimeSpan(cstep);

        var x = new TimeFrame(estart, cstept); 
        yield return x;
        while (cstept < latend)
            yield return new TimeFrame(cstept, cstept = TimeOnly.FromTimeSpan(cstep += step));
    }

    protected virtual IEnumerable<TimeFrame> GetCompactTimeFrames()
    {
    }

    public Schedule(string title) : base(title) => Events.CollectionChanged += EventsCollectionChanged;

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
        IEnumerable<ScheduledEvent>? newitems = null;
        IEnumerable<ScheduledEvent>? olditems = null;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var p in (newitems = e.NewItems!.Cast<ScheduledEvent>()).SelectMany(x => x.ActiveDays).Distinct())
                    DaysToInvalidate.Enqueue(p);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var p in (olditems = e.OldItems!.Cast<ScheduledEvent>()).SelectMany(x => x.ActiveDays).Distinct())
                    DaysToInvalidate.Enqueue(p);
                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (var p in (olditems = e.OldItems!.Cast<ScheduledEvent>())
                                                         .Concat(newitems = e.NewItems!.Cast<ScheduledEvent>())
                                                         .SelectMany(x => x.ActiveDays)
                                                         .Distinct())
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

        if (olditems is not null)
            foreach (var ev in olditems)
                ev.PropertyChanged -= ScheduledEventChanged;

        if(newitems is not null)
            foreach (var ev in newitems)
                ev.PropertyChanged += ScheduledEventChanged;

        if(DaysToInvalidate.Count is >0)
        {
            _activeDays = null;
            Notify(nameof(ActiveDays));
        }

        while (DaysToInvalidate.Count > 0)
        {
            var p = DaysToInvalidate.Dequeue();
            SetEvents(p, null);
        }

        eventCount = null;
    }

    private void ScheduledEventChanged(object? sender, PropertyChangedEventArgs e)
    {
        var ev = (ScheduledEvent)sender!;
        switch (e.PropertyName)
        {
            case nameof(ScheduledEvent.SundayPlans):
                SetEvents(DayOfWeek.Sunday, null);
                break;
            case nameof(ScheduledEvent.MondayPlans):
                SetEvents(DayOfWeek.Monday, null);
                break;
            case nameof(ScheduledEvent.TuesdayPlans):
                SetEvents(DayOfWeek.Tuesday, null);
                break;
            case nameof(ScheduledEvent.WednesdayPlans):
                SetEvents(DayOfWeek.Wednesday, null);
                break;
            case nameof(ScheduledEvent.ThursdayPlans):
                SetEvents(DayOfWeek.Thursday, null);
                break;
            case nameof(ScheduledEvent.FridayPlans):
                SetEvents(DayOfWeek.Friday, null);
                break;
            case nameof(ScheduledEvent.SaturdayPlans):
                SetEvents(DayOfWeek.Saturday, null);
                break;
            case nameof(ScheduledEvent.ActiveDays):
                _activeDays = null;
                Notify(nameof(ActiveDays));
                break;
            case nameof(ScheduledEvent.TotalPlans):
                planCount = null;
                Notify(nameof(PlanCount));
                break;
        }
    }

    protected IEnumerable<ScheduledEvent> FetchEvents(DayOfWeek dayOfWeek)
        => Events.Where(s => s.ActiveDays.Any(x => x == dayOfWeek));

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
    {
        GetEventsRef(dayOfWeek) = plans;
        Notify(GetEventsDay(dayOfWeek));
    }
}
