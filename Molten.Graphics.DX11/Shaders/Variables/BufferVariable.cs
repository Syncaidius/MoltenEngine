namespace Molten.Graphics
{
    internal class BufferVariable : ShaderResourceVariable
    {
        internal BufferVariable(HlslShader shader) : base(shader) { }

        protected override IShaderResource OnSetResource(object value)
        {
            return value as BufferDX11;
        }
    }
}
