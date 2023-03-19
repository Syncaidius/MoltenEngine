namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal unsafe class ScalarFloat3x3Variable : ShaderConstantVariable
    {
        Matrix3F _value;

        public ScalarFloat3x3Variable(ShaderConstantBuffer parent, string name) : 
            base(parent, name)
        {
            SizeOf = (uint)sizeof(Matrix3F);
        }

        public override unsafe void ValueFromPtr(void* ptr)
        {
            _value = *(Matrix3F*)ptr;
            _value.Transpose();
        }

        public override void Dispose() { }

        internal override void Write(byte* pDest)
        {
            ((Matrix3F*)pDest)[0] = _value;
        }

        public override object Value
        {
            get => _value;

            set
            {
                _value = (Matrix3F)value;
                _value.Transpose();
                DirtyParent();
            }
        }
    }
}
