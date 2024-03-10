namespace Molten.Graphics;

/// <summary>
/// Represents the type or category of a <see cref="GpuDevice"/>.
/// </summary>
public enum GpuDeviceType
{
    /// <summary>
    /// The device does not match any other available types.
    /// </summary>
    Other = 0,

    /// <summary>
    /// The device is typically one embedded in or tightly coupled with the host.
    /// </summary>
    IntegratedGpu = 1,

    /// <summary>
    /// The device is typically a separate processor connected to the host via an interlink.
    /// </summary>
    DiscreteGpu = 2,

    /// <summary>
    /// The device is typically a virtual node in a virtualization environment.
    /// </summary>
    VirtualGpu = 3,

    /// <summary>
    /// The device is typically running on the same processor(s) as the host.
    /// </summary>
    Cpu = 4,
}
