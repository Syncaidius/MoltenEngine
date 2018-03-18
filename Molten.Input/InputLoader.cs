using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    /// <summary>Serves one purpose only: To load a renderer library.</summary>
    public class InputLoader
    {
        Assembly _inputAssembly;

        public IInputManager GetManager(Logger log, InputSettings settings)
        {
            string defaultLibrary = InputSettings.DEFAULT_LIBRARY;
            IInputManager input = LibraryDetection.LoadInstance<IInputManager>(log, "input", "input manager", settings.InputLibrary, settings, defaultLibrary, out _inputAssembly);

            // Initialize
            try
            {
                input.Initialize(settings, log);
                log.WriteLine($"Initialized input manager");
            }
            catch (Exception e)
            {
                log.WriteLine("Failed to initialize input manager");
                log.WriteError(e);
            }

            return input;
        }
    }
}
