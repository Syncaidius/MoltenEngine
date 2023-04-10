namespace Molten.Graphics.DX11
{
    internal unsafe class ScalarMatrixArray<T> : ShaderConstantVariable where T : unmanaged
    {
        static Type _elementType = typeof(T);
        static uint _stride = (uint)sizeof(T);

        uint _components;
        uint _valueBytes;
        uint _expectedElements;
        Array _value;

        internal ScalarMatrixArray(ShaderConstantBuffer parent, uint rows, uint columns, uint expectedElements, string name) : 
            base(parent, name)
        {
            _components = columns * rows;
            _expectedElements = expectedElements;
            SizeOf = (_stride * _components) * _expectedElements;

            T[] tempVal = new T[_components];
            _value = tempVal;
        }

        public override unsafe void ValueFromPtr(void* ptr) { }

        public override void Dispose() { }

        internal override void Write(byte* pDest)
        {
            if (_value != null)
                EngineUtil.PinObject(_value, (ptr) => Buffer.MemoryCopy(ptr.ToPointer(), pDest, SizeOf, SizeOf));
            else
                EngineUtil.MemSet(pDest, 0, SizeOf);
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

                        _valueBytes = (uint)_value.Length * _stride;

                        if (_valueBytes != SizeOf)
                            throw new InvalidOperationException("Value that was set is not of the expected size (" + SizeOf + " bytes)");
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
