using Molten.Audio.OpenAL;
using Molten.Graphics;
using Molten.Input;

namespace Molten.Examples // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static ExampleBrowser<RendererDX11, WinInputService, AudioServiceAL> _browser;

        static void Main(string[] args)
        {
            EngineSettings settings = new EngineSettings();
            settings.Graphics.EnableDebugLayer.Value = true;
            settings.Graphics.VSync.Value = true;
            settings.Graphics.BufferingMode.Value = BackBufferMode.Double;

            _browser = new ExampleBrowser<RendererDX11, WinInputService, AudioServiceAL>("Example Browser");
            _browser.Start(settings, true);
        }
    }
}
