namespace Molten.Graphics.Overlays
{
    public class RenderProfilerOverlay : IRenderOverlay
    {
        ulong _lastFrameID = ulong.MaxValue;
        Color _colText;

        internal RenderProfilerOverlay()
        {
            _colText = Color.Yellow;
        }

        public void OnRender(Timing time, SpriteBatcher sb, SpriteFont font, RenderProfiler rendererProfiler, RenderProfiler sceneProfiler, RenderCamera camera)
        {
            Vector2F textPos = new Vector2F(5, 5);
            float lineHeight = font.GetHeight('|');
            float graphWidth = 400;

            // Renderer frame profiler stats.
            RenderProfiler.Snapshot frame = rendererProfiler.Previous;
            if (frame != null)
            {
                sb.DrawString(font, "[LAST FRAME]", textPos, _colText);
                textPos.X += 5;
                DrawStats(time, sb, font, frame, ref textPos, lineHeight);
                textPos.X -= 5;
            }
        }

        private void DrawStats(Timing time, SpriteBatcher sb, SpriteFont font, RenderProfiler.Snapshot frame, ref Vector2F textPos, float lineHeight)
        {
            textPos.Y += lineHeight; sb.DrawString(font, $"FPS: {time.FPS} -- Frame: {time.FrameID}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Time: {frame.Time.ToString("N2")}ms", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Target: {frame.TargetTime.ToString("N2")}ms", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Delta: {(frame.Time / frame.TargetTime).ToString("N2")}ms", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"VRAM usage: {frame.AllocatedVRAM}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Draw calls: {frame.DrawCalls}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Primatives: {frame.PrimitiveCount}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Buffer swaps: {frame.BufferBindings}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Shader swaps: {frame.ShaderBindings}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Surface swaps: {frame.SurfaceBindings}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Sub-resource update: {frame.UpdateSubresourceCount}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Sub-resource copy: {frame.CopySubresourceCount}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Map discard: {frame.MapDiscardCount}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Map no-overwrite: {frame.MapNoOverwriteCount}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Map read: {frame.MapReadWriteCount}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Map write: {frame.MapReadWriteCount}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Resource bindings: {frame.GpuBindings}", textPos, _colText);
        }

        public string Title => "Render Statistics";
    }
}
