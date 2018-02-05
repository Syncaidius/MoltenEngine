using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents different options of a TextureAsset2D upon creation.</summary>
    [Flags]
    public enum TextureFlags
    {
        None = 0,

        /// <summary>Allows unordered access to the texture.</summary>
        AllowUAV = 2,

        /// <summary>Allows mip maps to be generated.</summary>
        AllowMipMapGeneration = 4,

        /// <summary>Do not create or expect a shader resource for the texture.</summary>
        NoShaderResource = 8,

        /// <summary>Allow the resource to be shared between devices, which including devices of different DX versions.</summary>
        SharedResource = 16,

        /// <summary>The texture is used purely for staging. Staging textures can only be read by the CPU and written to by the GPU.
        /// This is useful when you want to copy data off the GPU for reading by the CPU.</summary>
        Staging = 32,

        /// <summary>The texture is dynamic. Dynamic textures can only be written to by the CPU and read from by the GPU. The GPU cannot write (or copy to) a dynamic
        /// texture. Dynamic textures are best used in a situation where their data will be changed 1 or more times per frame by the CPU.</summary>
        Dynamic = 64,
    }

}
