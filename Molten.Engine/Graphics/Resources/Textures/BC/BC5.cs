// Converted to C# by James Yarwood.
// MIT License.

//-------------------------------------------------------------------------------------
// BC4BC5.cpp
//  
// Block-compression (BC) functionality for BC4 and BC5 (DirectX 10 texture compression)
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// http://go.microsoft.com/fwlink/?LinkId=248926
//-------------------------------------------------------------------------------------

namespace Molten.Graphics.Textures;

internal class BC5_UNORM
{
    public BC4_UNORM Red = new BC4_UNORM();
    public BC4_UNORM Green = new BC4_UNORM();

    internal void Read(BinaryReader reader)
    {
        Red.Read(reader);
        Green.Read(reader);
    }

    internal void Write(BinaryWriter writer)
    {
        Red.Write(writer);
        Green.Write(writer);
    }
}

internal class BC5_SNORM
{
    public BC4_SNORM Red = new BC4_SNORM();
    public BC4_SNORM Green = new BC4_SNORM();

    internal void Read(BinaryReader reader)
    {
        Red.Read(reader);
        Green.Read(reader);
    }

    internal void Write(BinaryWriter writer)
    {
        Red.Write(writer);
        Green.Write(writer);
    }
}
