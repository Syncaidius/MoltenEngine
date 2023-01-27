namespace Molten.Graphics
{
    /// <summary>
    /// Represents a dummy shader value which is not linked to any shader constant buffers or resources.
    /// </summary>
    public class DummyShaderValue : IShaderValue
    {
        internal DummyShaderValue() { }

        public string Name { get; set; }

        public object Value { get; set; }
    }
}
