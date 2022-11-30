using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which can picked via a<see cref="CameraComponent"/>.
    /// </summary>
    public interface IPickable
    {
        /// <summary>
        /// Performs 2D picking on the current <see cref="IPickable"/>. Returning null means no object was picked.
        /// </summary>
        /// <param name="pos">The 2D screen position to be picked.</param>
        /// <param name="time">A timing instance.</param>
        /// <returns></returns>
        IPickable<Vector2F> Pick2D(Vector2F pos, Timing time);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos">The 3D world position to be picked.</param>
        /// <param name="time">A timing instance.</param>
        /// <returns></returns>
        IPickable<Vector3F> Pick3D(Vector3F pos, Timing time);
    }

    /// <summary>
    /// Represents an object which can picked via a<see cref="CameraComponent"/>.
    /// </summary>
    public interface IPickable<T> : IPickable
        where T : IVector<float>
    {
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
