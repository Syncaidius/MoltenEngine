using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class AttachmentVK : IEquatable<AttachmentVK>
    {
        AttachmentDescription _desc;

        /// <summary>
        /// Creates a new vulakn attachment description from the given 2D surface.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="surface"></param>
        public AttachmentVK(IRenderSurfaceVK surface) 
        {
            _desc = new AttachmentDescription()
            {
                Format = surface.ResourceFormat.ToApi(),
                Samples = GetSampleFlags(surface.MultiSampleLevel),
                LoadOp = surface.ClearColor.HasValue ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store,
                InitialLayout = ImageLayout.Undefined, // TODO Track current layout of texture/surface and use it here
                StencilLoadOp = AttachmentLoadOp.DontCare, // TODO if the surface is a depth/stencil surface, use the appropriate load/store ops
                StencilStoreOp = AttachmentStoreOp.DontCare,
                FinalLayout = GetFinalLayout(surface)
            };
        }

        /// <summary>
        /// Creates a new vulakn attachment description from the given 2D surface.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="surface"></param>
        public AttachmentVK(DepthSurfaceVK surface)
        {
            _desc = new AttachmentDescription()
            {
                Format = surface.ResourceFormat.ToApi(),
                Samples = GetSampleFlags(surface.MultiSampleLevel),
            };
        }

        public bool Equals(AttachmentVK other)
        {
            return _desc.FinalLayout == other._desc.FinalLayout &&
                _desc.Format == other._desc.Format &&
                _desc.InitialLayout == other._desc.InitialLayout &&
                _desc.LoadOp == other._desc.LoadOp &&
                _desc.Samples == other._desc.Samples &&
                _desc.StencilLoadOp == other._desc.StencilLoadOp &&
                _desc.StencilStoreOp == other._desc.StencilStoreOp &&
                _desc.StoreOp == other._desc.StoreOp;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AttachmentVK);
        }

        private ImageLayout GetFinalLayout(IRenderSurface surface)
        {
            // TODO support depth/stencil layouts
            if (surface is IDepthStencilSurface)
                throw new NotImplementedException();

            if(surface is ISwapChainSurface swapSurface)
            {
                return ImageLayout.PresentSrcKhr;
            }
            else
            {
                if(!surface.Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                    return ImageLayout.ColorAttachmentOptimal;
                else
                    return ImageLayout.ShaderReadOnlyOptimal;
            }
        }

        private SampleCountFlags GetSampleFlags(AntiAliasLevel aaLevel)
        {
            switch (aaLevel)
            {
                default:
                case AntiAliasLevel.None:
                    return SampleCountFlags.Count1Bit;

                case AntiAliasLevel.X2:
                    return SampleCountFlags.Count2Bit;

                case AntiAliasLevel.X4:
                    return SampleCountFlags.Count4Bit;

                case AntiAliasLevel.X8:
                    return SampleCountFlags.Count8Bit;

                case AntiAliasLevel.X16:
                    return SampleCountFlags.Count16Bit;
            }
        }

        internal ref AttachmentDescription Handle => ref _desc;
    }
}
