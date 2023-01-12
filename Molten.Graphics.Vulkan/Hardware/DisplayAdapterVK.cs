using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe class DisplayAdapterVK : NativeObjectVK<PhysicalDevice>, IDisplayAdapter
    {
        public event DisplayOutputChanged OnOutputActivated;
        public event DisplayOutputChanged OnOutputDeactivated;

        DisplayManagerVK _manager;

        List<DisplayOutputVK> _outputs;
        List<DisplayOutputVK> _activeOutputs;

        internal DisplayAdapterVK(DisplayManagerVK manager, PhysicalDevice device)
        {
            _manager = manager;
            Native = device;

            GetProperties();
        }

        protected override void OnDispose()
        {
            
        }

        private void GetProperties()
        {
            PhysicalDeviceProperties2 p = new PhysicalDeviceProperties2(StructureType.PhysicalDeviceProperties2);
            _manager.Renderer.VK.GetPhysicalDeviceProperties2(Native, &p);

            Name = SilkMarshal.PtrToString((nint)p.Properties.DeviceName, NativeStringEncoding.UTF8);
            ID = ParseDeviceID(p.Properties.DeviceID);
            Vendor = ParseVendorID(p.Properties.VendorID);
            Type = (DisplayAdapterType)p.Properties.DeviceType;

            Capabilities = _manager.CapBuilder.Build(Native, _manager.Renderer, ref p);

#if DEBUG
            _manager.CapBuilder.LogAdditionalProperties(_manager.Renderer.Log, &p);
#endif

            _outputs = new List<DisplayOutputVK>();
            _activeOutputs = new List<DisplayOutputVK>();
            Outputs = _outputs.AsReadOnly();
            ActiveOutputs = _activeOutputs.AsReadOnly();
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

        internal bool AssociateOutput(DisplayOutputVK output)
        {
            if (output.AssociatedAdapter == this)
                return true;

            output.AssociatedAdapter?.UnassociateOutput(output);

            _outputs.Add(output);
            output.AssociatedAdapter = this;

            return false;
        }

        internal void UnassociateOutput(DisplayOutputVK output)
        {
            if (output.AssociatedAdapter != this)
                return;

            int index = _activeOutputs.IndexOf(output);
            if (index > -1)
                _activeOutputs.RemoveAt(index);

            _outputs.Remove(output);
            output.AssociatedAdapter = null;
        }

        /// <inheritdoc/>
        public void AddActiveOutput(IDisplayOutput output)
        {
            if (output is DisplayOutputVK vkOutput)
            {
                if (vkOutput.AssociatedAdapter != this)
                    return;

                if (!_activeOutputs.Contains(vkOutput))
                    _activeOutputs.Add(vkOutput);
            }
        }

        /// <inheritdoc/>
        public void RemoveActiveOutput(IDisplayOutput output)
        {
            if (output is DisplayOutputVK vkOutput)
            {
                if (vkOutput.AssociatedAdapter != this)
                    return;

                _activeOutputs.Remove(vkOutput);
            }
        }

        /// <inheritdoc/>
        public void RemoveAllActiveOutputs()
        {
            throw new NotImplementedException();
        }

        public static implicit operator PhysicalDevice(DisplayAdapterVK adapter)
        {
            return adapter.Native;
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
        public DisplayManager Manager => _manager;

        /// <inheritdoc/>
        public GraphicsCapabilities Capabilities { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayOutput> Outputs { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayOutput> ActiveOutputs { get; private set; }
    }
}
