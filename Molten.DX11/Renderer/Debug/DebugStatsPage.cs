using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class DebugStatsPage : DebugOverlayPage
    {
        public override void Render(ISpriteFont font, RendererDX11 renderer, SpriteBatch batch, Timing time)
        {
            Vector2 pos = new Vector2(3, 3);
            batch.DrawString(font, $"FPS: {time.UPS}", pos, Color.Yellow);

            double frameTime = renderer.FrameTime.TotalMilliseconds;
            double frameTimePercentage = frameTime / time.TargetFrameTime;
            pos.Y += 20; batch.DrawString(font, $"Frame Time: {frameTime}ms -- {frameTimePercentage}%", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"  After Vsync: {time.ElapsedTime.TotalMilliseconds}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Delta: x{time.Delta}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Frame ID: {time.CurrentFrame}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Draw Calls: {renderer.Device.Profiler.DrawCalls}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Bindings: {renderer.Device.Profiler.Bindings}", pos, Color.Yellow);

            RenderProfilerDX profiler = renderer.Device.Profiler;
            pos.Y += 20; batch.DrawString(font, $" Swaps -- Buffer: {profiler.BufferSwaps} -- Shader: {profiler.ShaderSwaps} -- RT: {profiler.RTSwaps}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"VRAM: {renderer.Device.Profiler.AllocatedVRAM}", pos, Color.Yellow);
        }
    }
}
