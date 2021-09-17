namespace ScheduleKeeper.Base.Models;

public class TimeFramePlan : TimeFrame
{
    private DayOfWeek _dayOfWeek;

    public DayOfWeek OnDay
    {
        get => _dayOfWeek;
        set
        {
            if (_dayOfWeek == value)
                return;
            _dayOfWeek = value;
            Notify();
        }
    }

    public TimeFramePlan(DayOfWeek day, TimeOnly start, TimeOnly end) : base(start, end) => _dayOfWeek = day;
}
