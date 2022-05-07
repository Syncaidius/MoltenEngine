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

        public void OnRender(Timing time, SpriteBatcher sb, TextFont font, RenderProfiler rendererProfiler, RenderProfiler sceneProfiler, RenderCamera camera)
        {
            Vector2F textPos = new Vector2F(5, 5);
            float lineHeight = font.GetHeight('|');
            float graphWidth = 400;
            _fpsGraph.MaxPoints = time.TargetUPS * 10; // Aim for 10 seconds worth of frames
            _fpsGraph.Bounds = new RectangleF(camera.OutputSurface.Width - graphWidth - 5, 5, graphWidth, 200);

            // Renderer frame profiler stats.
            RenderProfiler.Snapshot frame = rendererProfiler.Previous;
            if (frame != null)
            {
                if (_lastFrameID != frame.FrameID)
                    _fpsGraph.Add(time.FPS);

                sb.DrawString(font, "[FRAME]", textPos, _col);
                textPos.X += 5;
                DrawStats(time, sb, font, frame, ref textPos, lineHeight);
                textPos.X -= 5;
            }

            // Scene profiler stats.
            frame = sceneProfiler.Previous;
            if (frame != null)
            {
                textPos.Y += lineHeight; sb.DrawString(font, "[SCENE]", textPos, _col);
                textPos.Y += 5;
                DrawStats(time, sb, font, frame, ref textPos, lineHeight);
                textPos.X -= 5;
            }

            _fpsGraph.Render(sb, font);
        }

        private void DrawStats(Timing time, SpriteBatcher sb, TextFont font, RenderProfiler.Snapshot frame, ref Vector2F textPos, float lineHeight)
        {
            textPos.Y += lineHeight; sb.DrawString(font, $"FPS: {time.FPS}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Time: {frame.Time.ToString("N2")}ms", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Target: {frame.TargetTime.ToString("N2")}ms", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Delta: {(frame.Time / frame.TargetTime).ToString("N2")}ms", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"VRAM usage: {frame.AllocatedVRAM}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Draw calls: {frame.DrawCalls}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Primatives: {frame.PrimitiveCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Buffer swaps: {frame.BufferBindings}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Shader swaps: {frame.ShaderBindings}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Surface swaps: {frame.SurfaceBindings}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Sub-resource update: {frame.UpdateSubresourceCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Sub-resource copy: {frame.CopySubresourceCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Map discard: {frame.MapDiscardCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Map no-overwrite: {frame.MapNoOverwriteCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Map read: {frame.MapReadCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Map write: {frame.MapWriteCount}", textPos, _col);
            textPos.Y += lineHeight; sb.DrawString(font, $"Resource bindings: {frame.GpuBindings}", textPos, _col);
        }

        public string Title => "Render Statistics";
    }
}
