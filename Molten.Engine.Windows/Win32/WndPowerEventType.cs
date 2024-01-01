namespace Molten.Windows32
{
    // Generate enum which encapsulates win32 power management events.
    public enum WndPowerEventType
    {
        PBT_APMQUERYSUSPEND = 0x0000,

        PBT_APMPOWERSTATUSCHANGE = 0x000A,

        PBT_APMRESUMEAUTOMATIC = 0x0012,

        PBT_APMRESUMECRITICAL = 0x0006,

        PBT_APMRESUMESUSPEND = 0x0007,

        PBT_APMSUSPEND = 0x0004,

        PBT_POWERSETTINGCHANGE = 0x8013,
    }
}
