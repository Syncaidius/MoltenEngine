using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class ShaderSlotBinder<T> : ContextSlotBinder<ShaderComposition<T>>
        where T : unmanaged
    {
        ContextShaderStage<T> _stage;

        internal ShaderSlotBinder(ContextShaderStage<T> stage)
        {
            _stage = stage;
        }

        internal override void Bind(ContextSlot<ShaderComposition<T>> slot, ShaderComposition<T> value)
        {
            _stage.SetShader(value.PtrShader, null, 0);
        }

        internal override void Unbind(ContextSlot<ShaderComposition<T>> slot, ShaderComposition<T> value)
        {
            _stage.SetShader(null, null, 0);
        }
    }
}
