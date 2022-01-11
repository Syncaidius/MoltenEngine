using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    internal class ScalarMatrixArray<T> : ShaderConstantVariable where T : struct
    {
        Type _elementType;
        uint _components;
        uint _stride;

        uint _valueBytes;
        uint _expectedElements;
        Array _value;

        internal ScalarMatrixArray(ShaderConstantBuffer parent, uint rows, uint columns, uint expectedElements) : base(parent)
        {
            _elementType = typeof(T);
            _stride = (uint)Marshal.SizeOf(_elementType);
            _components = columns * rows;
            _expectedElements = expectedElements;
            SizeOf = (_stride * _components) * _expectedElements;

            T[] tempVal = new T[_components];
            _value = tempVal;
        }

        internal override void Write(ResourceStream stream)
        {
            if (_value != null)
            {
                EngineUtil.PinObject(_value, (ptr) =>
                {
                    stream.Write(ptr, 0, SizeOf);
                });
            }
            else
            {
                stream.Seek(SizeOf, System.IO.SeekOrigin.Current);
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
