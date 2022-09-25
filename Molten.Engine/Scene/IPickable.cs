using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which can picked via <see cref="CameraComponent.PickObject"/>
    /// </summary>
    public interface IPickable
    {
        /// <summary>
        /// Invoked when the pickable is being tested against a screen position.
        /// </summary>
        /// <param name="pDevice"></param>
        /// <returns></returns>
        bool Pick(PointingDevice pDevice, Timing time);

        /// <summary>
        /// Invoked when the pickable is being tested against a world-based position.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        //bool Pick(Vector3F point);
    }
}
