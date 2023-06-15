using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class UniformBufferVK : BufferVK, IConstantBuffer
    {
        internal GraphicsConstantVariable[] Variables;
        internal bool DirtyVariables;
        internal Dictionary<string, GraphicsConstantVariable> _varLookup;
        internal int Hash;
        byte* _constData;

        internal UniformBufferVK(GraphicsDevice device, ConstantBufferInfo info) : 
            base(device, GraphicsBufferType.Constant, GraphicsResourceFlags.NoShaderAccess | GraphicsResourceFlags.CpuWrite, 1, info.Size)
        {
            _varLookup = new Dictionary<string, GraphicsConstantVariable>();
            _constData = (byte*)EngineUtil.Alloc(info.Size);
        }

        public string BufferName { get; }
        public bool IsDirty { get; set; }
    }
}
