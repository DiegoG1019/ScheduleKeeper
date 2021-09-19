global using ScheduleKeeper.Base;
global using ScheduleKeeper.Base.Models;

static void Print(object o) => Console.WriteLine(o);

var schedule = new Schedule("TestSchedule")
{
    Events = new()
    {
        new ScheduledEvent("Administracion de Centros de Computacion")
        {
            Plans = new()
            {
                new TimeFramePlan(DayOfWeek.Monday, new TimeOnly(12 + 4, 0, 0, 0), new TimeOnly(12 + 5, 10, 0, 0)),
                new TimeFramePlan(DayOfWeek.Wednesday, new TimeOnly(12 + 4, 0, 0, 0), new TimeOnly(12 + 5, 10, 0, 0))
            }
        },
        new ScheduledEvent("Teoria General de Sistemas")
        {
            Plans = new()
            {
                new TimeFramePlan(DayOfWeek.Tuesday, new TimeOnly(12 + 4, 0, 0, 0), new TimeOnly(12 + 5, 45, 0, 0)),
            }
        },
        new ScheduledEvent("Tecnologia del Hardware")
        {
            Plans = new()
            {
                new TimeFramePlan(DayOfWeek.Tuesday, new TimeOnly(12 + 6, 20, 0, 0), new TimeOnly(12 + 7, 30, 0, 0)),
                new TimeFramePlan(DayOfWeek.Thursday, new TimeOnly(12 + 5, 10, 0, 0), new TimeOnly(12 + 6, 20, 0, 0)),
            }
        },
        new ScheduledEvent("Ingles III")
        {
            Plans = new()
            {
                new TimeFramePlan(DayOfWeek.Wednesday, new TimeOnly(12 + 5, 45, 0, 0), new TimeOnly(12 + 7, 30, 0, 0)),
            }
        },
        new ScheduledEvent("Sistemas Digitales")
        {
            Plans = new()
            {
                new TimeFramePlan(DayOfWeek.Thursday, new TimeOnly(12 + 6, 20, 0, 0), new TimeOnly(12 + 7, 30, 0, 0)),
                new TimeFramePlan(DayOfWeek.Friday, new TimeOnly(12 + 6, 20, 0, 0), new TimeOnly(12 + 7, 30, 0, 0)),
            }
        },
        new ScheduledEvent("Autoestima")
        {
            Plans = new()
            {
                new TimeFramePlan(DayOfWeek.Wednesday, new TimeOnly(12 + 4, 0, 0, 0), new TimeOnly(12 + 5, 45, 0, 0)),
            }
        }
    }
};

Print($"Title: {schedule.Title}");
Print($"Events: {schedule.EventCount}");
Print($"Plans: {schedule.PlanCount}");
Print($"Active Days: {string.Join(", ", schedule.ActiveDays)}");

{
    string str = string.Join("\n\n", Enum.GetValues<DayOfWeek>()
        .Select(x => string.Join("\n", schedule[x]
        .SelectMany(x => x.Plans).OrderBy(x => x.OnDay).Select(x => $"Day: {x.OnDay} | Start: {x.StartTime} | End: {x.EndTime} | Duration: {x.Duration}"))));

    Print($"Events of day: {str}\n\n\n");
}

Print($"{string.Join("\n\n", schedule.GetTimeFrames().Select(x => $"{x.StartTime}\n{x.EndTime}\n({x.Duration})"))}\n\n\n------\n");
Print($"{string.Join("\n\n", schedule.GetTimeFrames(TimeSpan.FromMinutes(30)).Select(x => $"{x.StartTime}\n{x.EndTime}\n({x.Duration})"))}");
;