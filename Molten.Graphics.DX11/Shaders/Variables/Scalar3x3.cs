using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal class ScalarFloat3x3Variable : ShaderConstantVariable
    {
        Matrix3F _value;

        public ScalarFloat3x3Variable(ShaderConstantBuffer parent)
            : base(parent)
        {
            SizeOf = sizeof(float) * (3 * 3);
        }

        internal override void Write(RawStream stream)
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
                _value = (Matrix3F)value;
                _value.Transpose();
                DirtyParent();
            }
        }
    }
}
