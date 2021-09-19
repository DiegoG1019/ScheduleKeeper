using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleKeeper.Base.Models;

public record ScheduledEventNotification(string Title, string? Description, TimeOnly StartTime, TimeOnly EndTime)
{
    private TimeSpan? _dur;
    public TimeSpan Duration => _dur ??= EndTime - StartTime; 
    public IDictionary<string, string>? Notes { get; init; }

    public ScheduledEventNotification(ScheduledEvent @event, TimeFramePlan plan) : 
        this(@event.Title, @event.Description, plan.StartTime, plan.EndTime)
    {
        if (@event.Notes.Count > 0)
            Notes = GetNotes(@event.Notes);
    }

    public ScheduledEventNotification(string Title, string? Description, TimeOnly StartTime, TimeOnly EndTime, IEnumerable<MutableKeyValuePair<string, string>> notes) :
        this(Title, Description, StartTime, EndTime)
    {
        if (notes.Any())
            Notes = GetNotes(notes);
    }

    private static IDictionary<string, string> GetNotes(IEnumerable<MutableKeyValuePair<string, string>> notes)
    {
        var n = new Dictionary<string, string>();
        foreach (var (key, value) in notes)
            n.Add(key!, value!);
        return n;
    }
}
