using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    internal class ScalarVariable<T> : ShaderConstantVariable where T : struct 
    {
        uint _expectedElements;
        uint _stride;

        uint _valueBytes;
        unsafe object _value;

        internal ScalarVariable(ShaderConstantBuffer parent, uint rows, uint columns) : base(parent)
        {
            _stride = Marshal.SizeOf<T>();
            _expectedElements = columns * rows;
            SizeOf = _stride * _expectedElements;

            T[] tempVal = new T[_expectedElements];
            _value = tempVal;
        }

        internal override void Write(RawStream stream)
        {
            // Pin array so a pointer can be retrieved safely.
            GCHandle handle = GCHandle.Alloc(_value, GCHandleType.Pinned);
            IntPtr ptr = (IntPtr)(handle.AddrOfPinnedObject().ToInt64());
            stream.Write(ptr, 0, SizeOf);

            // Free GC handle
            handle.Free();
        }

        public override object Value
        {
            get
            {                
                return _value;
            }

            set
            {
                Type t = value.GetType();
                uint valBytes = (uint)Marshal.SizeOf(t);

                if (valBytes != SizeOf)
                {
                    throw new InvalidOperationException("Value is not of the expected byte size.");
                }
                else
                {
                    _value = value;
                    _valueBytes = valBytes;
                    DirtyParent();
                }
            }
        }
    }
}
