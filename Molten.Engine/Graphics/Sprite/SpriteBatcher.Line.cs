using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        Color[] _singleColorList = new Color[] { Color.White };

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="color">The color of the lines</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        public void DrawLinePath(IList<Vector2F> points, Color color, float thickness)
        {
            _singleColorList[0] = color;
            DrawLinePath(points, 0, points.Count, _singleColorList, thickness);
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="pointColors">A list of colors (one per point) that lines should transition to/from at each point.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        public void DrawLinePath(IList<Vector2F> points, IList<Color> pointColors, float thickness)
        {
            DrawLinePath(points, 0, points.Count, pointColors, thickness);
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="Color">The color of all the lines in the path.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        /// <param name="startIndex">The start index within the points list from which to start drawing.</param>
        /// <param name="count">The number of points from the point list to draw.</param>
        public void DrawLinePath(IList<Vector2F> points, int startIndex, int count, Color color, float thickness)
        {
            _singleColorList[0] = color;
            DrawLinePath(points, startIndex, count, _singleColorList, thickness);
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="pointColors">A list of colors (one per point) that lines should transition to/from at each point.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        /// <param name="startIndex">The start index within the points list from which to start drawing.</param>
        /// <param name="count">The number of points from the point list to draw.</param>
        public void DrawLinePath(IList<Vector2F> points, int startIndex, int count, IList<Color> pointColors, float thickness)
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
                DrawLine(points[0], points[1], pointColors[0], pointColors[secondCol], thickness);
            }
            else
            {
                int lineCount = points.Count - 1;

                Vector2F p1, p2;
                int last = startIndex + count - 1;
                int prev = 0;
                int next = 1;

                Vector2F vThickness = new Vector2F(thickness);

                for (int i = startIndex; i < last; i++)
                {
                    p1 = points[i];
                    p2 = points[next];

                    ref SpriteItem item = ref GetItem();
                    item.Texture = null;
                    item.Material = null;
                    item.Format = SpriteFormat.Line;

                    float dist = Vector2F.Distance(ref p1, ref p2);
                    Vector2F dir = Vector2F.Normalize(p2 - p1);

                    Vector2F size = new Vector2F(dist, thickness);
                    Vector2F pos = (p2 + p1) / 2; // The center of the line will be the mean position of both points.

                    item.Vertex.Position = pos;
                    item.Vertex.Rotation = (float)Math.Atan2(dir.Y, dir.X);
                    item.Vertex.ArraySlice = 0;
                    item.Vertex.Size = size;
                    item.Vertex.UV = Vector4F.Zero;
                    item.Vertex.Color = pointColors[i % pointColors.Count];
                    item.Vertex.Color2 = pointColors[(i + 1) % pointColors.Count];
                    item.Vertex.Data.Thickness = vThickness / size; // Convert to UV coordinate system (0 - 1) range
                    item.Vertex.Origin = DEFAULT_ORIGIN_CENTER;
                    next++;
                    prev = i;
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
        public void DrawLine(Vector2F p1, Vector2F p2, Color color, float thickness)
        {
            DrawLine(p1, p2, color, color, thickness);
        }
        /// <summary>
        /// Draws a line between two points with a color gradient produced with the two provided colors.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="color1">The color for <paramref name="p1"/>.</param>
        /// <param name="color2">The color for <paramref name="p2"/>.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        public void DrawLine(Vector2F p1, Vector2F p2, Color color1, Color color2, float thickness)
        {
            ref SpriteItem item = ref GetItem();
            item.Texture = null;
            item.Material = null;
            item.Format = SpriteFormat.Line;

            float dist = Vector2F.Distance(ref p1, ref p2);
            Vector2F dir = Vector2F.Normalize(p2 - p1);

            Vector2F size = new Vector2F(dist, thickness);
            Vector2F pos = (p2 + p1) / 2; // The center of the line will be the mean position of both points.

            item.Vertex.Position = pos;
            item.Vertex.Rotation = (float)Math.Atan2(dir.Y, dir.X);
            item.Vertex.ArraySlice = 0;
            item.Vertex.Size = size;
            item.Vertex.UV = Vector4F.Zero;
            item.Vertex.Color = color1;
            item.Vertex.Color2 = color2;
            item.Vertex.Data.Thickness = new Vector2F(thickness) / size; // Convert to UV coordinate system (0 - 1) range
            item.Vertex.Origin = DEFAULT_ORIGIN_CENTER;
        }

        /// <summary>
        /// Draws a line between two points using the <see cref="SpriteStyle"/> of the current <see cref="SpriteBatcher"/>.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        public void DrawLine(Vector2F p1, Vector2F p2)
        {
            ref SpriteItem item = ref GetItem();
            item.Texture = null;
            item.Material = null;
            item.Format = SpriteFormat.Line;

            float dist = Vector2F.Distance(ref p1, ref p2) + 1;
            Vector2F dir = Vector2F.Normalize(p2 - p1);

            Vector2F size = new Vector2F(dist, _style.Thickness);
            Vector2F pos = (p2 + p1) / 2; // The center of the line will be the mean position of both points.

            item.Vertex.Position = pos;
            item.Vertex.Rotation = (float)Math.Atan2(dir.Y, dir.X);
            item.Vertex.ArraySlice = 0;
            item.Vertex.Size = size;
            item.Vertex.UV = Vector4F.Zero;
            item.Vertex.Color2 = _style.SecondaryColor;
            item.Vertex.Color = _style.PrimaryColor;
            item.Vertex.Data.Thickness = new Vector2F(_style.Thickness) / size; // Convert to UV coordinate system (0 - 1) range
            item.Vertex.Origin = DEFAULT_ORIGIN_CENTER;
        }
    }
}
