using NeoSmart.AsyncLock;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ScheduleKeeper.Base.Models;

public class ScheduledEvent : DescribedContextual, IObservable<ScheduledEventNotification>
{
    private readonly static object hashlock = new();
    private static int statichash;
    
    private readonly int hash;
    public override int GetHashCode() => hash;

    public ScheduledEvent(string title) : base(title)
    {
        lock (hashlock)
            hash = statichash++;
        SchedulerLock = new AsyncLock();
        ActiveDays = new(_activeDays);
        Plans.CollectionChanged += PlansCollectionChanged;
    }

    public IEnumerable<TimeFramePlan> this[DayOfWeek dayOfWeek] => GetPlans(dayOfWeek);

    #region fields

    private readonly AsyncLock SchedulerLock;

    private readonly RangeEnabledObservableCollection<DayOfWeek> _activeDays = new();

    private ObservableCollection<TimeFramePlan> _plans = new();
    private IEnumerable<TimeFramePlan>? _sundayPlans;
    private IEnumerable<TimeFramePlan>? _mondayPlans;
    private IEnumerable<TimeFramePlan>? _tuesdayPlans;
    private IEnumerable<TimeFramePlan>? _wednesdayPlans;
    private IEnumerable<TimeFramePlan>? _thursdayPlans;
    private IEnumerable<TimeFramePlan>? _fridayPlans;
    private IEnumerable<TimeFramePlan>? _saturdayPlans;
    private int? _planCount;

    #endregion

