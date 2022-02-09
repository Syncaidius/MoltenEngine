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

using System.IO;

namespace Molten.Graphics.Textures
{
    internal class D3DX_BC1
    {
        public ushort[] rgb = new ushort[2]; // 565 colors
        public uint bitmap; // 2bpp rgb bitmap

        internal void Read(BinaryReader reader)
        {
            rgb[0] = reader.ReadUInt16();
            rgb[1] = reader.ReadUInt16();
            bitmap = reader.ReadUInt32();
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(rgb[0]);
            writer.Write(rgb[1]);
            writer.Write(bitmap);
        }
    };
}
