using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public struct Ellipse
    {
        public Vector2F Center;
        public float RadiusX;
        public float RadiusY;
        public float StartAngle;
        public float EndAngle;

        public Ellipse()
        {
            Center = Vector2F.Zero;
            RadiusX = 1;
            RadiusY = 1;
            StartAngle = 0;
            EndAngle = MathHelper.TwoPi;
        }

        public Ellipse(Vector2F center, float radX, float radY, float startAngle, float endAngle)
        {
            Center = center;
            RadiusX = radX;
            RadiusY = radY;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        public Ellipse(Vector2F center, float radX, float radY)
        {
            Center = center;
            RadiusX = radX;
            RadiusY = radY;
            StartAngle = 0;
            EndAngle = MathHelper.TwoPi;
        }

        /// <summary>
        /// Instantiates a circular ellipse using the same radius for X and Y axis.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public Ellipse(Vector2F center, float radius)
        {
            Center = center;
            RadiusX = radius;
            RadiusY = radius;
            StartAngle = 0;
            EndAngle = MathHelper.TwoPi;
        }

        /// <summary>
        /// Gets the angle represented between <see cref="EndAngle"/> and <see cref="StartAngle"/>.
        /// </summary>
        /// <returns>A float representing an angle.</returns>
        public float GetAngleRange()
        {
            return EndAngle - StartAngle;
        }

        /// <summary>
        /// Gets the area of the current<see cref="Ellipse"/>.
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            return MathHelper.Pi * (RadiusX * RadiusY);
        }

        /// <summary>
        /// Returns true if <see cref="RadiusX"/> is equal to <see cref="RadiusY"/>, forming a circular ellipse.
        /// </summary>
        /// <returns></returns>
        public bool IsCircle()
        {
            return RadiusX == RadiusY;
        }
    }
}
