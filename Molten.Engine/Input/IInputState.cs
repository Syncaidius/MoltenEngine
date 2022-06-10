namespace Molten.Input
{
    public interface IInputState
    {
        /// <summary>
        /// Gets the UTC time at which the input press was first detected.
        /// </summary>
        DateTime PressTimestamp { get; set; }

        /// <summary>
        /// Gets the update ID in which the current <see cref="IInputState"/> was processed. 
        /// This is based on the number of times the parent <see cref="InputService"/> has updated.
        /// </summary>
        ulong UpdateID { get; set; }

        /// <summary>
        /// Gets or sets the state set ID.
        /// </summary>
        int SetID { get; set; }

        /// <summary>
        /// Gets the current press state.
        /// </summary>
        InputAction Action { get; set; }
    }
}
