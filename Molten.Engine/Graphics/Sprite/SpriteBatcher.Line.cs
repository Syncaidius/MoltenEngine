namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        Color[] _singleColorList = new Color[] { Color.White };

        /// <summary>
        /// Placeholder line style for overloads which don't accept a style, but individual parameters instead.
        /// </summary>
        LineStyle _lineStyle;

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="color">The color of the lines</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        /// <param name="sharpness">The edge-sharpness of path lines.</param>
        public void DrawLinePath(IList<Vector2F> points, Color color, float thickness, float sharpness = 1.0f, uint surfaceSlice = 0)
        {
            _singleColorList[0] = color;
            DrawLinePath(points, 0, points.Count, _singleColorList, thickness, sharpness, surfaceSlice);
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="pointColors">A list of colors (one per point) that lines should transition to/from at each point.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        /// <param name="sharpness">The edge-sharpness of path lines.</param>
        public void DrawLinePath(IList<Vector2F> points, IList<Color> pointColors, float thickness, float sharpness = 1.0f, uint surfaceSlice = 0)
        {
            DrawLinePath(points, 0, points.Count, pointColors, thickness, sharpness, surfaceSlice);
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="Color">The color of all the lines in the path.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        /// <param name="startIndex">The start index within the points list from which to start drawing.</param>
        /// <param name="count">The number of points from the point list to draw.</param>
        /// <param name="sharpness">The edge-sharpness of path lines.</param>
        public void DrawLinePath(IList<Vector2F> points, int startIndex, int count, Color color, float thickness, float sharpness = 1.0f, uint surfaceSlice = 0)
        {
            _singleColorList[0] = color;
            DrawLinePath(points, startIndex, count, _singleColorList, thickness, sharpness, surfaceSlice);
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="pointColors">A list of colors (one per point) that lines should transition to/from at each point.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        /// <param name="startIndex">The start index within the points list from which to start drawing.</param>
        /// <param name="count">The number of points from the point list to draw.</param>
        /// <param name="sharpness">The edge-sharpness of path lines.</param>
        public void DrawLinePath(IList<Vector2F> points, int startIndex, int count, IList<Color> pointColors, float thickness, float sharpness = 1.0f, uint surfaceSlice = 0)
        {
            if (pointColors.Count == 0)
                throw new SpriteBatcherException(this, "There must be at least one color available in the pointColors list.");

            if (startIndex + count > points.Count)
                throw new SpriteBatcherException(this, "The sum of the start index and the count must be less than the point count.");

            if (count < 2)
                throw new SpriteBatcherException(this, "There must be at least 2 points in the point list.");

            if (count == 2)
            {
                int secondCol = pointColors.Count > 1 ? 1 : 0;
                DrawLine(points[0], points[1], pointColors[0], pointColors[secondCol], thickness, sharpness, surfaceSlice);
            }
            else
            {
                Vector2F p1, p2;
                int last = startIndex + count - 1;
                int next = startIndex + 1;

                _lineStyle.Sharpness = sharpness;
                _lineStyle.Thickness = thickness;

                int i = startIndex;
                p1 = points[i];
                _lineStyle.Color1 = pointColors[i % pointColors.Count];

                for (;i < last; i++)
                {
                    p2 = points[next];
                    _lineStyle.Color2 = pointColors[next % pointColors.Count];
                    DrawLine(p1, p2, ref _lineStyle);

                    _lineStyle.Color1 = _lineStyle.Color2;
                    p1 = p2;

                    next++;
                }
            }
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="startIndex">The start index within the points list from which to start drawing.</param>
        /// <param name="count">The number of points from the point list to draw.</param>
        /// <param name="style">The <see cref="LineStyle"/> to apply to the path.</param>
        public void DrawLinePath(IList<Vector2F> points, ref LineStyle style, uint surfaceSlice = 0)
        {
            DrawLinePath(points, 0, points.Count, ref style, surfaceSlice);
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="startIndex">The start index within the points list from which to start drawing.</param>
        /// <param name="count">The number of points from the point list to draw.</param>
        /// <param name="style">The <see cref="LineStyle"/> to apply to the path.</param>
        public void DrawLinePath(IList<Vector2F> points, int count, ref LineStyle style, uint surfaceSlice = 0)
        {
            DrawLinePath(points, 0, count, ref style, surfaceSlice);
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="startIndex">The start index within the points list from which to start drawing.</param>
        /// <param name="count">The number of points from the point list to draw.</param>
        /// <param name="style">The <see cref="LineStyle"/> to apply to the path.</param>
        public void DrawLinePath(IList<Vector2F> points, int startIndex, int count, ref LineStyle style, uint surfaceSlice = 0)
        {
            if (startIndex + count > points.Count)
                throw new SpriteBatcherException(this, "The sum of the start index and the count must be less than the point count.");

            if (count < 2)
                throw new SpriteBatcherException(this, "There must be at least 2 points in the point list.");

            if (count == 2)
            {
                DrawLine(points[0], points[1], ref style, surfaceSlice);
            }
            else
            {
                Vector2F p1, p2;
                int last = startIndex + count - 1;
                int next = startIndex + 1;

                int i = startIndex;
                p1 = points[i];
                _lineStyle = style;

                for (; i < last; i++)
                {
                    p2 = points[next];
                    DrawLine(p1, p2, ref _lineStyle);

                    if(i % 2 > 0)
                    {
                        _lineStyle.Color1 = style.Color1;
                        _lineStyle.Color2 = style.Color2;
                    }
                    else
                    {
                        _lineStyle.Color1 = style.Color2;
                        _lineStyle.Color2 = style.Color1;
                    }

                    p1 = p2;
                    next++;
                }
            }
        }

        /// <summary>
        /// Draws a line between two points with a color gradient produced with the two provided colors.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        /// <param name="sharpness">The edge-sharpness of the line.</param>
        public void DrawLine(Vector2F p1, Vector2F p2, Color color, float thickness, float sharpness = 1.0f, uint surfaceSlice = 0)
        {
            DrawLine(p1, p2, color, color, thickness, sharpness, surfaceSlice);
        }
        /// <summary>
        /// Draws a line between two points with a color gradient produced with the two provided colors.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="color1">The color for <paramref name="p1"/>.</param>
        /// <param name="color2">The color for <paramref name="p2"/>.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        /// <param name="sharpness">The edge-sharpness of the line.</param>
        public void DrawLine(Vector2F p1, Vector2F p2, Color color1, Color color2, float thickness, float sharpness = 1.0f, uint surfaceSlice = 0)
        {
            _lineStyle.Color1 = color1;
            _lineStyle.Color2 = color2;
            _lineStyle.Thickness = thickness;
            _lineStyle.Sharpness = sharpness;
            DrawLine(p1, p2, ref _lineStyle, surfaceSlice);
        }

        /// <summary>
        /// Draws a line between two points using the <see cref="SpriteStyle"/> of the current <see cref="SpriteBatcher"/>.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        public void DrawLine(Vector2F p1, Vector2F p2, ref LineStyle style, uint surfaceSlice = 0)
        {
            ref GpuData data = ref GetData(RangeType.Line, null, null);
            float dist = Vector2F.Distance(ref p1, ref p2) + 1;
            Vector2F dir = Vector2F.Normalize(p2 - p1);

            Vector2F size = new Vector2F(dist, style.Thickness);
            Vector2F pos = (p2 + p1) / 2; // The center of the line will be the mean position of both points.

            data.Position = pos;
            data.Rotation = MathF.Atan2(dir.Y, dir.X);
            data.Array.SrcArraySlice = 0;
            data.Array.DestSurfaceSlice = surfaceSlice;
            data.Size = size;
            data.UV = Vector4F.Zero;
            data.Color1 = style.Color1;
            data.Color2 = style.Color2;
            data.Extra.D1 = style.Thickness / size.Y; // Convert to UV coordinate system (0 - 1) range
            data.Extra.D2 = style.Sharpness;
            data.Origin = DEFAULT_ORIGIN_CENTER;
        }
    }
}
