using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        /// <summary>Draws a triangle using 3 provided points.</summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="color">The color of the triangle.</param>
        public void DrawTriangle(Vector2F p1, Vector2F p2, Vector2F p3, Color color)
        {
            uint id = GetItemID();
            ref SpriteItem item = ref Sprites[id];
            item.Texture = null;
            item.Material = null;
            item.Format = SpriteFormat.Triangle;

            ref SpriteGpuData data = ref Data[id];
            data.Position = p1;
            data.Rotation = 0; // TODO triangle rotation.
            data.ArraySlice = 0;
            data.Size = p2;
            data.UV = Vector4F.Zero; // Unused
            data.Color = color;
            data.Origin = p3;
        }

        /// <summary>
        /// Draws a polygon from a list of points. The point list is expected to be in triangle-list format. 
        /// This means every 3 points should form a triangle. The polygon should be made up of several triangles.
        /// </summary>
        /// <param name="points">A list of points that form the polygon. A minimum of 3 points is expected.</param>
        /// <param name="triColors">A list of colors. One color per triangle. A minimum of 1 color is expected. 
        /// Insufficient colors for the provided triangles will cause the colors to be repeated.</param>
        public void DrawTriangleList(IList<Vector2F> points, IList<Color> triColors)
        {
            if (points.Count % 3 > 0)
                throw new SpriteBatcherException(this, "Incorrect number of points for triangle list. There should be 3 points per triangle");

            if (triColors.Count == 0)
                throw new SpriteBatcherException(this, "There must be at least one color available in the triColors list.");

            for (int i = 0; i < points.Count; i += 3)
            {
                int colID = i / 3;

                uint id = GetItemID();
                ref SpriteItem item = ref Sprites[id];
                item.Texture = null;
                item.Material = null;
                item.Format = SpriteFormat.Triangle;

                ref SpriteGpuData data = ref Data[id];
                data.Position = points[i];
                data.Rotation = 0;
                data.ArraySlice = 0;
                data.Size = points[i + 1];
                data.UV = Vector4F.Zero; // Unused
                data.Color = triColors[colID % triColors.Count];
                data.Origin = points[i + 2];
            }
        }

        /// <summary>
        /// Draws a polygon from a list of points. The point list is expected to be in triangle-list format. 
        /// This means every 3 points should form a triangle. The polygon should be made up of several triangles.
        /// </summary>
        /// <param name="points">A list of points that form the polygon.</param>
        /// <param name="color">The color of the polygon.</param>
        public void DrawTriangleList(IList<Vector2F> points, Color color)
        {
            _singleColorList[0] = color;
            DrawTriangleList(points, _singleColorList);
        }
    }
}
