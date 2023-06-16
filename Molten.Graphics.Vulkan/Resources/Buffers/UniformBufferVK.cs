using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class UniformBufferVK : BufferVK, IConstantBuffer, IEquatable<UniformBufferVK>
    {
        internal GraphicsConstantVariable[] Variables;
        internal bool DirtyVariables;
        internal Dictionary<string, GraphicsConstantVariable> _varLookup;
        byte* _constData;

        internal UniformBufferVK(GraphicsDevice device, ConstantBufferInfo info) : 
            base(device, GraphicsBufferType.Constant, GraphicsResourceFlags.NoShaderAccess | GraphicsResourceFlags.CpuWrite, 1, info.Size)
        {
            _varLookup = new Dictionary<string, GraphicsConstantVariable>();
            _constData = (byte*)EngineUtil.Alloc(info.Size);
        }

        public bool Equals(UniformBufferVK other)
        {
            return GraphicsConstantVariable.AreEqual(Variables, other.Variables);
        }

        public string BufferName { get; }

        public bool IsDirty { get; set; }
    }
}
