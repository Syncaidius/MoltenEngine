namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal unsafe class ScalarArray<T> : ShaderConstantVariable where T : unmanaged
    {
        static Type _elementType = typeof(T);
        static uint _stride = (uint)sizeof(T);

        Array _value;
        internal int ExpectedElements;

        public ScalarArray(ShaderConstantBuffer parent, uint expectedElements)
            : base(parent)
        {
            SizeOf = expectedElements * _stride;
        }

        public override void Dispose() { }

        internal override void Write(byte* pDest)
        {
            if (_value != null)
                EngineUtil.PinObject(_value, (ptr) => Buffer.MemoryCopy(ptr.ToPointer(), pDest, SizeOf, SizeOf));
            else
                EngineUtil.Zero(pDest, SizeOf);
        }

        public override object Value
        {
            get => _value;

            set
            {
                Type vType = value.GetType();

                if (vType.IsArray)
                {
                    Type eType = vType.GetElementType();

                    if (eType == _elementType)
                    {
                        _value = (Array)value;

                        nuint valueBytes = (nuint)_value.Length * _stride;

                        if (valueBytes != SizeOf)
                            throw new InvalidOperationException($"Value that was set is not of the expected size ({SizeOf}bytes)");
                    }
                    DirtyParent();
                }
                else
                {
                    throw new InvalidOperationException("Cannot set a non-array object to a HLSL array constant.");
                }
            }
        }
    }
}
