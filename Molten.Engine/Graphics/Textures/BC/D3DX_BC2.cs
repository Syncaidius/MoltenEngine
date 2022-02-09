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
    internal class D3DX_BC2
    {
        public uint[] bitmap = new uint[2];         // 4bpp alpha bitmap
        public D3DX_BC1 bc1 = new D3DX_BC1();        // BC1 rgb data

        internal void Read(BinaryReader reader)
        {
            bitmap[0] = reader.ReadUInt32();
            bitmap[1] = reader.ReadUInt32();
            bc1.Read(reader);
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(bitmap[0]);
            writer.Write(bitmap[1]);
            bc1.Write(writer);
        }
    };

}
