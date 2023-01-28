namespace Molten.Input
{
    public abstract class TouchDevice : PointingDevice
    {
        /// <summary>
        /// The number of active touch points on the current <see cref="ITouchDevice"/>.
        /// </summary>
        public abstract int TouchPointCount { get; protected set; }

        public override PointingDeviceType PointerType => PointingDeviceType.Touchpanel;
    }
}
