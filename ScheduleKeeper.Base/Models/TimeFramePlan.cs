namespace ScheduleKeeper.Base.Models;

public class TimeFramePlan : Contextual
{
    private TimeOnly _startTime;
    private TimeOnly _endTime;
    private DayOfWeek _dayOfWeek;

    public virtual TimeOnly StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime == value)
                return;
            _startTime = value; 
            Notify();
        }
    }

    public virtual TimeOnly EndTime 
    { 
        get => _endTime; 
        set
        {
            if (_endTime == value)
                return;
            _endTime = value; 
            Notify(); 
        } 
    }

    public virtual DayOfWeek OnDay
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
}
