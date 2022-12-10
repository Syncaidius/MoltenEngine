using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which can picked via a<see cref="CameraComponent"/>.
    /// </summary>
    public interface IPickable<T>
        where T : IVector<float>
    {        
        /// <summary>
        /// Performs picking on the current <see cref="IPickable"/>. Returning null means no object was picked.
        /// </summary>
        /// <param name="pos">The position to be picked.</param>
        /// <param name="time">A timing instance.</param>
        /// <returns></returns>
        IPickable<T> Pick(T pos, Timing time);

        /// <summary>
        /// Invoked when a mouse wheel was scrolled over the current <see cref="IPickable{T}"/>.
        /// </summary>
        /// <param name="wheel">The <see cref="InputScrollWheel"/> which triggered the invocation.</param>
        /// <returns></returns>
        bool OnScrollWheel(InputScrollWheel wheel);

        /// <summary>
        /// Invoked when a mouse (or other pointer) hovered over the current <see cref="IPickable{T}"/>.
        /// </summary>
        /// <param name="tracker">THe <see cref="CameraInputTracker"/> which triggered the invocation.</param>
        void OnHover(CameraInputTracker tracker);

        /// <summary>
        /// Invoked when a mouse (or other pointer) enters the bounds of the current <see cref="IPickable{T}"/>.
        /// </summary>
        /// <param name="tracker">THe <see cref="CameraInputTracker"/> which triggered the invocation.</param>
        void OnEnter(CameraInputTracker tracker);

        /// <summary>
        /// Invoked when a mouse (or other pointer) leaves the bounds of the current <see cref="IPickable{T}"/>.
        /// </summary>
        /// <param name="tracker">THe <see cref="CameraInputTracker"/> which triggered the invocation.</param>
        void OnLeave(CameraInputTracker tracker);

        /// <summary>
        /// Invoked when current <see cref="IPickable{T}"/> is pressed.
        /// </summary>
        /// <param name="tracker">THe <see cref="CameraInputTracker"/> which triggered the invocation.</param>
        void OnPressed(CameraInputTracker tracker);

        /// <summary>
        /// Invoked when current <see cref="IPickable{T}"/> is double-pressed/clicked.
        /// </summary>
        /// <param name="tracker">THe <see cref="CameraInputTracker"/> which triggered the invocation.</param>
        void OnDoublePressed(CameraInputTracker tracker);

        /// <summary>
        /// Invoked when current <see cref="IPickable{T}"/> is held.
        /// </summary>
        /// <param name="tracker">THe <see cref="CameraInputTracker"/> which triggered the invocation.</param>
        void OnHeld(CameraInputTracker tracker);

        /// <summary>
        /// Invoked when current <see cref="IPickable{T}"/> is dragged.
        /// </summary>
        /// <param name="tracker">THe <see cref="CameraInputTracker"/> which triggered the invocation.</param>
        void OnDragged(CameraInputTracker tracker);

        /// <summary>
        /// Invoked when a previous press action is released by a <see cref="CameraInputTracker"/>.
        /// </summary>
        /// <param name="tracker">THe <see cref="CameraInputTracker"/> which triggered the invocation.</param>
        void OnReleased(CameraInputTracker tracker, bool releasedOutside);

        /// <summary>
        /// Checks if the current <see cref="IPickable{T}"/> contains the provided position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns></returns>
        bool Contains(T pos);

        /// <summary>
        /// Focuses the current <see cref="IPickable{T}"/>.
        /// </summary>
        void Focus();

        /// <summary>
        /// Unfocuses the current <see cref="IPickable{T}"/>.
        /// </summary>
        void Unfocus();

        /// <summary>
        /// Gets the parent <see cref="IPickable{T}"/>, if any.
        /// </summary>
        IPickable<T> ParentPickable { get; }

        /// <summary>
        /// Gets or sets whether the current <see cref="IPickable{T}"/> is focused.
        /// </summary>
        bool IsFocused { get; set; }

        /// <summary>
        /// Gets the name of the current <see cref="IPickable{T}"/>.
        /// </summary>
        string Name { get; }
    }
}
