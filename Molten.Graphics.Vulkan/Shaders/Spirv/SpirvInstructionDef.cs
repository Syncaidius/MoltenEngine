using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    internal class SpirvInstructionDef
    {
        public uint ID { get; set; }

        /// <summary>
        /// A list of alias instructions
        /// </summary>
        public string[] Alias { get; set; }

        public string DeprecationMessage { get; set; }

        public Dictionary<string, string> Words { get; set; } = new Dictionary<string, string>();
    }
}
