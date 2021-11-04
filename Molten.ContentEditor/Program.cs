using Molten;
using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.ContentEditor
{
    class Program
    {
        static Foundation<RendererDX11, WinInputManager> _core;

        static void Main(string[] args)
        {
            EngineSettings settings = new EngineSettings();
            settings.Graphics.EnableDebugLayer.Value = false;
            settings.Graphics.VSync.Value = true;
            settings.UseGuiControl = false;

            _core = new EditorCore();
            _core.Start();
        }
    }
}
