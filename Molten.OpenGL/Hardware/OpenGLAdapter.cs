using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class OpenGLAdapter : IDisplayAdapter
    {
        public string Name => throw new NotImplementedException();

        public double DedicatedVideoMemory => throw new NotImplementedException();

        public double DedicatedSystemMemory => throw new NotImplementedException();

        public double SharedSystemMemory => throw new NotImplementedException();

        public int ID => throw new NotImplementedException();

        public int VendorID => throw new NotImplementedException();

        public int DeviceID => throw new NotImplementedException();

        public long UniqueID => throw new NotImplementedException();

        public int Revision => throw new NotImplementedException();

        public int SubsystemID => throw new NotImplementedException();

        public int OutputCount => throw new NotImplementedException();

        public IDisplayManager Manager => throw new NotImplementedException();

        public event DisplayOutputChanged OnOutputAdded;
        public event DisplayOutputChanged OnOutputRemoved;

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
