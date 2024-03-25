namespace Molten.Graphics;

/// <summary>
/// Represents the priority of a graphics task or command.
/// </summary>
public enum GpuPriority
{
    /// <summary>
    /// The task or command will be executed at the start of the next device frame.
    /// </summary>
    StartOfFrame = 1,

    /// <summary>
    /// The task or command will be executed at the end of the current device frame.
    /// </summary>
    EndOfFrame = 2,
}
