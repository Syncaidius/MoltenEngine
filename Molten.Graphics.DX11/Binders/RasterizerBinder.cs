using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class RasterizerBinder : ContextSlotBinder<GraphicsRasterizerState>
    {
        internal override void Bind(ContextSlot<GraphicsRasterizerState> slot, GraphicsRasterizerState value)
        {
            value = value ?? slot.Context.Device.RasterizerBank.GetPreset(RasterizerPreset.Default);
            slot.Context.Native->RSSetState(value);
        }

        internal override void Unbind(ContextSlot<GraphicsRasterizerState> slot, GraphicsRasterizerState value)
        {
            slot.Context.Native->RSSetState(null);
        }
    }
}
