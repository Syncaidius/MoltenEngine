namespace Molten.Graphics
{
    public abstract class DxcFoundation : EngineObject, IShaderElement
    {
        /// <summary>
        /// Gets or sets the number of iterations the shader/component should be run.
        /// </summary>
        public int Iterations { get; set; } = 1;
    }
}
