namespace Molten.Graphics;

public class SupportedCommandSet
{
    /// <summary>
    /// Gets or sets the number of meaningful bits in query timestamps written via the timestamp functionality of a graphics API. e.g. in Vulkan these would be 
    /// functions such as vkCmdWriteTimestamp.
    /// <para>A value of 0 indicates no support for timestamps.</para>
    /// </summary>
    public uint TimeStampBits { get; set; }

    /// <summary>
    /// Gets or sets the maximum number allowed sets of the current <see cref="SupportedCommandSet"/>.
    /// </summary>
    public uint MaxQueueCount { get; set; }

    /// <summary>
    /// Gets the command capabilities of the current <see cref="SupportedCommandSet"/>.
    /// </summary>
    public CommandSetCapabilityFlags CapabilityFlags { get; set; }
}

/// <summary>
/// Flags that represent the command set capabilities of a <see cref="SupportedCommandSet"/>. Shares value parity wit Vulkan's vkQueueFlagBits.
/// <para>See Vulkan: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkQueueFlagBits.html</para>
/// <para>See DirectX 12: https://learn.microsoft.com/en-us/windows/win32/api/d3d12/ne-d3d12-d3d12_command_list_type</para>
/// </summary>
[Flags]
public enum CommandSetCapabilityFlags
{
    None = 0,

    Graphics = 0x00000001,

    Compute = 0x00000002,

    TransferCopy = 0x00000004,

    MemoryManagement = 0x00000008,

    ProtectedBit = 0x00000010,

    VideoDecode = 0x00000020,

    VideoEncode = 0x00000040,

    /// <summary>
    /// Optical flow are fundamental algorithms in computer vision (CV) area, often allowing applications to estimate 2D displacement of pixels between two frames.
    /// </summary>
    OpticalFlow = 0x00000100,
}
