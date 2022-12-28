using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Hardware
{
    public class DisplayAdapterVK : IDisplayAdapter
    {
        public event DisplayOutputChanged OnOutputActivated;
        public event DisplayOutputChanged OnOutputDeactivated;

        DisplayManagerVK _manager;
        GraphicsCapabilities _cap;

        internal DisplayAdapterVK(DisplayManagerVK manager, GraphicsCapabilities cap, ref PhysicalDeviceProperties properties)
        {
            _manager = manager;
            _cap = cap;
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

        public string Name { get; }

        public double DedicatedVideoMemory { get; }

        public double DedicatedSystemMemory { get; }

        public double SharedSystemMemory { get; }

        public int ID { get; }

        public GraphicsAdapterVendor Vendor { get; }

        public int OutputCount { get; }

        public IDisplayManager Manager => _manager;

        public GraphicsCapabilities Capabilities { get; internal set; }
    }
}
