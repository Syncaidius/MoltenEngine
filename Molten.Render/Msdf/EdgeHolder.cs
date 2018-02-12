//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017, WinterDev (C# port)
//MIT, 2017 Applied Ckohnert's changes from https://github.com/ckohnert/msdfgen
//MIT, 2018, James Yarwood (Adapted for Molten Engine)
namespace Msdfgen
{
    //#include "EdgeHolder.h"

    public class EdgeHolder
    {
        public EdgeSegment edgeSegment;
        public EdgeHolder(EdgeSegment segment)
        {
            this.edgeSegment = segment;
        }
        public EdgeColor color
        {
            get { return edgeSegment.color; }
            set
            {
                edgeSegment.color = value;
            }
        }

        public bool HasComponent(EdgeColor c)
        {
            return (color & c) != 0;
        }
        public Vector2 Direction(double param)
        {
            if (edgeSegment != null)
            {
                return edgeSegment.direction(param);
            }
            else
            {
                return default(Vector2);
            }
        }
        public Vector2 point(double param)
        {
            return edgeSegment.point(param);
        }

        public int crossings(Vector2 r, WindingSpanner cb)
        {
            return edgeSegment.crossings(r, cb);
        }

#if DEBUG
        public override string ToString()
        {
            return edgeSegment.ToString();
        }
#endif
    }
}