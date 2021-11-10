namespace Molten.Input
{
    public struct Touch2PointGesture : ITouchGesture
    {
        public Vector2F Point1;

        public Vector2F Point2;

        public Vector2F Point3;

        /// <summary>
        /// The amount of change between <see cref="Point1"/> and <see cref="Point2"/> 
        /// positions since this gesture was last updated.
        /// </summary>
        public Vector2F DeltaP1P2;

        /// <summary>
        /// The amount of change between <see cref="Point1"/> and <see cref="Point3"/> 
        /// positions since this gesture was last updated.
        /// </summary>
        public Vector2F DeltaP1P3;

        /// <summary>
        /// The amount of change between <see cref="Point2"/> and <see cref="Point3"/> 
        /// positions since this gesture was last updated.
        /// </summary>
        public Vector2F DeltaP2P3;

        public TouchPointState State;

        public int ID { get; }
    }
}