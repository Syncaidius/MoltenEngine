using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal class ScalarArray<T> : ShaderConstantVariable where T : struct
    {
        Type _elementType;
        Array _value;
        int _byteSize;
        internal int ExpectedElements;

        public ScalarArray(ShaderConstantBuffer parent, uint expectedElements)
            : base(parent)
        {
            _elementType = typeof(T);
            _byteSize = Marshal.SizeOf(_elementType);

            SizeOf = expectedElements * _byteSize;
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

                        int valueBytes = _value.Length * _byteSize;

                        if (valueBytes != SizeOf)
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
