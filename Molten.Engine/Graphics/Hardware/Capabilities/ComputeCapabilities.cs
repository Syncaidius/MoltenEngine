namespace Molten.Graphics;

public class ComputeCapabilities : ShaderStageCapabilities
{
    /// <summary>
    /// The max number of X threads per worker group.
    /// </summary>
    public uint MaxGroupSizeX { get; set; }

    /// <summary>
    /// The max number of Y threads per worker group.
    /// </summary>
    public uint MaxGroupSizeY { get; set; }

    /// <summary>
    /// The max number of Z threads per worker group.
    /// </summary>
    public uint MaxGroupSizeZ { get; set; }

    /// <summary>
    /// The max number of X worker groups possible in a dispatch command/call.
    /// </summary>
    public uint MaxGroupCountX { get; set; }

    /// <summary>
    /// The max number of Y worker groups possible in a dispatch command/call.
    /// </summary>
    public uint MaxGroupCountY { get; set; }

    /// <summary>
    /// The max number of Z worker groups possible in a dispatch command/call.
    /// </summary>
    public uint MaxGroupCountZ { get; set; }
}
