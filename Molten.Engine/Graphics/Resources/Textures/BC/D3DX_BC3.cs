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

namespace Molten.Graphics.Textures
{
    internal class D3DX_BC3
    {
        public byte[] alpha = new byte[2];          // Alpha values.
        public byte[] bitmap = new byte[6];         // 3bpp alpha bitmap
        public D3DX_BC1 bc1 = new D3DX_BC1();       // BC1 rgb data

        internal void Read(BinaryReader reader)
        {
            alpha[0] = reader.ReadByte();
            alpha[1] = reader.ReadByte();
            for (int i = 0; i < bitmap.Length; i++)
                bitmap[i] = reader.ReadByte();

            bc1.Read(reader);
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(alpha[0]);
            writer.Write(alpha[1]);
            writer.Write(bitmap, 0, bitmap.Length);
            bc1.Write(writer);
        }
    };

}
