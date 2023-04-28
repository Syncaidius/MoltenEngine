using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public class SpirvEnumerantDef
    {
        public string Enumerant { get; set; }

        public uint Value { get; set; }

        public string[] Capabilities { get; set; } = new string[0];

        public SpirvKindDef[] Parameters { get; set; } = new SpirvKindDef[0];

        public string[] Extensions { get; set; } = new string[0];

        /// <summary>
        /// The minimum Vulkan API version required for this enumerant to be supported.
        /// </summary>
        public string Version { get; set; }
    }
}
