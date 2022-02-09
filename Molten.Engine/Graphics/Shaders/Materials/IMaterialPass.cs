namespace Molten.Graphics
{
    public interface IMaterialPass
    {
        /// <summary>
        /// Gets the name of the pass.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the number of times the current pass is rendered.
        /// </summary>
        int Iterations { get; set; }

        /// <summary>
        /// Gets the parent <see cref="IMaterial"/> instance.
        /// </summary>
        IMaterial Material { get; }
    }
}
