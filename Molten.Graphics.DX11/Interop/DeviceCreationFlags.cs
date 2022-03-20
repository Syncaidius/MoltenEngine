namespace Molten.Graphics
{
    [Flags]
    internal enum DeviceCreationFlags
    {
        None = 0,
        SingleThreaded = 1,
        Debug = 1 << 1,
        SwitchToRef = 1 << 2,
        PreventInternalThreadingOptimizations = 1 << 3,
        BgraSupport = 1 << 4,
        Debuggable = 1 << 5,
        PreventAlteringLayerSettingsFromRegistery = 1 << 6,
        DisableGpuTimeout = 1 << 7,
        VideoSupport = 1 << 8,
    };
}
