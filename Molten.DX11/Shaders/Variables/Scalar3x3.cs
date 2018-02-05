using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal class ScalarFloat3x3Variable : ShaderConstantVariable
    {
        Matrix3x3 _value;

        public ScalarFloat3x3Variable(ShaderConstantBuffer parent)
            : base(parent)
        {
            SizeOf = sizeof(float) * (4 * 4);
        }

        internal override void Write(SharpDX.DataStream stream)
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
                _value = (Matrix3x3)value;
                _value.Transpose();
                DirtyParent();
            }
        }
    }
}
