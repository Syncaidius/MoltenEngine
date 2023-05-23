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

        TextureCube = 9,

        Sampler = 10,

        Sampler1D = 11,

        Sampler2D = 12,

        Sampler3D = 13,

        SamplerCube = 14,

        PixelShader = 0xF,

        VertexShader = 0x10,

        PixelFragment = 17,

        VertexFragment = 18,

        UInt = 19,

        UInt8 = 20,

        GeometryShader = 21,

        Rasterizer = 22,

        DepthStencil = 23,

        Blend = 24,

        Buffer = 25,

        ConstantBuffer = 26,

        TextureBuffer = 27,

        Texture1DArray = 28,

        Texture2DArray = 29,

        RenderTargetView = 30,

        DepthStencilView = 0x1F,

        Texture2DMS = 0x20,

        Texture2DMSArray = 33,

        TextureCubeArray = 34,

        HullShader = 35,

        DomainShader = 36,

        InterfacePointer = 37,

        ComputeShader = 38,

        Double = 39,

        RWTexture1D = 40,

        RWTexture1DArray = 41,

        RWTexture2D = 42,

        RWTexture2DArray = 43,

        RWTexture3D = 44,

        RWBuffer = 45,

        ByteAddressBuffer = 46,

        RWByteAddressBuffer = 47,

        StructuredBuffer = 48,

        RWStructuredBuffer = 49,

        AppendStructuredBuffer = 50,

        ConsumeStructuredBuffer = 51,

        Min8Float = 52,

        Min10Float = 53,

        Min16Float = 54,

        Min12Int = 55,

        Min16Int = 56,

        Min16UInt = 57,

        Int16 = 58,

        UInt16 = 59,

        Float16 = 60,

        Int64 = 61,

        UInt64 = 62,

        ForceDWord = 0x7fffffff,
    }
}
