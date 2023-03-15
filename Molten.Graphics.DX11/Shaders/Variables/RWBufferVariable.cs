namespace Molten.Graphics
{
    internal class RWBufferVariable : RWVariable
    {
        internal RWBufferVariable(HlslShader shader) : base(shader) { }

        protected override IShaderResource OnSetUnorderedResource(object value)
        {
            BufferDX11 buffer = value as BufferDX11;
            if (buffer != null && buffer.IsUnorderedAccess == false)
                throw new InvalidOperationException("A structured buffer with unordered access must be set to '" + nameof(RWBufferVariable) + "'");

            return buffer;
        }
    }
}
