namespace Molten.Graphics
{
    public partial class SpriteBatcher
    {
        EllipseStyle _ellipseStyle = EllipseStyle.Default;

        /// <summary>
        /// Draws an ellipse..
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="color">The color of the ellipse. Overrides the <see cref="EllipseStyle.FillColor"/> of the default <see cref="EllipseStyle"/>.</param>
        /// <param name="arraySlice">The texture array slice. This is ignored if <paramref name="texture"/> is null.</param>
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="texture">The texture, or null if none.</param>
        /// <param name="surfaceSlice"></param>
        public void DrawEllipse(ref Ellipse e, Color color, float rotation = 0, ITexture2D texture = null, Material material = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            _ellipseStyle.FillColor = color;
            _ellipseStyle.BorderThickness = 0;

            RectangleF source = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            DrawEllipse(ref e, ref _ellipseStyle, rotation, texture, material, arraySlice, surfaceSlice);
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="arraySlice">The texture array slice. This is ignored if <paramref name="texture"/> is null.</param>
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="texture">The texture, or null if none.</param>
        /// <param name="style">The style to use when drawing the ellipse.</param>
        public void DrawEllipse(ref Ellipse e, ref EllipseStyle style, float rotation = 0, ITexture2D texture = null, Material material = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            DrawEllipse(ref e, DEFAULT_ORIGIN_CENTER, ref style, rotation, texture, material, arraySlice, surfaceSlice);
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="arraySlice">The texture array slice. This is ignored if <paramref name="texture"/> is null.</param>
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="texture">The texture, or null if none.</param>
        /// <param name="origin">The origin of the ellipse, between 0f and 1.0f. An origin of 0.5f,0.5f would be the center of the sprite.</param>
        /// <param name="style">The style to use when drawing the ellipse.</param>
        public unsafe void DrawEllipse(ref Ellipse e, Vector2F origin, ref EllipseStyle style, float rotation = 0, ITexture2D texture = null, 
            Material material = null, uint arraySlice = 0, uint surfaceSlice = 0)
        {
            RectangleF source = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;

            ref GpuData data = ref GetData(RangeType.Ellipse, texture, material);
            data.Position = e.Center;
            data.Rotation = rotation + e.StartAngle;
            data.Array.SrcArraySlice = arraySlice;
            data.Array.DestSurfaceSlice = surfaceSlice;
            data.Size = new Vector2F(e.RadiusX * 2, e.RadiusY * 2);
            data.Color1 = style.FillColor;
            data.Color2 = style.BorderColor;
            data.Origin = origin;
            data.UV = *(Vector4F*)&source; // Source rectangle values are stored in the same layout as we need for UV: left, top, right, bottom.

            if (data.Color2.A > 0)
            {
                data.Extra.D1 = style.BorderThickness / data.Size.X; // Convert to UV coordinate system (0 - 1) range
                data.Extra.D2 = style.BorderThickness / data.Size.Y; // Convert to UV coordinate system (0 - 1) range
            }
            else
            {
                data.Extra.D1 = 0;
                data.Extra.D2 = 0;
            }

            data.Extra.D3 = e.GetAngleRange();
        }
    }
}
