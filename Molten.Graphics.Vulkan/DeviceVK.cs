using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe class DeviceVK : IDisplayAdapter
    {
        PhysicalDevice* _device;

        public string Name => throw new NotImplementedException();

        public double DedicatedVideoMemory => throw new NotImplementedException();

        public double DedicatedSystemMemory => throw new NotImplementedException();

        public double SharedSystemMemory => throw new NotImplementedException();

        public int ID => throw new NotImplementedException();

        public GraphicsAdapterVendor Vendor => throw new NotImplementedException();

        public int OutputCount => throw new NotImplementedException();

        public IDisplayManager Manager => throw new NotImplementedException();

        public event DisplayOutputChanged OnOutputActivated;
        public event DisplayOutputChanged OnOutputDeactivated;

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
    }
}
