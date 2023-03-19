namespace Molten.Graphics
{
    /// <summary>A shader matrix3x2 variable.</summary>
    internal unsafe class ScalarFloat3x2Variable : ShaderConstantVariable
    {
        Matrix3x2F _value;

        public ScalarFloat3x2Variable(ShaderConstantBuffer parent, string name)
            : base(parent, name)
        {
            SizeOf = sizeof(float) * (3 * 2);
        }
        public override unsafe void ValueFromPtr(void* ptr)
        {
            _value = *(Matrix3x2F*)ptr;
        }

        public override void Dispose() { }

        internal override void Write(byte* pDest)
        {
            ((Matrix3x2F*)pDest)[0] = _value;
        }

        public override object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = (Matrix3x2F)value;
                DirtyParent();
            }
        }
    }
}
