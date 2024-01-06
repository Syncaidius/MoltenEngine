using Molten.Input;

namespace Molten;

/// <summary>
/// Represents an <see cref="SceneComponent"/> which implements custom input handling for a <typeparamref name="T"/>.
/// </summary>
public interface IInputReceiver<T>
    where T : InputDevice
{
    /// <summary>
    /// Invoked when the component is added to a scene, or the <typeparamref name="T"/> is (re)connected.
    /// </summary>
    /// <param name="device">The <typeparamref name="T"/> to be initialized on the current <see cref="IInputReceiver{T}"/>.</param>
    /// <param name="time">A timing instance.</param>
    void InitializeInput(T device, Timing time);

    /// <summary>
    /// Invoked when the component is removed from the scene, or the <typeparamref name="T"/> is disconnected.
    /// </summary>
    /// <param name="device">The <typeparamref name="T"/> to be de-initialized from the current <see cref="IInputReceiver{T}"/>.</param>
    /// <param name="time">A timing instance.</param>
    void DeinitializeInput(T device, Timing time);

    /// <summary>
    /// Updates <typeparamref name="T"/> input on the current object.
    /// </summary>
    /// <param name="device">The <typeparamref name="T"/> that is providing input updates.</param>
    /// <param name="time">A timing instance.</param>
    void HandleInput(T device, Timing time);

    /// <summary>
    /// Checks whether the device has had its first input update for a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="device">The <typeparamref name="T"/> to check.</param>
    bool IsFirstInput(T device);
}
