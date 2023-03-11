﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Has an identical layout to a D3D Box struct.
    /// </summary>
    public struct ResourceRegion
    {
        public uint Left;

        public uint Top;

        public uint Front;

        public uint Right;

        public uint Bottom;

        public uint Back;
    }
}