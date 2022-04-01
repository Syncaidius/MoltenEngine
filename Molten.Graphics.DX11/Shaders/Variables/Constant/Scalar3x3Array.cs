namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal unsafe class ScalarFloat3x3ArrayVariable : ShaderConstantVariable
    {
        static Type _elementType = typeof(Matrix3F);
        static uint _stride = (uint)sizeof(Matrix3F);

        Matrix3F[] _value;
        uint _expectedElements;
        bool _isDirty = false;

        public ScalarFloat3x3ArrayVariable(ShaderConstantBuffer parent, uint expectedElements)
            : base(parent)
        {
            _expectedElements = expectedElements;
            SizeOf = _expectedElements * _stride;

            for (int i = 0; i < _value.Length; i++)
                _value[i] = Matrix3F.Identity;

            _isDirty = true;
        }

        public override unsafe void ValueFromPtr(void* ptr) { }

        public override void Dispose() { }

        internal override unsafe void Write(byte* pDest)
        {
            if (_isDirty)
            {
                if (_value != null)
                {

                    for (int i = 0; i < _value.Length; i++)
                        _value[i].Transpose();

                    _isDirty = false;
                }

                fixed (Matrix3F* ptr = _value)
                    Buffer.MemoryCopy(ptr, pDest, SizeOf, SizeOf);
            }
            else
            {
                EngineUtil.Zero(pDest, SizeOf);
            }
        }

        public override object Value
        {
            get
            {
                return _value;
            }
            set
            {
                Type vType = value.GetType();

                if (vType.IsArray)
                {
                    Type eType = vType.GetElementType();

                    if (eType == _elementType)
                    {
                        Matrix3F[] val = (Matrix3F[])value;

                        if (_value.Length != val.Length)
                            throw new InvalidOperationException($"Value that was set is not of the expected size ({_value.Length} elements).");

                        Buffer.BlockCopy(val, 0, _value, 0, (int)SizeOf);
                        _isDirty = true;
                        DirtyParent();
                    }
                    else
                    {
                        throw new InvalidOperationException("Attempt to set incorrect matrix type to a Matrix4x4 (float4x4) array constant.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot set a non-array object to a HLSL array constant.");
                }
            }
        }
    }
}
