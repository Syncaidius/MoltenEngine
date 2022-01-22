using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    /// <summary>A shader matrix3x2 variable.</summary>
    internal class ScalarFloat3x2Variable : ShaderConstantVariable
    {
        Matrix3x2F _value;

        public ScalarFloat3x2Variable(ShaderConstantBuffer parent)
            : base(parent)
        {
            SizeOf = sizeof(float) * (3 * 2);
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
                _value = (Matrix3x2F)value;
                DirtyParent();
            }
        }
    }
}
