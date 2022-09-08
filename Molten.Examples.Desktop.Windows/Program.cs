using Molten.Audio.OpenAL;
using Molten.Graphics;
using Molten.Input;
using Molten.Samples;

namespace Molten.Examples // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static ExampleBrowser<RendererDX11, WinInputService, AudioServiceAL> _browser;

        static void Main(string[] args)
        {
            EngineSettings settings = new EngineSettings();
            settings.Graphics.EnableDebugLayer.Value = false;
            settings.Graphics.VSync.Value = true;

            _browser = new ExampleBrowser<RendererDX11, WinInputService, AudioServiceAL>("Example Browser");
            _browser.Start(settings, false);
        }
    }
}
