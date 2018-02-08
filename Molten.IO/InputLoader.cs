using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    /// <summary>Serves one purpose only: To load a renderer library.</summary>
    public class InputLoader
    {
        InputSettings _settings;
        Assembly _renderAssembly;
        Logger _log;

        public InputLoader(Logger log, InputSettings settings)
        {
            _settings = settings;
            _log = log;
        }

        public IInputManager GetRenderer()
        {
            string defaultRenderer = GraphicsSettings.RENDERER_DX11; // TODO default to OpenGL if on a non-windows platform.


            string[] parts = _settings.RendererLibrary.Value.Split(';');
            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim().ToLower();

            string path = parts[0];
            if (!File.Exists(path))
                path = defaultRenderer;

            _log.WriteLine($"Attempting to load render library {parts[0]}");

            // Default to DX11 renderer if provided one is not found.
            _renderAssembly = LibraryDetection.LoadLibary(parts[0]);
            List<Type> renderers = LibraryDetection.FindType<IRenderer>(_renderAssembly).ToList();
            if (renderers.Count == 0)
            {
                _log.WriteLine("Provided render library has no implementations of IRenderer");
                _log.WriteLine($"Defaulting to {defaultRenderer} render library, please restart program");
                _settings.RendererLibrary.Value = defaultRenderer;
                _settings.Apply();
                return null;
            }

            // Attempt to instanciate renderer
            IRenderer renderer = null;
            Type rType = null;
            foreach (Type t in renderers)
            {
                if (t.FullName.ToLower() == parts[1])
                {
                    _log.WriteLine($"Provided renderer found: {parts[1]}");
                    rType = t;
                    break;
                }
            }

            // Instanciate
            if (rType != null)
            {
                try
                {
                    renderer = Activator.CreateInstance(rType) as IRenderer;
                    _log.WriteLine($"Created renderer");
                }
                catch (Exception e)
                {
                    _log.WriteLine("Failed to create renderer instance");
                    _log.WriteError(e);
                }
            }
            else
            {
                _log.WriteLine($"No renderers found in {parts[0]} matching the name {parts[1]}");
                return null;
            }

            // Initialize
            try
            {
                renderer.InitializeAdapter(_settings);
                _log.WriteLine($"Initialized renderer");
            }
            catch (Exception e)
            {
                _log.WriteLine("Failed to initialize renderer");
                _log.WriteError(e);
            }

            return renderer;
        }
    }
}
