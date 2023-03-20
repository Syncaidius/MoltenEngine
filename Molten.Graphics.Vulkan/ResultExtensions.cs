using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal static class ResultExtensions
    {
        public static bool Check(this Result r, RenderService renderer, Func<string> getMessageCallback = null)
        {
            if (r != Result.Success)
            {
                if (getMessageCallback == null)
                    renderer.Log.Error($"Vulkan error: {r}");
                else
                    renderer.Log.Error($"Vulkan error: {r} -- {getMessageCallback()}");

                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this Result r, DeviceVK device, Func<string> getMessageCallback = null)
        {
            return Check(r, device.Renderer, getMessageCallback);
        }
    }
}
