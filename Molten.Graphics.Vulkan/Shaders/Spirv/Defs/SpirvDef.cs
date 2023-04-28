using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Molten.Graphics.Vulkan
{
    internal class SpirvDef
    {
        [JsonProperty("major_version")]
        public int MajorVersion { get; set; }

        [JsonProperty("minor_version")]
        public int MinorVersion { get; set; }

        [JsonProperty("version")]
        public int Version
        {
            get => MinorVersion;
            set => MinorVersion = value;
        }

        [JsonProperty("revision")]
        public int Revision { get; set; }

        public SpirvInstructionDef[] Instructions { get; set; } = new SpirvInstructionDef[0];

        [JsonProperty("operand_kinds")]
        public SpirvOperandKindDef[] OperandKinds { get; set; } = new SpirvOperandKindDef[0];
    }
}
