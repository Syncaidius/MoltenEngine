using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

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

    public static void Throw(this Result r, DeviceVK device, Func<string> getMessageCallback = null)
    {
        if (r != Result.Success)
        {
            string msg;
            if (getMessageCallback == null)
                msg = "API operation failed";
            else
                msg = getMessageCallback();

            throw new Exception(msg);
        }
    }
}
