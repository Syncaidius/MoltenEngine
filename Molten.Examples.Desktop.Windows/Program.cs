using Molten.Audio.OpenAL;
using Molten.Graphics;
using Molten.Graphics.DX11;
using Molten.Graphics.DX12;
using Molten.Input;
using Silk.NET.Direct3D12;

namespace Molten.Examples;

internal class Program
{
    static ExampleBrowser<RendererDX11, WinInputService, AudioServiceAL> _browser;

    static void Main(string[] args)
    {
        unsafe
        {
            int test = sizeof(DepthStencilDesc1);
        }
        EngineSettings settings = new EngineSettings();
        settings.Graphics.EnableDebugLayer.Value = true;
        settings.Graphics.VSync.Value = true;
        settings.Graphics.FrameBufferMode.Value = FrameBufferMode.Double;

        _browser = new ExampleBrowser<RendererDX11, WinInputService, AudioServiceAL>("Example Browser");
        _browser.Start(settings, true);
    }
}
