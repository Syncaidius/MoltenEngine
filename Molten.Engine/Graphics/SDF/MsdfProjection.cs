using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SDF
{
    public class MsdfProjection
    {
        public Vector2D Scale;
        public Vector2D Translate;

        public static readonly MsdfProjection Default = new MsdfProjection(new Vector2D(1), new Vector2D(0));

        public MsdfProjection()
        {
            Scale = new Vector2D(1);
            Translate = new Vector2D();
        }

        public MsdfProjection(Vector2D scale, Vector2D translate)
        {
            Scale = scale;
            Translate = translate;
        }

        public Vector2D Project(Vector2D coord)
        {
            return Scale * (coord + Translate);
        }

        public Vector2D Unproject(Vector2D coord)
        {
            return coord / Scale - Translate;
        }

        public Vector2D ProjectVector(Vector2D vector)
        {
            return Scale * vector;
        }

        public Vector2D UnprojectVector(Vector2D vector)
        {
            return vector / Scale;
        }

        public double ProjectX(double x)
        {
            return Scale.X * (x + Translate.X);
        }

        public double ProjectY(double y)
        {
            return Scale.Y * (y + Translate.Y);
        }

        public double UnprojectX(double x)
        {
            return x / Scale.X - Translate.X;
        }

        public double UnprojectY(double y)
        {
            return y / Scale.Y - Translate.Y;
        }
    }
}
