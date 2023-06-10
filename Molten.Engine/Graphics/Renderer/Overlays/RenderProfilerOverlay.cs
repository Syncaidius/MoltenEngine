namespace Molten.Graphics.Overlays
{
    public class RenderProfilerOverlay : IRenderOverlay
    {
        ulong _lastFrameID = ulong.MaxValue;
        Color _colText;
        RectStyle _bgStyle = new RectStyle()
        {
            FillColor = new Color(30, 30, 30, 220),
            BorderColor = new Color(120, 120, 120, 255),
            BorderThickness = new Thickness(1),
        };

        internal RenderProfilerOverlay()
        {
            _colText = Color.Yellow;
        }

        public void OnRender(Timing time, SpriteBatcher sb, SpriteFont font, RenderProfiler rendererProfiler, RenderProfiler sceneProfiler, RenderCamera camera)
        {
            Vector2F textPos = new Vector2F(20, 20);
            float lineHeight = font.GetHeight('|');

            // Renderer frame profiler stats.
            RenderProfiler.Snapshot frame = rendererProfiler.Previous;
            if (frame != null)
            {
                DrawStats(time, sb, font, frame, ref textPos, lineHeight, "[FRAME]");
                textPos.Y += 45;
            }

            // Scene profiler stats.
            frame = sceneProfiler.Previous;
            if (frame != null)
                DrawStats(time, sb, font, frame, ref textPos, lineHeight, "[SCENE]");
        }

        private void DrawStats(Timing time, SpriteBatcher sb, SpriteFont font, RenderProfiler.Snapshot frame, ref Vector2F textPos, float lineHeight, string title)
        {
            RectangleF r = new RectangleF(textPos, new Vector2F(200, lineHeight * 18));
            r.Inflate(8);
            sb.DrawRect(r, ref _bgStyle);

            sb.DrawString(font, title, textPos, _colText);
            textPos.Y += 5;
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
