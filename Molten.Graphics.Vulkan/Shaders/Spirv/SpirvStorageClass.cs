using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public enum SpirvStorageClass
    {
        UniformConstant = 0,
        Input = 1,
        Uniform = 2,
        Output = 3,
        Workgroup = 4,
        CrossWorkgroup = 5,
        Private = 6,
        Function = 7,
        Generic = 8,
        PushConstant = 9,
        AtomicCounter = 10,
        Image = 11,
        StorageBuffer = 12,
        TileImageEXT = 4172,
        CallableDataKHR = 5328,
        CallableDataNV = 5328,
        IncomingCallableDataKHR = 5329,
        IncomingCallableDataNV = 5329,
        RayPayloadKHR = 5338,
        RayPayloadNV = 5338,
        HitAttributeKHR = 5339,
        HitAttributeNV = 5339,
        IncomingRayPayloadKHR = 5342,
        IncomingRayPayloadNV = 5342,
        ShaderRecordBufferKHR = 5343,
        ShaderRecordBufferNV = 5343,
        PhysicalStorageBuffer = 5349,
        PhysicalStorageBufferEXT = 5349,
        HitObjectAttributeNV = 5385,
        TaskPayloadWorkgroupEXT = 5402,
        CodeSectionINTEL = 5605,
        DeviceOnlyINTEL = 5936,
        HostOnlyINTEL = 5937,
        Max = 0x7fffffff,
    }
}
