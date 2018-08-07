using Molten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainEditor
{
    class Program
    {
        static Foundation _core;

        static void Main(string[] args)
        {
            EngineSettings settings = new EngineSettings();
            settings.Graphics.EnableDebugLayer.Value = false;
            settings.Graphics.VSync.Value = true;
            settings.UseGuiControl = false;

            _core = new EditorCore(settings);
            _core.Start();
        }
    }
}
