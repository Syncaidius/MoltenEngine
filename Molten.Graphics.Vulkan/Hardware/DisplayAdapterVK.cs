using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
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

            Capabilities = _manager.CapBuilder.Build(_device, _manager.Renderer, ref p);

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

        /// <inheritdoc/>
        public void AddActiveOutput(IDisplayOutput output)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveActiveOutput(IDisplayOutput output)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveAllActiveOutputs()
        {
            throw new NotImplementedException();
        }

        public static implicit operator PhysicalDevice(DisplayAdapterVK adapter)
        {
            return adapter._device;
        }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public DeviceID ID { get; private set; }

        /// <inheritdoc/>
        public DeviceVendor Vendor { get; private set; }

        /// <inheritdoc/>
        public DisplayAdapterType Type { get; private set; }

        /// <inheritdoc/>
        public IDisplayManager Manager => _manager;

        /// <summary>
        /// Gets the underlying Vulkan <see cref="PhysicalDevice"/>.
        /// </summary>
        internal PhysicalDevice Native => _device;

        /// <inheritdoc/>
        public GraphicsCapabilities Capabilities { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayOutput> Outputs => throw new NotImplementedException();

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayOutput> ActiveOutputs => throw new NotImplementedException();
    }
}
