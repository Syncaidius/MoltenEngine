namespace Molten.Graphics
{
    /// <summary>
    /// Represents a dummy shader value which is not linked to any shader constant buffers or resources.
    /// </summary>
    public class DummyShaderValue : ShaderVariable
    {
        internal DummyShaderValue() { }

        public override object Value { get; set; }
    }
}
