using System.Numerics;

namespace Molten
{
    /// <summary>
    /// Represents a vector of any size.
    /// </summary>
    /// <typeparam name="T">The number type of each vector component.</typeparam>
    public interface IVector<T>
        where T : INumber<T>
    {
    }
}
