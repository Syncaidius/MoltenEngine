using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class AttachmentVK
    {
        AttachmentDescription _desc;

        /// <summary>
        /// Creates a new vulakn attachment description from the given 2D surface.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="surface"></param>
        public AttachmentVK(RenderSurface2DVK surface) 
        {
            _desc = new AttachmentDescription()
            {
                Format = surface.ResourceFormat.ToApi(),
                Samples = GetSampleFlags(surface.MultiSampleLevel),
            };
        }

        /// <summary>
        /// Creates a new vulakn attachment description from the given 2D surface.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="surface"></param>
        public AttachmentVK(RenderSurface1DVK surface)
        {
            _desc = new AttachmentDescription()
            {
                Format = surface.ResourceFormat.ToApi(),
                Samples = GetSampleFlags(surface.MultiSampleLevel),
            };
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
