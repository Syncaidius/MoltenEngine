namespace Molten
{
    /// <summary>
    /// Describes the result of an intersection with a plane in three dimensions.
    /// </summary>
    public enum PlaneIntersectionType
    {
        /// <summary>
        /// The object is behind the plane.
        /// </summary>
        Back,

        /// <summary>
        /// The object is in front of the plane.
        /// </summary>
        Front,

        /// <summary>
        /// The object is intersecting the plane.
        /// </summary>
        Intersecting
    }
}
