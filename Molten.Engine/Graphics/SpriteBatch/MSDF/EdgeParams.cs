﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public struct EdgeParams
    {
        public SignedDistance minDistance;
        public EdgeHolder nearEdge;
        public double nearParam;
    }
}
