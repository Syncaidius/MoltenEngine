using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ISpriteBatch
    {
        void DrawString(ISpriteFont font, string text, Vector2 position, Color color, IMaterial material = null);

        /// <summary>
        /// Draws a circle with the specified radius.
        /// </summary>
        /// <param name="center">The position of the circle center.</param>
        /// <param name="radius">The radius, in radians.</param>
        /// <param name="startAngle">The start angle of the circle, in radians. This is useful when drawing a partial-circle.</param>
        /// <param name="endAngle">The end angle of the circle, in radians. This is useful when drawing a partial-circle</param>
        /// <param name="col">The color of the circle.</param>
        /// <param name="segments">The number of segments that will make up the circle. A higher value will produce a smoother edge. The minimum value is 5.</param>
        void DrawCircle(Vector2 center, float radius, float startAngle, float endAngle, Color col, int segments = 16);

        /// <summary>
        /// Draws a circle with the specified radius.
        /// </summary>
        /// <param name="center">The position of the circle center.</param>
        /// <param name="xRadius">The radius, in radians.</param>
        /// <param name="col">The color of the circle.</param>
        /// <param name="segments">The number of segments that will make up the circle. A higher value will produce a smoother edge. The minimum value is 5.</param>
        void DrawCircle(Vector2 center, float radius, Color col, int segments = 16);

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="center">The position of the ellipse center.</param>
        /// <param name="xRadius">The X radius, in radians.</param>
        /// <param name="yRadius">The Y radius, in radians.</param>
        /// <param name="col">The color of the ellipse.</param>
        /// <param name="segments">The number of segments that will make up the ellipse. A higher value will produce a smoother edge. The minimum value is 5.</param>
        void DrawEllipse(Vector2 center, float xRadius, float yRadius, Color col, int segments = 16);

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="center">The position of the ellipse center.</param>
        /// <param name="xRadius">The X radius, in radians.</param>
        /// <param name="yRadius">The Y radius, in radians.</param>
        /// <param name="startAngle">The start angle of the circle, in radians. This is useful when drawing a partial-ellipse.</param>
        /// <param name="endAngle">The end angle of the circle, in radians. This is useful when drawing a partial-ellipse</param>
        /// <param name="col">The color of the ellipse.</param>
        /// <param name="segments">The number of segments that will make up the ellipse. A higher value will produce a smoother edge. The minimum value is 5.</param>
        void DrawEllipse(Vector2 center, float xRadius, float yRadius, float startAngle, float endAngle, Color col, int segments = 16);

        /// <summary>Draws a triangle using 3 provided points.</summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="col1">The color of the triangle.</param>
        void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color col);

        /// <summary>Draws a triangle using 3 provided points.</summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="col1">The color for the first point.</param>
        /// <param name="col2">The color for the second point.</param>
        /// <param name="col3">The color for the thrid point.</param>
        void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color col1, Color col2, Color col3);

        /// <summary>
        /// Draws a polygon from a list of points. The point list is expected to be in triangle-list format. 
        /// This means every 3 points should form a triangle. The polygon should be made up of several triangles.
        /// </summary>
        /// <param name="points">A list of points that form the polygon. A minimum of 3 points is expected.</param>
        /// <param name="col">A list of colors. One color per point. A minimum of 3 colors is expected.</param>
        void DrawPolygon(IList<Vector2> points, IList<Color> colors);

        /// <summary>
        /// Draws a polygon from a list of points. The point list is expected to be in triangle-list format. 
        /// This means every 3 points should form a triangle. The polygon should be made up of several triangles.
        /// </summary>
        /// <param name="points">A list of points that form the polygon.</param>
        /// <param name="color">The color of the polygon.</param>
        void DrawPolygon(IList<Vector2> points, Color color);

        /// <summary>Draws lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="pointColors">A list of colors (one per point) that lines should transition to/from at each point.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        void DrawLines(IList<Vector2> points, IList<Color> pointColors, float thickness);

        /// <summary>Draws lines between each of the provided points.</summary>
        /// <param name="points">The points between which to draw lines.</param>
        /// <param name="color">The color of the lines</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        void DrawLines(IList<Vector2> points, Color color, float thickness);

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        void DrawLine(Vector2 p1, Vector2 p2, Color color, float thickness);

        /// <summary>
        /// Draws a line between two points with a color gradient produced with the two provided colors.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="color1">The color for pos1.</param>
        /// <param name="color2">The color for pos2.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        void DrawLine(Vector2 p1, Vector2 p2, Color color1, Color color2, float thickness);

        void DrawRect(Rectangle destination, Color color, IMaterial material = null);

        void DrawRect(Rectangle destination, Color color, float rotation, Vector2 origin, IMaterial material = null);

        void Draw(ITexture2D texture, Rectangle destination, Color color, IMaterial material = null);

        void Draw(ITexture2D texture, Rectangle destination, Color color, float rotation, Vector2 origin, IMaterial material = null);

        void Draw(ITexture2D texture, Vector2 position, Color color, IMaterial material = null);

        void Draw(ITexture2D texture, Vector2 position, Color color, float rotation, Vector2 origin, IMaterial material = null);

        void Draw(ITexture2D texture, Vector2 position, Rectangle source, Color color, float rotation, Vector2 scale, Vector2 origin, IMaterial material = null);

        void Draw(ITexture2D texture, Rectangle source, Rectangle destination, Color color, float rotation, Vector2 origin, IMaterial material);

        /// <summary>
        /// Adds the contents of the specified <see cref="SpriteBatchCache"/> to the current <see cref="ISpriteBatch"/>.
        /// </summary>
        /// <param name="cache">The cache.</param>
        void Draw(SpriteBatchCache cache);

        void PushClip(Rectangle clipBounds);

        void PopClip();

        /// <summary>Gets the current clip depth. This increases and decreases with calls to <see cref="PushClip(Rectangle)"/> and <see cref="PopClip"/></summary>
        int ClipDepth { get; }
    }
}
