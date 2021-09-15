using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ScheduleKeeper.Base.Models;

public class ScheduledEvent : DescribedContextual
{
    public IEnumerable<TimeFramePlan> this[DayOfWeek dayOfWeek] => GetPlans(dayOfWeek);

    private ObservableCollection<TimeFramePlan> _plans = new();
    private IEnumerable<TimeFramePlan>? _sundayPlans;
    private IEnumerable<TimeFramePlan>? _mondayPlans;
    private IEnumerable<TimeFramePlan>? _tuesdayPlans;
    private IEnumerable<TimeFramePlan>? _wednesdayPlans;
    private IEnumerable<TimeFramePlan>? _thursdayPlans;
    private IEnumerable<TimeFramePlan>? _fridayPlans;
    private IEnumerable<TimeFramePlan>? _saturdayPlans;

    public ObservableCollection<TimeFramePlan> Plans
    {
        get => _plans;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), $"{nameof(Plans)} cannot be set to null.");
            if (ReferenceEquals(_plans, value))
                return;

            _plans.CollectionChanged -= Plans_CollectionChanged;
            _plans = value;
            _plans.CollectionChanged += Plans_CollectionChanged;
            ClearPlans();
            Notify();
            foreach (var p in Enum.GetValues<DayOfWeek>())
                Notify(GetPlansDay(p));
        }
    }

    public IEnumerable<TimeFramePlan> SundayPlans => _sundayPlans ?? FetchPlans(DayOfWeek.Sunday, ref _sundayPlans);
    public IEnumerable<TimeFramePlan> MondayPlans => _mondayPlans ?? FetchPlans(DayOfWeek.Monday, ref _mondayPlans);
    public IEnumerable<TimeFramePlan> TuesdayPlans => _tuesdayPlans ?? FetchPlans(DayOfWeek.Tuesday, ref _tuesdayPlans);
    public IEnumerable<TimeFramePlan> WednesdayPlans => _wednesdayPlans ?? FetchPlans(DayOfWeek.Wednesday, ref _wednesdayPlans);
    public IEnumerable<TimeFramePlan> ThursdayPlans => _thursdayPlans ?? FetchPlans(DayOfWeek.Thursday, ref _thursdayPlans);
    public IEnumerable<TimeFramePlan> FridayPlans => _fridayPlans ?? FetchPlans(DayOfWeek.Friday, ref _fridayPlans);
    public IEnumerable<TimeFramePlan> SaturdayPlans => _saturdayPlans ?? FetchPlans(DayOfWeek.Saturday, ref _saturdayPlans);

    public ScheduledEvent(string title)
    {
        _title = title;
        Plans.CollectionChanged += Plans_CollectionChanged;
    }

    public IEnumerable<TimeFramePlan> GetPlans(DayOfWeek dayOfWeek)
        => dayOfWeek switch
        {
            DayOfWeek.Sunday => SundayPlans,
            DayOfWeek.Monday => MondayPlans,
            DayOfWeek.Tuesday => TuesdayPlans,
            DayOfWeek.Wednesday => WednesdayPlans,
            DayOfWeek.Thursday => ThursdayPlans,
            DayOfWeek.Friday => FridayPlans,
            DayOfWeek.Saturday => SaturdayPlans,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek),
                $"The allowed range is between 0 and 6, or from {nameof(DayOfWeek.Sunday)} to {nameof(DayOfWeek.Saturday)}"),
        };

    protected string GetPlansDay(DayOfWeek dayOfWeek)
        => dayOfWeek switch
        {
            DayOfWeek.Sunday => nameof(SundayPlans),
            DayOfWeek.Monday => nameof(MondayPlans),
            DayOfWeek.Tuesday => nameof(TuesdayPlans),
            DayOfWeek.Wednesday => nameof(WednesdayPlans),
            DayOfWeek.Thursday => nameof(ThursdayPlans),
            DayOfWeek.Friday => nameof(FridayPlans),
            DayOfWeek.Saturday => nameof(SaturdayPlans),
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek),
                $"The allowed range is between 0 and 6, or from {nameof(DayOfWeek.Sunday)} to {nameof(DayOfWeek.Saturday)}"),
        };

    private readonly Queue<DayOfWeek> DaysToInvalidate = new();
    protected void Plans_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var p in e.NewItems!.Cast<TimeFramePlan>().Select(x => x.OnDay).Distinct())
                    DaysToInvalidate.Enqueue(p);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var p in e.OldItems!.Cast<TimeFramePlan>().Select(x => x.OnDay).Distinct())
                    DaysToInvalidate.Enqueue(p);
                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (var p in e.OldItems!.Cast<TimeFramePlan>().Concat(e.NewItems!.Cast<TimeFramePlan>()).Select(x => x.OnDay).Distinct())
                    DaysToInvalidate.Enqueue(p);
                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (var p in Enum.GetValues<DayOfWeek>())
                    DaysToInvalidate.Enqueue(p);
                break;
        }

        while (DaysToInvalidate.Count > 0)
        {
            var p = DaysToInvalidate.Dequeue();
            SetPlans(p, null);
            Notify(GetPlansDay(p));
        }
    }

    protected IEnumerable<TimeFramePlan> FetchPlans(DayOfWeek dayOfWeek, ref IEnumerable<TimeFramePlan>? plans)
        => plans = Plans.Where(s => s.OnDay == dayOfWeek).ToArray();

    protected ref IEnumerable<TimeFramePlan>? GetPlansRef(DayOfWeek dayOfWeek)
    {
        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                return ref _sundayPlans;
            case DayOfWeek.Monday:
                return ref _mondayPlans;
            case DayOfWeek.Tuesday:
                return ref _tuesdayPlans;
            case DayOfWeek.Wednesday:
                return ref _wednesdayPlans;
            case DayOfWeek.Thursday:
                return ref _thursdayPlans;
            case DayOfWeek.Friday:
                return ref _fridayPlans;
            case DayOfWeek.Saturday:
                return ref _saturdayPlans;
            default:
                throw new ArgumentOutOfRangeException(nameof(dayOfWeek),
                    $"The allowed range is between 0 and 6, or from {nameof(DayOfWeek.Sunday)} to {nameof(DayOfWeek.Saturday)}");
        }
    }

    protected void ClearPlans()
    {
        foreach (var p in Enum.GetValues<DayOfWeek>())
            SetPlans(p, null);
    }

    protected void SetPlans(DayOfWeek dayOfWeek, IEnumerable<TimeFramePlan>? plans)
        => GetPlansRef(dayOfWeek) = plans;
}
