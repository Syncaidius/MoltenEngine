using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class DebugStatsPage : DebugOverlayPage
    {
        public override void Render(SpriteFont font, RendererDX11 renderer, SpriteBatch batch, SceneRenderData scene, IRenderSurface surface)
        {
            RenderFrameSnapshot frame = renderer.Profiler.PreviousFrame;

            Vector2F pos = new Vector2F(3, 3);
            double fps = Math.Max(0.0, 1000.0f / frame.Time);
            double frameTime = renderer.Profiler.PreviousFrame.Time;
            double frameTimePercentage = (frameTime / frame.TargetTime) * 100;
            double frameDelta = frameTime / frame.TargetTime;

            batch.DrawString(font, $"FPS: {fps:N2}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Frame Time: {frameTime}ms -- {frameTimePercentage}%", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Delta: x{frameDelta}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Frame ID: {frame.FrameID}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Draw Calls: {frame.DrawCalls}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Bindings: {frame.Bindings}", pos, Color.Yellow);

            RenderProfiler profiler = renderer.Device.Profiler;
            pos.Y += 20; batch.DrawString(font, $"Swaps -- Buffer: {frame.BufferSwaps} -- Shader: {frame.ShaderSwaps} -- RT: {frame.RTSwaps}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Maps: ", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Discard: {frame.MapDiscardCount}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   No-overwrite: {frame.MapNoOverwriteCount}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Write: {frame.MapWriteCount}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Update Subresource: {frame.UpdateSubresourceCount}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Copy Subresource: {frame.CopySubresourceCount}", pos, Color.Yellow);
        }
    }
}
