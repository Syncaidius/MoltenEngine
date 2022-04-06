﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    internal class MSDFErrorCorrection
    {
        BitmapRef<byte> stencil;
        MsdfProjection projection;
        double invRange;
        double minDeviationRatio;
        double minImproveRatio;


        /// <summary>
        /// Stencil flags
        /// </summary>
        public enum Flags
        {
            /// Texel marked as potentially causing interpolation errors.
            ERROR = 1,
            /// Texel marked as protected. Protected texels are only given the error flag if they cause inversion artifacts.
            PROTECTED = 2
        };
    }
}
