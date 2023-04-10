using System.Runtime.InteropServices;

namespace Molten.Graphics.DX11
{
    internal unsafe class ScalarVariable<T> : ShaderConstantVariable where T : unmanaged 
    {
        static uint _stride = (uint)sizeof(T);
        uint _expectedElements;
        T* _value;

        internal ScalarVariable(ShaderConstantBuffer parent, uint rows, uint columns, string name) : 
            base(parent, name)
        {
            _expectedElements = columns * rows;
            SizeOf = _stride * _expectedElements;
            _value = EngineUtil.AllocArray<T>(_expectedElements);
        }

        public override unsafe void ValueFromPtr(void* ptr)
        {
            Buffer.MemoryCopy(ptr, _value, SizeOf, SizeOf);
        }

        internal override void Write(byte* pDest)
        {
            Buffer.MemoryCopy(_value, pDest, SizeOf, SizeOf);
        }

        ~ScalarVariable()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if(_value != null)
                EngineUtil.Free(ref _value);
        }

        public override object Value
        {
            get => *_value;

            set
            {
                Type t = value.GetType();
                if (!t.IsValueType)
                {
                    throw new InvalidOperationException("The value is not a Value-Type");
                }
                else
                {
                    uint valBytes = (uint)Marshal.SizeOf(t);
                    if(valBytes != SizeOf)
                        throw new InvalidOperationException("Value is not of the expected byte size.");
                }

                EngineUtil.PinObject(in value, (ptr) =>
                {
                    void * p = ptr.ToPointer();
                    Buffer.MemoryCopy(p, _value, SizeOf, SizeOf);
                });

                DirtyParent();
            }
        }
    }
}
