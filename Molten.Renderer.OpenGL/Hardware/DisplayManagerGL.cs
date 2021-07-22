using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Molten.Graphics
{
    public class DisplayManagerGL : IDisplayManager
    {
        GraphicsAdapterGL _adapter;
        Logger _log;

        public void Initialize(Logger log, GraphicsSettings settings)
        {
            _log = log;

            // Create a device GL context for the purpose of detecting hardware info.
            DeviceContext deviceContext = DeviceContext.Create();
            deviceContext.IncRef();
            IntPtr detectionContext = deviceContext.CreateContext(IntPtr.Zero); 

            deviceContext.MakeCurrent(detectionContext);

            _adapter = new GraphicsAdapterGL(this, 0, detectionContext);

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

            // Dispose of the temporary context now we're done.
            deviceContext.MakeCurrent(IntPtr.Zero);
            deviceContext.DeleteContext(detectionContext);
            deviceContext.Dispose();
        }

        public void Dispose()
        {
            
        }

        public IDisplayAdapter GetAdapter(int id)
        {
            if (id != 0)
                throw new AdapterException(null, "OpenGL renderer only supports adapter 0.");

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
