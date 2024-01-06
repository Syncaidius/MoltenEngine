using Silk.NET.Vulkan;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Molten.Graphics.Vulkan;

internal class SemaphoreVK : IDisposable
{
    Semaphore _semaphore;
    DeviceVK _device;

    internal unsafe SemaphoreVK(DeviceVK device)
    {
        _device = device;
    }

    internal unsafe void Start(SemaphoreCreateFlags flags)
    {
        if(_semaphore.Handle != 0)
            _device.VK.DestroySemaphore(_device, _semaphore, null);

        _semaphore = new Semaphore();
        SemaphoreCreateInfo fInfo = new SemaphoreCreateInfo(StructureType.SemaphoreCreateInfo, null, flags);

        Semaphore s = new Semaphore();
        Result r = _device.VK.CreateSemaphore(_device, &fInfo, null, &s);
        r.Check(_device, () => $"Failed to create semaphore with flags [{flags}]");
        _semaphore = s;
    }

    public unsafe void Dispose()
    {
        if (_semaphore.Handle != 0)
            _device.VK.DestroySemaphore(_device, _semaphore, null);

        _semaphore = new Semaphore();
    }

    public static implicit operator Semaphore(SemaphoreVK semaphore)
    {
        return semaphore._semaphore;
    }

    internal ref Semaphore Ptr => ref _semaphore;
}
