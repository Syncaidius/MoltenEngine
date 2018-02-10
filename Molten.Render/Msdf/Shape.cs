//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017, WinterDev (C# port)
//MIT, 2018, James Yarwood (Adapted for Molten Engine)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msdfgen
{
    public class Shape
    {
        public List<Contour> contours = new List<Contour>();
        public bool InverseYAxis { get; set; }
        public void normalized()
        {
            int j = contours.Count;
            for (int i = 0; i < j; ++i)
            {
                Contour contour = contours[i];
                List<EdgeHolder> edges = contour.Edges;
                if (edges.Count == 1)
                {
                    //TODO:
                    EdgeSegment e0, e1, e2;
                    edges[0].edgeSegment.splitInThirds(out e0, out e1, out e2);
                    edges.Clear();
                    edges.Add(new EdgeHolder(e0));
                    edges.Add(new EdgeHolder(e1));
                    edges.Add(new EdgeHolder(e2));

                }
            }
        }

        public void FindBounds(out double left, out double bottom, out double right, out double top)
        {
            left = top = right = bottom = 0;
            int j = contours.Count;
            for (int i = 0; i < j; ++i)
            {
                contours[i].FindBounds(ref left, ref bottom, ref right, ref top);
            }
        }
    }
}
