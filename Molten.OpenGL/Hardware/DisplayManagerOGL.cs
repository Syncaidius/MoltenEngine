using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Molten.Graphics
{
    public class DisplayManagerOGL : IDisplayManager
    {
        List<OpenGLAdapter> _adapters;

        public void Initialize(Logger logger, GraphicsSettings settings)
        {
            HashSet<DisplayDevice> displays = new HashSet<DisplayDevice>();

            int last = (int)DisplayIndex.Sixth + 1;
            for (int i = -1; i < last; i++)
            {
                DisplayDevice display = DisplayDevice.GetDisplay((DisplayIndex)i);
                if (display != null)
                    displays.Add(display);
            }
        }

        public void Dispose()
        {

        }

        public IDisplayAdapter GetAdapter(int id)
        {
            throw new NotImplementedException();            
        }

        public void GetAdapters(List<IDisplayAdapter> output)
        {
            output.AddRange(_adapters);
        }

        public void GetAdaptersWithOutputs(List<IDisplayAdapter> output)
        {
            throw new NotImplementedException();
        }


        public int AdapterCount => _adapters.Count;

        public IDisplayAdapter DefaultAdapter => throw new NotImplementedException();

        public IDisplayAdapter SelectedAdapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    }
}
