using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Molten.Graphics
{
    public class DisplayManagerGL : IDisplayManager
    {
        GraphicsAdapterGL _adapter;

        public void Initialize(Logger logger, GraphicsSettings settings)
        {
            _adapter = new GraphicsAdapterGL(this, 0);
        }

        public void Dispose()
        {

        }

        public IDisplayAdapter GetAdapter(int id)
        {
            if (id != 0)
                throw new GraphicsException("OpenGL renderer only supports adapter 0.");

            return _adapter;
        }

        public void GetAdapters(List<IDisplayAdapter> output)
        {
            output.Add(_adapter);
        }

        public void GetAdaptersWithOutputs(List<IDisplayAdapter> output)
        {
            if (_adapter.OutputCount > 0)
                output.Add(_adapter);
        }

        public int AdapterCount => 1;

        public IDisplayAdapter DefaultAdapter => _adapter;

        public IDisplayAdapter SelectedAdapter
        {
            get => _adapter;
            set => _adapter = (value as GraphicsAdapterGL) ?? _adapter;
        }
    }
}
