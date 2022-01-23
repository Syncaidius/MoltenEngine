using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Molten.IO;

namespace Molten.Graphics
{
    internal unsafe class ScalarVariable<T> : ShaderConstantVariable where T : unmanaged 
    {
        static uint _stride = (uint)sizeof(T);
        uint _expectedElements;
        T* _value;

        internal ScalarVariable(ShaderConstantBuffer parent, uint rows, uint columns) : base(parent)
        {
            _expectedElements = columns * rows;
            SizeOf = _stride * _expectedElements;
            _value = EngineUtil.AllocArray<T>(_expectedElements);
        }

        internal override void Write(RawStream stream)
        {
            stream.Write(_value, SizeOf);
        }

        public override void Dispose()
        {
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
