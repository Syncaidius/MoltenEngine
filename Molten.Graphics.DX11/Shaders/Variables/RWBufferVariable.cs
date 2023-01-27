namespace Molten.Graphics
{
    internal class RWBufferVariable : RWVariable
    {
        GraphicsBuffer _buffer;

        internal RWBufferVariable(HlslShader shader) : base(shader) { }

        protected override IShaderResource OnSetUnorderedResource(object value)
        {
            _buffer = value as GraphicsBuffer;
            if (_buffer != null && _buffer.IsUnorderedAccess == false)
                throw new InvalidOperationException("A structured buffer with unordered access must be set to '" + nameof(RWBufferVariable) + "'");

            return _buffer;
        }
    }
}
