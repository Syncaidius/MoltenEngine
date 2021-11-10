namespace Molten.Input
{
    public delegate void InputConnectionStatusHandler<T>(IInputDevice<T> device, bool isConnected) where T : struct;

    public delegate void InputConnectionHandler<T>(IInputDevice<T> device) where T : struct;

    public interface IInputDevice
    {
        /// <summary>Attempts to open the associated control pane application/software for the device. Does nothing if no control app is available.</summary>
        void OpenControlPanel();

        /// <summary>
        /// Clears the current state of the input handler.
        /// </summary>
        void ClearState();

        /// <summary>Gets whether or not the input device is connected.</summary>
        bool IsConnected { get; }

        /// <summary>Gets the name of the device.</summary>
        string DeviceName { get; }
    }

    public interface IInputDevice<T> : IInputDevice
        where T : struct
    {
        /// <summary>
        /// Invoked when the connection status of the device has changed.
        /// </summary>
        event InputConnectionStatusHandler<T> OnConnectionStatusChanged;

        /// <summary>
        /// Invoked when the device is connected.
        /// </summary>
        event InputConnectionHandler<T> OnConnected;

        /// <summary>
        /// Invoked when the device is disconnected.
        /// </summary>
        event InputConnectionHandler<T> OnDisconnected;

        /// <summary>Returns true if the specified button is pressed.</summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns>Returns true if the button is pressed.</returns>
        bool IsPressed(T value);

        /// <summary>Returns true if any of the provided keys/buttons are pressed.</summary>
        /// <param name="values">The buttons or keys to check.</param>
        /// <returns></returns>
        bool IsAnyPressed(params T[] values);

        /// <summary>Returns true if the specified button was tapped.</summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns></returns>
        bool IsTapped(T value);

        /// <summary>Returns true if the specified button was pressed in both the previous and current frame. </summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns></returns>
        bool IsHeld(T value);
    }
}
