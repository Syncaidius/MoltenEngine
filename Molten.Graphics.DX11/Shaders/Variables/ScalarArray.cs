using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Molten.IO;

namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal unsafe class ScalarArray<T> : ShaderConstantVariable where T : unmanaged
    {
        static Type _elementType = typeof(T);
        static int _stride = sizeof(T);

        Array _value;
        internal int ExpectedElements;

        public ScalarArray(ShaderConstantBuffer parent, uint expectedElements)
            : base(parent)
        {
            SizeOf = expectedElements * _stride;
        }

        public override void Dispose() { }

        internal override void Write(RawStream stream)
        {
            if (_value != null)
            {
                EngineUtil.PinObject(_value, (ptr) =>
                {
                    stream.Write(ptr.ToPointer(), SizeOf);
                });
            }
            else
            {
                stream.Seek(SizeOf, SeekOrigin.Current);
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

                        int valueBytes = _value.Length * _stride;

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
