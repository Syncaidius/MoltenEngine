using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class DisplayManagerVK : IDisplayManager
    {
        public int AdapterCount => throw new NotImplementedException();

        public IDisplayAdapter DefaultAdapter => throw new NotImplementedException();

        public IDisplayAdapter SelectedAdapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal DisplayManagerVK(RendererVK renderer)
        {

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IDisplayAdapter GetAdapter(int id)
        {
            throw new NotImplementedException();
        }

        public void GetAdapters(List<IDisplayAdapter> output)
        {
            throw new NotImplementedException();
        }

        public void GetAdaptersWithOutputs(List<IDisplayAdapter> output)
        {
            throw new NotImplementedException();
        }

        public void Initialize(Logger logger, GraphicsSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