    #region properties
    private readonly object planlock = new();
    public ObservableCollection<TimeFramePlan> Plans
    {
        get
        {
            lock (planlock)
                return _plans;
        }
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), $"{nameof(Plans)} cannot be set to null.");
            lock (planlock)
            {
                if (ReferenceEquals(_plans, value))
                    return;

                _plans.CollectionChanged -= PlansCollectionChanged;
                _plans = value;
                _plans.CollectionChanged += PlansCollectionChanged;
            }
            _activeDays.AddRange(_plans.Select(x => x.OnDay).Distinct());
            ClearPlans();
            Notify();
        }
    }

    public ReadOnlyObservableCollection<DayOfWeek> ActiveDays { get; init; }

    public int TotalPlans => _planCount ??= Plans.Count;

    public IEnumerable<TimeFramePlan> SundayPlans => _sundayPlans ??= FetchPlans(DayOfWeek.Sunday);
    public IEnumerable<TimeFramePlan> MondayPlans => _mondayPlans ??= FetchPlans(DayOfWeek.Monday);
    public IEnumerable<TimeFramePlan> TuesdayPlans => _tuesdayPlans ??= FetchPlans(DayOfWeek.Tuesday);
    public IEnumerable<TimeFramePlan> WednesdayPlans => _wednesdayPlans ??= FetchPlans(DayOfWeek.Wednesday);
    public IEnumerable<TimeFramePlan> ThursdayPlans => _thursdayPlans ??= FetchPlans(DayOfWeek.Thursday);
    public IEnumerable<TimeFramePlan> FridayPlans => _fridayPlans ??= FetchPlans(DayOfWeek.Friday);
    public IEnumerable<TimeFramePlan> SaturdayPlans => _saturdayPlans ??= FetchPlans(DayOfWeek.Saturday);

    #endregion

    #region methods

    #region public

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

    #endregion

    #region protected

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

    protected IEnumerable<TimeFramePlan> FetchPlans(DayOfWeek dayOfWeek)
        => Plans.Where(s => s.OnDay == dayOfWeek).ToArray();

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
    {
        GetPlansRef(dayOfWeek) = plans;
        Notify(GetPlansDay(dayOfWeek));
    }

    #endregion

    #region event handlers

    private readonly Queue<DayOfWeek> DaysToInvalidate = new();
    protected void PlansCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IEnumerable<DayOfWeek> affected;
        IEnumerable<TimeFramePlan> frames;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                affected = (frames = e.NewItems!.Cast<TimeFramePlan>()).Select(x => x.OnDay).Distinct();
                foreach (var p in affected)
                    DaysToInvalidate.Enqueue(p);
                foreach (var f in frames)
                    RegisterEvent(f);
                _activeDays.AddRange(affected.Except(ActiveDays));
                break;
            case NotifyCollectionChangedAction.Remove:
                affected = (frames = e.OldItems!.Cast<TimeFramePlan>()).Select(x => x.OnDay).Distinct();
                foreach (var p in affected)
                    DaysToInvalidate.Enqueue(p);
                foreach (var f in frames)
                    UnregisterEvent(f);
                _activeDays.Remove(affected);
                break;
            case NotifyCollectionChangedAction.Replace:
                var tr = e.OldItems!.Cast<TimeFramePlan>();
                var ta = e.NewItems!.Cast<TimeFramePlan>();
                affected = tr.Concat(ta).Select(x => x.OnDay).Distinct();
                foreach (var p in affected)
                    DaysToInvalidate.Enqueue(p);
                foreach (var f in ta)
                    RegisterEvent(f);
                foreach (var f in tr)
                    UnregisterEvent(f);
                _activeDays.ClearAndRepopulate(affected);
                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (var p in Enum.GetValues<DayOfWeek>())
                    DaysToInvalidate.Enqueue(p);
                _activeDays.Clear();
                ClearEventsAndRepopulate();
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            default:
                break;
        }

        _planCount = null;

        while (DaysToInvalidate.Count > 0)
        {
            var p = DaysToInvalidate.Dequeue();
            SetPlans(p, null);
        }
    }

    #endregion

    #endregion

    #region IObservable

    private readonly AsyncLock alock = new();
    private readonly List<IObserver<ScheduledEventNotification>> Observers = new();

    public IDisposable Subscribe(IObserver<ScheduledEventNotification> observer)
    {
        if (!Observers.Contains(observer))
            using (alock.Lock())
                Observers.Add(observer);
        return new Unsubscriber(Observers, observer, alock);
    }

    public async Task<IAsyncDisposable> SubscribeAsync(IObserver<ScheduledEventNotification> observer)
    {
        if (!Observers.Contains(observer))
            using (await alock.LockAsync())
                Observers.Add(observer);
        return new Unsubscriber(Observers, observer, alock);
    }

    protected async Task NotifyEventToObservers(ScheduledEventNotification eventNotification)
    {
        using (await alock.LockAsync())
            foreach (var o in Observers)
                try
                {
                    o.OnNext(eventNotification);
                }
                catch
                {
                    continue;
                }
    }

    #region Unsubscriber
    private class Unsubscriber : IDisposable, IAsyncDisposable
    {
        private readonly List<IObserver<ScheduledEventNotification>> Observers;
        private readonly IObserver<ScheduledEventNotification> Observer;
        private readonly AsyncLock AsyncLock;

        public Unsubscriber(List<IObserver<ScheduledEventNotification>> observers, IObserver<ScheduledEventNotification> observer, AsyncLock asyncLock)
        {
            Observers = observers;
            Observer = observer;
            AsyncLock = asyncLock;
        }

        public void Dispose()
        {
            using (AsyncLock.Lock())
                if (Observer != null && Observers.Contains(Observer))
                    Observers.Remove(Observer);
        }

        public async ValueTask DisposeAsync()
        {
            using (await AsyncLock.LockAsync())
                if (Observer != null && Observers.Contains(Observer))
                    Observers.Remove(Observer);
        }
    }

    #endregion

    #endregion

    #region Scheduler

    private class SchedulerJob
    {
        private static int _hash = 0;
        private static readonly object sync = new();

        private int hash;
        private readonly ScheduledEventNotification notif;
        private readonly Timer timer;

        public readonly ScheduledEvent Parent;
        public readonly TimeFramePlan Plan;

        public SchedulerJob(ScheduledEvent sched, TimeFramePlan plan)
        {
            lock (sync)
                hash = _hash++;
            notif = new(sched, plan);
            Parent = sched;
            Plan = plan;
            timer = new(Callback, null, plan.StartTime - TimeOnly.FromDateTime(DateTime.Now), TimeSpan.FromHours(24));
        }

        public SchedulerJob Stop()
        {
            timer.Dispose();
            return this;
        }

        ~SchedulerJob()
            => Stop();

        public override int GetHashCode() => hash;
        private async void Callback(object? state) => await Parent.NotifyEventToObservers(notif);
    }

    private readonly List<SchedulerJob> Jobs = new();
    private async void RegisterEvent(TimeFramePlan plan)
    {
        using (await alock.LockAsync())
            if (!Jobs.Any(x => ReferenceEquals(plan, x.Plan)))
                Jobs.Add(new SchedulerJob(this, plan));
    }

    private async void UnregisterEvent(TimeFramePlan plan)
    {
        SchedulerJob? job;
        using (await alock.LockAsync())
            if ((job = Jobs.FirstOrDefault(x => ReferenceEquals(plan, x.Plan))) is not null)
                Jobs.Remove(job.Stop());
    }

    private async void ClearEventsAndRepopulate()
    {
        using (await alock.LockAsync())
        {
            foreach (var job in Jobs)
                job.Stop();
            Jobs.Clear();
            foreach (var plan in Plans)
                Jobs.Add(new(this, plan));
        }
    }

    #endregion

    ~ScheduledEvent()
    {
        foreach (var o in Observers)
            o.OnCompleted();
    }
}
