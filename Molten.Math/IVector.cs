using System.Numerics;

namespace Molten
{
    /// <summary>
    /// Represents a vector of any size.
    /// </summary>
    /// <typeparam name="T">The number type of each vector component.</typeparam>
    public interface IVector<T> : IFormattable
        where T : INumber<T>
    {
        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        void Clamp(T min, T max);

        /// <summary>
        /// Calculates the squared length of the vector.
        /// </summary>
        /// <returns>The squared length of the vector.</returns>
        T LengthSquared();

        /// <summary>
        /// Creates an array containing the elements of the current <see cref="IVector{T}"/>.
        /// </summary>
        /// <returns>An array containing the components of the vector.</returns>
        T[] ToArray();

        /// <summary>
        /// Gets whether or not the vector is zero length. That is, all components are 0.
        /// </summary>
        bool IsZero { get; }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of a component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on. This must be between 0 and 2</param>
        /// <returns>The value of the component at the specified index.</returns>
        T this[int index] { get; set; }
    }
}
