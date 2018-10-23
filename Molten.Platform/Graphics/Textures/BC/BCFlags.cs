// Converted to C# by James Yarwood.
// MIT License.

//-------------------------------------------------------------------------------------
// BC.h
//  
// Block-compression (BC) functionality
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// http://go.microsoft.com/fwlink/?LinkId=248926
//-------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    [Flags]
    enum BCFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        NONE = 0x0,

        /// <summary>
        /// Enables dithering for RGB colors for BC1-3
        /// </summary>
        DITHER_RGB = 0x10000,

        /// <summary>
        /// Enables dithering for Alpha channel for BC1-3.
        /// </summary>
        DITHER_A = 0x20000,

        /// <summary>
        /// By default, uses perceptual weighting for BC1-3; this flag makes it a uniform weighting.
        /// </summary>
        UNIFORM = 0x40000,

        /// <summary>
        /// By default, BC7 skips mode 0 & 2; this flag adds those modes back.
        /// </summary>
        USE_3SUBSETS = 0x80000, 

        /// <summary>
        /// BC7 should only use mode 6; skip other modes.
        /// </summary>
        FORCE_BC7_MODE6 = 0x100000,
    };
}
