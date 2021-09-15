namespace ScheduleKeeper.Base.Models;

public class MutableKeyValuePair<T1, T2> : Contextual
{
    private T1? _t1;
    private T2? _t2;

    public T1? Key
    {
        get => _t1;
        set
        {
            _t1 = value;
            Notify();
        }
    }

    public T2? Value
    {
        get => _t2;
        set
        {
            _t2 = value;
            Notify();
        }
    }
}