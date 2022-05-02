namespace Molten.Graphics.Overlays
{
    public class RenderProfilerOverlay : IRenderOverlay
    {
        Color _col = Color.Yellow;
        GraphRenderer _fpsGraph;
        ulong _lastFrameID = ulong.MaxValue;

        internal RenderProfilerOverlay()
        {
            _fpsGraph = new GraphRenderer(1000);
        }

        public void OnRender(Timing time, SpriteBatcher sb, SpriteFont font, RenderProfiler rendererProfiler, RenderProfiler sceneProfiler, RenderCamera camera)
        {
            Vector2F textPos = new Vector2F(5, 5);
            float lineHeight = font.GetHeight('|', 16);
            float graphWidth = 400;
            _fpsGraph.MaxPoints = time.TargetUPS * 10; // Aim for 10 seconds worth of frames
            _fpsGraph.Bounds = new RectangleF(camera.OutputSurface.Width - graphWidth - 5, 5, graphWidth, 200);

            // Renderer frame profiler stats.
            RenderProfiler.Snapshot frame = rendererProfiler.Previous;
            if (frame != null)
            {
                if (_lastFrameID != frame.FrameID)
                    _fpsGraph.Add(time.FPS);

                sb.DrawString(font, 16, "[FRAME]", textPos, _col);
                textPos.X += 5;
                DrawStats(time, sb, font, frame, ref textPos, lineHeight);
                textPos.X -= 5;
            }

            // Scene profiler stats.
            frame = sceneProfiler.Previous;
            if (frame != null)
            {
                textPos.Y += lineHeight; sb.DrawString(font, 16, "[SCENE]", textPos, _col);
                textPos.Y += 5;
                DrawStats(time, sb, font, frame, ref textPos, lineHeight);
                textPos.X -= 5;
            }

            _fpsGraph.Render(sb, font);
        }

        private void DrawStats(Timing time, SpriteBatcher sb, SpriteFont font, RenderProfiler.Snapshot frame, ref Vector2F textPos, float lineHeight)
        {
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"FPS: {time.FPS}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Time: {frame.Time.ToString("N2")}ms", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Target: {frame.TargetTime.ToString("N2")}ms", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Delta: {(frame.Time / frame.TargetTime).ToString("N2")}ms", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"VRAM usage: {frame.AllocatedVRAM}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Draw calls: {frame.DrawCalls}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Primatives: {frame.PrimitiveCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Buffer swaps: {frame.BufferBindings}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Shader swaps: {frame.ShaderBindings}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Surface swaps: {frame.SurfaceBindings}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Sub-resource update: {frame.UpdateSubresourceCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Sub-resource copy: {frame.CopySubresourceCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Map discard: {frame.MapDiscardCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Map no-overwrite: {frame.MapNoOverwriteCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Map read: {frame.MapReadCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Map write: {frame.MapWriteCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, 16, $"Resource bindings: {frame.GpuBindings}", textPos, _col);
        }

        public string Title => "Render Statistics";
    }
}
