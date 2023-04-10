using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class FenceVK : GraphicsFence, IDisposable
    {
        Fence _native;
        DeviceVK _device;

        internal unsafe FenceVK(DeviceVK device, FenceCreateFlags flags)
        {
            _device = device;

            _native = new Fence();
            FenceCreateInfo fInfo = new FenceCreateInfo(StructureType.FenceCreateInfo, null, flags);

            Fence f = new Fence();
            Result r = device.VK.CreateFence(_device, &fInfo, null, &f);
            r.Check(_device, () => $"Failed to create fence with flags [{flags}]");
            _native = f;
        }

        internal unsafe bool CheckStatus()
        {
            // See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/vkGetFenceStatus.html
            Result r = _device.VK.GetFenceStatus(_device, _native);
            if(r == Result.Success)
                return true;

            return false;
        }

        public override unsafe void Reset()
        {
            fixed (Fence* ptrFence = &_native)
                _device.VK.ResetFences(_device, 1, ptrFence);
        }

        public override bool Wait(ulong nsTimeout = ulong.MaxValue)
        {
            Span<Fence> f = stackalloc Fence[] { _native };
            Result r = _device.VK.WaitForFences(_device, 1, f, true, nsTimeout);
            return r == Result.Success;
        }

        public unsafe void Dispose()
        {
            _device.VK.DestroyFence(_device, _native, null);
        }

        public static implicit operator Fence(FenceVK fence)
        {
            return fence._native;
        }

        internal ref Fence Ptr => ref _native;
    }
}
