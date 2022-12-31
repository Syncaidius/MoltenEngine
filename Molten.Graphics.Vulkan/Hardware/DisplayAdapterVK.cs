using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Hardware
{
    public unsafe class DisplayAdapterVK : IDisplayAdapter
    {
        public event DisplayOutputChanged OnOutputActivated;
        public event DisplayOutputChanged OnOutputDeactivated;

        DisplayManagerVK _manager;
        PhysicalDevice _device;

        internal DisplayAdapterVK(DisplayManagerVK manager, PhysicalDevice device)
        {
            _manager = manager;
            _device = device;

            GetProperties();
        }

        private void GetProperties()
        {
            PhysicalDeviceProperties2 p = new PhysicalDeviceProperties2(StructureType.PhysicalDeviceProperties2);
            _manager.Renderer.VK.GetPhysicalDeviceProperties2(_device, &p);

            Name = SilkMarshal.PtrToString((nint)p.Properties.DeviceName, NativeStringEncoding.UTF8);
            ID = ParseDeviceID(p.Properties.DeviceID);
            Vendor = ParseVendorID(p.Properties.VendorID);
            Type = (DisplayAdapterType)p.Properties.DeviceType;

            PhysicalDeviceFeatures2 dFeatures = new PhysicalDeviceFeatures2(StructureType.PhysicalDeviceFeatures2);
            _manager.Renderer.VK.GetPhysicalDeviceFeatures2(_device, &dFeatures);

            Capabilities = _manager.CapBuilder.Build(ref p, ref p.Properties.Limits, ref dFeatures.Features);

#if DEBUG
            _manager.CapBuilder.LogAdditionalProperties(_manager.Renderer.Log, &p);
#endif
        }

        private DeviceVendor ParseVendorID(uint vendorID)
        {
            // From docs: If the vendor has a PCI vendor ID, the low 16 bits of vendorID must contain that PCI vendor ID, and the remaining bits must be set to zero. 
            if ((vendorID & 0xFFFF0000) == 0)
                return EngineUtil.VendorFromPCI(vendorID & 0x0000FFFF); // PCI vendor ID
            else
                return (DeviceVendor)(vendorID & 0xFFFF0000); // Vulkan Vendor ID
        }

        private DeviceID ParseDeviceID(uint deviceID)
        {
            // Docs: The vendor is also responsible for the value returned in deviceID. If the implementation is driven primarily by a PCI device with a PCI device ID,
            //      the low 16 bits of deviceID must contain that PCI device ID, and the remaining bits must be set to zero. 
            if ((deviceID & 0xFFFF0000) == 0) 
                return new DeviceID((deviceID & 0x0000FFFF)); // PCI device ID.
            else
                return new DeviceID(deviceID); // OS/Platform-based device ID.
        }

        public void AddActiveOutput(IDisplayOutput output)
        {
            throw new NotImplementedException();
        }

        public void GetActiveOutputs(List<IDisplayOutput> outputList)
        {
            throw new NotImplementedException();
        }

        public void GetAttachedOutputs(List<IDisplayOutput> outputList)
        {
            throw new NotImplementedException();
        }

        public IDisplayOutput GetOutput(int id)
        {
            throw new NotImplementedException();
        }

        public void RemoveActiveOutput(IDisplayOutput output)
        {
            throw new NotImplementedException();
        }

        public void RemoveAllActiveOutputs()
        {
            throw new NotImplementedException();
        }

        public string Name { get; private set; }

        public double DedicatedVideoMemory { get; private set; }

        public double DedicatedSystemMemory { get; private set; }

        public double SharedSystemMemory { get; private set; }

        public DeviceID ID { get; private set; }

        public DeviceVendor Vendor { get; private set; }

        public DisplayAdapterType Type { get; private set; }

        public int OutputCount { get; }

        public IDisplayManager Manager => _manager;

        public GraphicsCapabilities Capabilities { get; private set; }
    }
}
