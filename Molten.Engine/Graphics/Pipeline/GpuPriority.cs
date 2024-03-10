namespace Molten.Graphics;

/// <summary>
/// Represents the priority of a graphics task or command.
/// </summary>
public enum GpuPriority
{
    /// <summary>
    /// The task or command must be executed immediately.
    /// </summary>
    Immediate = 0,

    /// <summary>
    /// The task or command must be executed next time the object is applied to the pipeline.
    /// </summary>
    Apply = 1,

    /// <summary>
    /// The task or command will be executed at the start of the next frame, before render.
    /// </summary>
    StartOfFrame = 2,

    /// <summary>
    /// The task or command will be executed at the end of the current frame, after render.
    /// </summary>
    EndOfFrame = 3,
}
