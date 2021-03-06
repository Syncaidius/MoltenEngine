﻿using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class Texture1DProperties
    {

        public int MipMapLevels = 1;

        public int Width = 1;

        public GraphicsFormat Format = GraphicsFormat.R8G8B8A8_UNorm;

        public int ArraySize = 1;

        public TextureFlags Flags = TextureFlags.None;
    }
}
