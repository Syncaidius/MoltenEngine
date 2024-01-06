namespace Molten;

/// <summary>
/// Represents an implementation of a uniform matrix, where all dimensions are the same length.
/// </summary>
/// <typeparam name="T">The underlying component type.</typeparam>
public interface IUniformMatrix<T> : IMatrix<T>
    where T : unmanaged
{
    void Transpose();
}
