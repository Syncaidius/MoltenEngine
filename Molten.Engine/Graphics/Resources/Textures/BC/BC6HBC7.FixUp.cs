// Converted to C# by James Yarwood.
// MIT License.

//-------------------------------------------------------------------------------------
// BC6HBC7.cpp
//  
// Block-compression (BC) functionality for BC6H and BC7 (DirectX 11 texture compression)
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// http://go.microsoft.com/fwlink/?LinkId=248926
//-------------------------------------------------------------------------------------

namespace Molten.Graphics.Textures;

internal static partial class BC67
{
    internal static readonly byte[][][] g_aFixUp = new byte[][][]
    {
        new byte[][]{   // No fix-ups for 1st subset for BC6H or BC7
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },
            new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 },new byte[]{ 0, 0, 0 }
        },

        new byte[][]{   // BC6H/BC7 Partition Set Fixups for 2 Subsets
            new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },
            new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },
            new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },
            new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },
            new byte[]{ 0,15, 0 },new byte[]{ 0, 2, 0 },new byte[]{ 0, 8, 0 },new byte[]{ 0, 2, 0 },
            new byte[]{ 0, 2, 0 },new byte[]{ 0, 8, 0 },new byte[]{ 0, 8, 0 },new byte[]{ 0,15, 0 },
            new byte[]{ 0, 2, 0 },new byte[]{ 0, 8, 0 },new byte[]{ 0, 2, 0 },new byte[]{ 0, 2, 0 },
            new byte[]{ 0, 8, 0 },new byte[]{ 0, 8, 0 },new byte[]{ 0, 2, 0 },new byte[]{ 0, 2, 0 },

            // BC7 Partition Set Fixups for 2 Subsets (second-half)
            new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0, 6, 0 },new byte[]{ 0, 8, 0 },
            new byte[]{ 0, 2, 0 },new byte[]{ 0, 8, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },
            new byte[]{ 0, 2, 0 },new byte[]{ 0, 8, 0 },new byte[]{ 0, 2, 0 },new byte[]{ 0, 2, 0 },
            new byte[]{ 0, 2, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0, 6, 0 },
            new byte[]{ 0, 6, 0 },new byte[]{ 0, 2, 0 },new byte[]{ 0, 6, 0 },new byte[]{ 0, 8, 0 },
            new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0, 2, 0 },new byte[]{ 0, 2, 0 },
            new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },new byte[]{ 0,15, 0 },
            new byte[]{ 0,15, 0 },new byte[]{ 0, 2, 0 },new byte[]{ 0, 2, 0 },new byte[]{ 0,15, 0 }
        },

        new byte[][]{   // BC7 Partition Set Fixups for 3 Subsets
            new byte[]{ 0, 3,15 },new byte[]{ 0, 3, 8 },new byte[]{ 0,15, 8 },new byte[]{ 0,15, 3 },
            new byte[]{ 0, 8,15 },new byte[]{ 0, 3,15 },new byte[]{ 0,15, 3 },new byte[]{ 0,15, 8 },
            new byte[]{ 0, 8,15 },new byte[]{ 0, 8,15 },new byte[]{ 0, 6,15 },new byte[]{ 0, 6,15 },
            new byte[]{ 0, 6,15 },new byte[]{ 0, 5,15 },new byte[]{ 0, 3,15 },new byte[]{ 0, 3, 8 },
            new byte[]{ 0, 3,15 },new byte[]{ 0, 3, 8 },new byte[]{ 0, 8,15 },new byte[]{ 0,15, 3 },
            new byte[]{ 0, 3,15 },new byte[]{ 0, 3, 8 },new byte[]{ 0, 6,15 },new byte[]{ 0,10, 8 },
            new byte[]{ 0, 5, 3 },new byte[]{ 0, 8,15 },new byte[]{ 0, 8, 6 },new byte[]{ 0, 6,10 },
            new byte[]{ 0, 8,15 },new byte[]{ 0, 5,15 },new byte[]{ 0,15,10 },new byte[]{ 0,15, 8 },
            new byte[]{ 0, 8,15 },new byte[]{ 0,15, 3 },new byte[]{ 0, 3,15 },new byte[]{ 0, 5,10 },
            new byte[]{ 0, 6,10 },new byte[]{ 0,10, 8 },new byte[]{ 0, 8, 9 },new byte[]{ 0,15,10 },
            new byte[]{ 0,15, 6 },new byte[]{ 0, 3,15 },new byte[]{ 0,15, 8 },new byte[]{ 0, 5,15 },
            new byte[]{ 0,15, 3 },new byte[]{ 0,15, 6 },new byte[]{ 0,15, 6 },new byte[]{ 0,15, 8 },
            new byte[]{ 0, 3,15 },new byte[]{ 0,15, 3 },new byte[]{ 0, 5,15 },new byte[]{ 0, 5,15 },
            new byte[]{ 0, 5,15 },new byte[]{ 0, 8,15 },new byte[]{ 0, 5,15 },new byte[]{ 0,10,15 },
            new byte[]{ 0, 5,15 },new byte[]{ 0,10,15 },new byte[]{ 0, 8,15 },new byte[]{ 0,13,15 },
            new byte[]{ 0,15, 3 },new byte[]{ 0,12,15 },new byte[]{ 0, 3,15 },new byte[]{ 0, 3, 8 }
        }
    };
}
