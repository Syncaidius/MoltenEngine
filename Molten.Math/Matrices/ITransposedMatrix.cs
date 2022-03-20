namespace Molten
{
    public interface ITransposedMatrix<T> where T : unmanaged
    {
        void Transpose(out T result);
    }
}
