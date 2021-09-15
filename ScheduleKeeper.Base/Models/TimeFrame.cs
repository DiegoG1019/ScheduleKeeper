﻿namespace ScheduleKeeper.Base.Models;

public class TimeFrame : Contextual
{
    private TimeOnly _startTime;
    private TimeOnly _endTime;

    public TimeOnly StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime == value)
                return;
            _startTime = value;
            Duration = StartTime - EndTime;
            Notify();
            Notify(nameof(Duration));
        }
    }

    public TimeOnly EndTime
    {
        get => _endTime;
        set
        {
            if (_endTime == value)
                return;
            _endTime = value;
            Duration = StartTime - EndTime;
            Notify();
            Notify(nameof(Duration));
        }
    }

    public TimeSpan Duration { get; private set; }
}