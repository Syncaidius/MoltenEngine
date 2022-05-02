using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SDF
{
    public struct EdgeParams
    {
        public SignedDistance minDistance;
        public Shape.Edge nearEdge;
        public double nearParam;
    }
}
