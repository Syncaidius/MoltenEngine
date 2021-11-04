using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal class ScalarFloat4x4ArrayVariable : ShaderConstantVariable
    {
        Type _elementType = typeof(Matrix4F);
        Matrix4F[] _value;
        int _byteSize;
        int _expectedElements;
        bool _isDirty;

        public ScalarFloat4x4ArrayVariable(ShaderConstantBuffer parent, int expectedElements)
            : base(parent)
        {
            _byteSize = Matrix4F.SizeInBytes;
            _expectedElements = expectedElements;
            _value = new Matrix4F[_expectedElements];
            SizeOf = _expectedElements * _byteSize;

            for (int i = 0; i < _value.Length; i++)
                _value[i] = Matrix4F.Identity;

            _isDirty = true;
        }

        internal override void Write(SharpDX.DataStream stream)
        {
            if (_value != null)
            {
                if (_isDirty)
                {
                    for (int i = 0; i < _value.Length; i++)
                        _value[i].Transpose();

                    _isDirty = false;
                }

                EngineInterop.PinObject(_value, (ptr) =>
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
                        Matrix4F[] val = (Matrix4F[])value;

                        if (_value.Length != val.Length)
                            throw new InvalidOperationException("Value that was set is not of the expected size (" + _value.Length + " elements).");

                        // Transpose matrix values in the matrix.
                        for (int i = 0; i < val.Length; i++)
                            _value[i] = val[i];

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
