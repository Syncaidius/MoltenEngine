namespace Molten
{
    /// <summary>
    /// Represents an implementation of a transposable matrix.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITransposableMatrix<T> where T : unmanaged
    {
        /// <summary>
        /// Transposes the current matrix and outputs it to <paramref name="result"/>.
        /// </summary>
        /// <param name="result"></param>
        void Transpose(out T result);

        /// <summary>
        /// Transposes the current matrix in-place.
        /// </summary>
        void Transpose();

        /// <summary>
        /// Calculates the transpose of the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix whose transpose is to be calculated.</param>
        /// <param name="result">When the method completes, contains the transpose of the specified matrix.</param>
        static abstract void Transpose(ref T matrix, out T result);

        /// <summary>
        /// Calculates the transpose of the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix whose transpose is to be calculated.</param>
        /// <returns>The transpose of the specified matrix.</returns>
        static abstract T Transpose(T matrix);
    }
}
