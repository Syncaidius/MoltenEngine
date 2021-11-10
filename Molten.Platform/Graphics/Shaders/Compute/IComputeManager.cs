namespace Molten.Graphics
{
    public interface IComputeManager
    {
        void Dispatch(IComputeTask task, int x, int y, int z);

        /// <summary>Gets a compute task that was previously loaded via an asset manager.</summary>
        /// <param name="name">The name of the compute task. case-insensitive.</param>
        /// <returns></returns>
        IComputeTask this[string name] { get; }
    }
}
