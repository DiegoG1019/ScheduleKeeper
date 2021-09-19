namespace ScheduleKeeper.WPF;

public sealed class Config : ScheduleKeeperConfig, ISettings
{
    private const string _st = $"{nameof(ScheduleKeeper)}.{nameof(WPF)}.{nameof(Config)}";
    public string SettingsType { get; } = _st;
    public ulong Version { get; }

    //----
    private ScheduledEventSettings_ _scheduledEventSettings = new();

    public ScheduledEventSettings_ ScheduledEventSettings
    {
        get => _scheduledEventSettings;
        set
        {
            if (SetAndVerifyChange(ref _scheduledEventSettings, value))
                Notify();
        }
    }

    public sealed class ScheduledEventSettings_ : ScheduleKeeperConfig
    {
        private bool tfp_showduration;

        public bool ShowTimeFrameDuration
        {
            get => tfp_showduration;
            set
            {
                if (SetAndVerifyChange(ref tfp_showduration, value))
                    Notify();
            }
        }
    }
}
