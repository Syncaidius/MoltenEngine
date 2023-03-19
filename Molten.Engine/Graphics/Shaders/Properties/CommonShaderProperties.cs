namespace Molten.Graphics
{
    /// <summary>
    /// A helper class for storing references to common shader properties, or filling missing ones in with dummy properties.
    /// </summary>
    public abstract class CommonShaderProperties
    {
        protected CommonShaderProperties(HlslShader shader) { }

        protected ShaderVariable MapValue(HlslShader shader, string name)
        {
            return shader[name] ?? new DummyShaderValue();
        }
    }
}
