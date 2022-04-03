using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public class MsdfProjection
    {
        Vector2D _scale;
        Vector2D _translate;

        public MsdfProjection()
        {
            _scale = new Vector2D(1);
            _translate = new Vector2D();
        }

        public MsdfProjection(Vector2D scale, Vector2D translate)
        {
            _scale = scale;
            _translate = translate;
        }

        public Vector2D Project(Vector2D coord)
        {
            return _scale * (coord + _translate);
        }

        public Vector2D Unproject(Vector2D coord)
        {
            return coord / _scale - _translate;
        }

        public Vector2D ProjectVector(Vector2D vector)
        {
            return _scale * vector;
        }

        public Vector2D UnprojectVector(Vector2D vector)
        {
            return vector / _scale;
        }

        public double ProjectX(double x)
        {
            return _scale.X * (x + _translate.X);
        }

        public double ProjectY(double y)
        {
            return _scale.Y * (y + _translate.Y);
        }

        public double UnprojectX(double x)
        {
            return x / _scale.X - _translate.X;
        }

        public double UnprojectY(double y)
        {
            return y / _scale.Y - _translate.Y;
        }
    }
}
