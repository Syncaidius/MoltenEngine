using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;

namespace Molten.Graphics
{
    /// <summary>
    /// Based on the D3D shader variable type enum: D3D_SHADER_VARIABLE_TYPE .
    /// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_shader_variable_type</param>
    /// </summary>
    public enum ShaderVariableType
    {
        Void = 0,

        Bool = 1,

        Int = 2,

        Float = 3,

        String = 4,

        Texture = 5,

        Texture1D = 6,

        Texture2D = 7,

        Texture3D = 8,

        Texturecube = 9,

        Sampler = 10,

        Sampler1D = 11,

        Sampler2D = 12,

        Sampler3D = 13,

        Samplercube = 14,

        Pixelshader = 0xF,

        Vertexshader = 0x10,

        Pixelfragment = 17,

        Vertexfragment = 18,

        Uint = 19,

        Uint8 = 20,

        Geometryshader = 21,

        Rasterizer = 22,

        Depthstencil = 23,

        Blend = 24,

        Buffer = 25,

        Cbuffer = 26,

        Tbuffer = 27,

        Texture1Darray = 28,

        Texture2Darray = 29,

        Rendertargetview = 30,

        Depthstencilview = 0x1F,

        Texture2Dms = 0x20,

        Texture2Dmsarray = 33,

        Texturecubearray = 34,

        Hullshader = 35,

        Domainshader = 36,

        InterfacePointer = 37,

        Computeshader = 38,

        Double = 39,

        Rwtexture1D = 40,

        Rwtexture1Darray = 41,

        Rwtexture2D = 42,

        Rwtexture2Darray = 43,

        Rwtexture3D = 44,

        Rwbuffer = 45,

        ByteaddressBuffer = 46,

        RwbyteaddressBuffer = 47,

        StructuredBuffer = 48,

        RwstructuredBuffer = 49,

        AppendStructuredBuffer = 50,

        ConsumeStructuredBuffer = 51,

        Min8float = 52,

        Min10float = 53,

        Min16float = 54,

        Min12int = 55,

        Min16int = 56,

        Min16Uint = 57,

        Int16 = 58,

        Uint16 = 59,

        Float16 = 60,

        Int64 = 61,

        Uint64 = 62,

        ForceDword = int.MaxValue
    }
}
