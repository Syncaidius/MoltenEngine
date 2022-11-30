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

        bool OnScrollWheel(InputScrollWheel wheel);

        void OnHover(CameraInputTracker tracker);

        void OnEnter(CameraInputTracker tracker);

        void OnLeave(CameraInputTracker tracker);

        void OnPressed(CameraInputTracker tracker);

        void OnHeld(CameraInputTracker tracker);

        void OnDragged(CameraInputTracker tracker);

        void OnReleased(CameraInputTracker tracker, bool releasedOutside);

        bool Contains(T pos);

        void Focus();

        void Unfocus();

        IPickable<T> ParentPickable { get; }

        bool IsFocused { get; set; }

        string Name { get; }
    }
}
