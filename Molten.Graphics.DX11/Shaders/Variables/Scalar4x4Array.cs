using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Molten.IO;

namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal unsafe class ScalarFloat4x4ArrayVariable : ShaderConstantVariable
    {
        static uint _stride = (uint)Matrix4F.SizeInBytes;
        static Type _elementType = typeof(Matrix4F);

        Matrix4F[] _value;
        uint _expectedElements;
        bool _isDirty;

        public ScalarFloat4x4ArrayVariable(ShaderConstantBuffer parent, uint expectedElements)
            : base(parent)
        {
            _expectedElements = expectedElements;
            _value = new Matrix4F[_expectedElements];
            SizeOf = _expectedElements * _stride;

            for (int i = 0; i < _value.Length; i++)
                _value[i] = Matrix4F.Identity;

            _isDirty = true;
        }

        public override void Dispose() { }

        internal override void Write(RawStream stream)
        {
            if (_value != null)
            {
                if (_isDirty)
                {
                    for (int i = 0; i < _value.Length; i++)
                        _value[i].Transpose();

                    _isDirty = false;
                }

                fixed (Matrix4F* ptrValue = _value)
                    stream.Write(ptrValue, SizeOf);
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
                        Matrix4F[] val = (Matrix4F[])value;

                        if (_value.Length != val.Length)
                            throw new InvalidOperationException($"Value that was set is not of the expected size ({_value.Length} elements).");

                        Buffer.BlockCopy(val, 0, _value, 0, val.Length);
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
