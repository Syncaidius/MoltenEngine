namespace Molten;

public enum ReorderMode
{
    /// <summary>
    /// Pushes an item backward by one ID.
    /// </summary>
    PushBackward,

    /// <summary>
    /// Pushes an item forward by one ID.
    /// </summary>
    PushForward,

    /// <summary>
    /// Sends an item to the back of a list or collection.
    /// </summary>
    SendToBack,

    /// <summary>
    /// Sends an item to the front of a list or collection.
    /// </summary>
    BringToFront,
}
