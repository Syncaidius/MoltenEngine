﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class GraphicsBufferException : Exception
    {
        public GraphicsBufferException(string message) : base(message) { }
    }
}
