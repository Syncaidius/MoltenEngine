using System.Numerics;

namespace Molten
{    
    /// <summary>
    /// Represents a vector of any size.
    /// </summary>
    /// <typeparam name="N">The number type of each vector component.</typeparam>
    public interface IVector<N> : IFormattable
        where N : INumber<N>
    {
        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        void Clamp(N min, N max);

        /// <summary>
        /// Calculates the squared length of the vector.
        /// </summary>
        /// <returns>The squared length of the vector.</returns>
        N LengthSquared();

        /// <summary>
        /// Creates an array containing the elements of the current <see cref="IVector{V, N}"/>.
        /// </summary>
        /// <returns>An array containing the components of the vector.</returns>
        N[] ToArray();

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
        N this[int index] { get; set; }
    }

    /// <summary>
    /// Represents a vector of any size.
    /// </summary>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <typeparam name="N">The number type of each vector component.</typeparam>
    public interface IVector<V, N> : IVector<N>, IEquatable<V>
        where N : INumber<N>
        where V : IVector<V, N>
    {
        #region Methods
        static abstract V SmoothStep(V start, V end, N amount);

        static abstract N Dot(V left, V right);

        /// <summary>
        /// Returns a <see cref="IVector{V, N}"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="IVector{V, N}"/>.</param>
        /// <param name="right">The second source <see cref="IVector{V, N}"/>.</param>
        /// <param name="result">The output for the resultant <see cref="IVector{V, N}"/>.</param>
        /// <returns>A <see cref="IVector{V, N}"/> containing the smallest components of the source vectors.</returns>
        static abstract void Min(ref V left, ref V right, out V result);

        /// <summary>
        /// Returns a <see cref="IVector{V, N}"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="IVector{V, N}"/>.</param>
        /// <param name="right">The second source <see cref="IVector{V, N}"/>.</param>
        /// <returns>A <see cref="IVector{V, N}"/> containing the smallest components of the source vectors.</returns>
        static abstract V Min(ref V left, ref V right);

        /// <summary>
        /// Returns a <see cref="IVector{V, N}"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="IVector{V, N}"/>.</param>
        /// <param name="right">The second source <see cref="IVector{V, N}"/>.</param>
        /// <returns>A <see cref="IVector{V, N}"/> containing the smallest components of the source vectors.</returns>
        static abstract V Min(V left, V right);

        /// <summary>
        /// Returns a <see cref="IVector{V, N}"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="IVector{V, N}"/>.</param>
        /// <param name="right">The second source <see cref="IVector{V, N}"/>.</param>
        /// <param name="result">The output for the resultant <see cref="IVector{V, N}"/>.</param>
        /// <returns>A <see cref="IVector{V, N}"/> containing the largest components of the source vectors.</returns>
        static abstract void Max(ref V left, ref V right, out V result);

        /// <summary>
        /// Returns a <see cref="IVector{V, N}"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="IVector{V, N}"/>.</param>
        /// <param name="right">The second source <see cref="IVector{V, N}"/>.</param>
        /// <returns>A <see cref="IVector{V, N}"/> containing the largest components of the source vectors.</returns>
        static abstract V Max(ref V left, ref V right);

        /// <summary>
        /// Returns a <see cref="IVector{V, N}"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="IVector{V, N}"/>.</param>
        /// <param name="right">The second source <see cref="IVector{V, N}"/>.</param>
        /// <returns>A <see cref="IVector{V, N}"/> containing the largest components of the source vectors.</returns>
        static abstract V Max(V left, V right);

        /// <summary>
        /// Calculates the squared distance between two <see cref="IVector{V, N}"/> vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between the two vectors.</returns>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
        static abstract N DistanceSquared(ref V value1, ref V value2);

        /// <summary>
        /// Calculates the squared distance between two <see cref="IVector{V, N}"/> vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between the two vectors.</returns>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
        static abstract N DistanceSquared(V value1, V value2);

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="IVector{V, N}"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        static abstract V Clamp(V value, N min, N max);

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="IVector{V, N}"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="IVector{V, N}"/>.</param>
        static abstract void Clamp(ref V value, ref V min, ref V max, out V result);

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="IVector{V, N}"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        static abstract V Clamp(V value, V min, V max);

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal. 
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        static abstract V Reflect(V vector, V normal);

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal. 
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        static abstract V Reflect(ref V vector, ref V normal);
        #endregion

        #region Operators

        #endregion
    }
}
