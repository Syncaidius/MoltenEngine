using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which can picked via a<see cref="CameraComponent"/>.
    /// </summary>
    public interface IPickable2D : IPickable
    {
        bool OnScrollWheel(InputScrollWheel wheel);

        void OnHover(CameraInputTracker tracker);

        void OnEnter(CameraInputTracker tracker);

        void OnLeave(CameraInputTracker tracker);

        void OnPressed(CameraInputTracker tracker);

        void OnHeld(CameraInputTracker tracker);

        void OnDragged(CameraInputTracker tracker);

        void OnReleased(CameraInputTracker tracker, bool releasedOutside);

        bool Contains(Vector2F pos);

        void Focus();

        void Unfocus();

        IPickable2D ParentPickable { get; }

        bool IsFocused { get; set; }

        string Name { get; }
    }
}
