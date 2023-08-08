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

        public void OnRender(Timing time, SpriteBatcher sb, SpriteFont font, GraphicsProfiler rendererProfiler, RenderCamera camera)
        {
            Vector2F textPos = new Vector2F(20, 20);
            float lineHeight = font.GetHeight('|');

            // Renderer frame profiler stats.
            GraphicsProfiler.FrameProfile frame = rendererProfiler.Previous;
            if (frame != null)
            {
                DrawStats(time, sb, font, frame, ref textPos, lineHeight, "[FRAME]");
                textPos.Y += 45;
            }
        }

        private void DrawStats(Timing time, SpriteBatcher sb, SpriteFont font, GraphicsProfiler.FrameProfile frame, ref Vector2F textPos, float lineHeight, string title)
        {
            RectangleF r = new RectangleF(textPos, new Vector2F(200, lineHeight * 18));
            r.Inflate(8);
            sb.DrawRect(r, ref _bgStyle);

            sb.DrawString(font, title, textPos, _colText);
            textPos.Y += 5;
            textPos.Y += lineHeight; sb.DrawString(font, $"FPS:                 {time.FPS} -- Frame: {time.FrameID}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Time:                {frame.TimeTaken:N2}ms", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Target:              {frame.TargetTime:N2}ms", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Delta:               {(frame.TimeTaken / frame.TargetTime):N2}ms", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"VRAM usage:          {frame.VideoMemoryAllocated}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Draw calls:          {frame.DrawCalls}", textPos, _colText);
            //textPos.Y += lineHeight; sb.DrawString(font, $"Primatives:        {frame.PrimitiveCount}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Binds - Buffers:     {frame.BindBufferCalls}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Binds - Shaders:     {frame.BindShaderCalls}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Binds - Surfaces:    {frame.BindSurfaceCalls}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Binds - Resources:   {frame.BindResourceCalls}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Sub-resource update: {frame.SubResourceUpdateCalls}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Sub-resource copy:   {frame.SubResourceCopyCalls}", textPos, _colText);
            textPos.Y += lineHeight; sb.DrawString(font, $"Map discard:         {frame.ResourceMapCalls}", textPos, _colText);
        }

        public string Title => "Render Statistics";
    }
}
