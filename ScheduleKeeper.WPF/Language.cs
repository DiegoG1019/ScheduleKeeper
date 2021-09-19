using ScheduleKeeper.WPF.Types.Collections;

namespace ScheduleKeeper.WPF;

public sealed class Lang
{
    public static Language Current => Settings<Language>.Current;
}

public sealed class Language : ScheduleKeeperConfig, ISettings
{
    private const string _st = $"{nameof(ScheduleKeeper)}.{nameof(WPF)}.{nameof(Language)}";
    public string SettingsType { get; } = _st;
    public ulong Version { get; }

    //--
    private Lang_DayOfWeek _days = new();

    public Lang_DayOfWeek Days
    {
        get => _days;
        set
        {
            if (SetAndVerifyChange(ref _days, value))
                Notify();
        }
    }
    public sealed class Lang_DayOfWeek : ScheduleKeeperConfig
    {
        private string sun = "Sunday";
        private string mon = "Monday";
        private string tue = "Tuesday";
        private string wed = "Wednesday";
        private string thu = "Thursday";
        private string fri = "Friday";
        private string sat = "Saturday";

        private void NotifyDay()
            => Notify(nameof(Day));

        public IReadOnlyDictionary<DayOfWeek, string> Day { get; init; }

        public string Sunday
        {
            get => sun;
            set
            {
                if (SetAndVerifyChange(ref sun, value))
                {
                    Notify();
                    NotifyDay();
                }
            }
        }

        public string Monday
        {
            get => mon;
            set
            {
                if (SetAndVerifyChange(ref mon, value))
                {
                    Notify();
                    NotifyDay();
                }
            }
        }

        public string Tuesday
        {
            get => tue;
            set
            {
                if (SetAndVerifyChange(ref tue, value))
                {
                    Notify();
                    NotifyDay();
                }
            }
        }

        public string Wednesday
        {
            get => wed;
            set
            {
                if (SetAndVerifyChange(ref wed, value))
                {
                    Notify();
                    NotifyDay();
                }
            }
        }

        public string Thursday
        {
            get => thu;
            set
            {
                if (SetAndVerifyChange(ref thu, value))
                {
                    Notify();
                    NotifyDay();
                }
            }
        }

        public string Friday
        {
            get => fri;
            set
            {
                if (SetAndVerifyChange(ref fri, value))
                {
                    Notify();
                    NotifyDay();
                }
            }
        }

        public string Saturday
        {
            get => sat;
            set
            {
                if (SetAndVerifyChange(ref sat, value))
                {
                    Notify();
                    NotifyDay();
                }
            }
        }

        public Lang_DayOfWeek()
        {
            Day = new FuncDictionary<DayOfWeek, string>()
            {
                { DayOfWeek.Sunday, () => sun },
                { DayOfWeek.Monday, () => mon },
                { DayOfWeek.Tuesday, () => tue },
                { DayOfWeek.Wednesday, () => wed },
                { DayOfWeek.Thursday, () => thu },
                { DayOfWeek.Friday, () => fri },
                { DayOfWeek.Saturday, () => sat }
            };
        }
    }
}
