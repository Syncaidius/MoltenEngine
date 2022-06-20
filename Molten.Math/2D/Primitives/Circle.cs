using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public struct Circle
    {
        public Vector2F Center;
        public float Radius;
        public float StartAngle;
        public float EndAngle;

        public Circle()
        {
            Center = Vector2F.Zero;
            Radius = 1;
            StartAngle = 0;
            EndAngle = MathHelper.TwoPi;
        }


        public Circle(Vector2F center, float radius, float startAngle, float endAngle)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        public Circle(Vector2F center, float radius)
        {
            Center = center;
            Radius = radius;
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
        /// Gets the area of the current<see cref="Circle"/>.
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            return MathHelper.Pi * (Radius * Radius);
        }
    }
}
