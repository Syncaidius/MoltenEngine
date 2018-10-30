using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;

namespace Molten.Graphics
{
    public class DisplayManagerGL : IDisplayManager
    {
        GraphicsAdapterGL _adapter;
        GameWindow _window;
        IGraphicsContext _context;
        Logger _log;

        public void Initialize(Logger log, GraphicsSettings settings)
        {
            _log = log;
            _window = new GameWindow();
            _context = _window.Context;
            _adapter = new GraphicsAdapterGL(this, 0);

            // Log preferred adapter stats
            _log.WriteLine($"Chosen {_adapter.Name.Replace("\0", "")}");
            _log.WriteLine($"    Dedicated VRAM: {_adapter.DedicatedVideoMemory:N2} MB");
            _log.WriteLine($"    System RAM dedicated to video: {_adapter.DedicatedSystemMemory:N2} MB");
            _log.WriteLine($"    Shared system RAM: {_adapter.SharedSystemMemory:N2} MB");
            _log.WriteLine("    Attached displays: ");
            List<IDisplayOutput> displays = new List<IDisplayOutput>();
            _adapter.GetAttachedOutputs(displays);
            for (int d = 0; d < displays.Count; d++)
                _log.WriteLine($"       Display {d}: {displays[d].Name}");
        }

        public void Dispose()
        {
            _context.Dispose();
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

        internal GameWindow Window => _window;

        internal IGraphicsContext Context => _context;
    }
}
