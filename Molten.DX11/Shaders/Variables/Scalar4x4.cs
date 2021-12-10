using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal class ScalarFloat4x4Variable : ShaderConstantVariable
    {
        Matrix4F _value;

        public ScalarFloat4x4Variable(ShaderConstantBuffer parent)
            : base(parent)
        {
            SizeOf = sizeof(float) * (4 * 4);
        }

        internal override void Write(ResourceStream stream)
        {
            stream.Write(_value);
        }

        public override object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = (Matrix4F)value;
                _value.Transpose();
                DirtyParent();
            }
        }
    }
}
