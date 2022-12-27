using Molten.Audio.OpenAL;
using Molten.Graphics;
using Molten.Input;

namespace Molten.Examples // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static ExampleBrowser<RendererVK, WinInputService, AudioServiceAL> _browser;

        static void Main(string[] args)
        {
            EngineSettings settings = new EngineSettings();
            settings.Graphics.EnableDebugLayer.Value = true;
            settings.Graphics.VSync.Value = true;

            _browser = new ExampleBrowser<RendererVK, WinInputService, AudioServiceAL>("Example Browser");
            _browser.Start(settings, false);
        }
    }
}
