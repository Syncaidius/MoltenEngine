using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which can picked via a<see cref="CameraComponent"/>.
    /// </summary>
    public interface IPickable
    {
        IPickable2D Pick(Vector2F pos, Timing time);
    }
}
