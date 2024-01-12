namespace Molten.Graphics;

/// <summary>
/// Represents shader model tiers.
/// </summary>
public enum ShaderModel : uint
{
    /// <summary>Shader model 1.1.</summary>
    Model1_1 = 0,

    /// <summary>Shader model 2.0</summary>
    Model2_0 = 1,

    /// <summary>Shader model 2.0a</summary>
    Model2_0a = 2,

    /// <summary>Shader model 2.0b</summary>
    Model2_0b = 3,

    /// <summary>Shader model 3.0</summary>
    Model3_0 = 4,

    /// <summary>Shader model 4.0</summary>
    Model4_0 = 5,

    /// <summary>Shader model 4.1</summary>
    Model4_1 = 6,

    /// <summary>Shader model 5.0</summary>
    Model5_0 = 7,

    /// <summary>
    /// Shader model 5.1
    /// </summary>
    Model5_1 = 8,

    /// <summary>
    /// Shader model 6.0. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.0
    /// </summary>
    Model6_0 = 9,

    /// <summary>
    /// Shader model 6.1. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.1
    /// </summary>
    Model6_1 = 10,

    /// <summary>
    /// Shader model 6.2. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.2
    /// </summary>
    Model6_2 = 11,

    /// <summary>
    /// Shader model 6.3. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.3
    /// </summary>
    Model6_3 = 12,

    /// <summary>
    /// Shader model 6.4. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.4
    /// </summary>
    Model6_4 = 13,

    /// <summary>
    /// Shader model 6.5. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.5
    /// </summary>
    Model6_5 = 14,

    /// <summary>
    /// Shader model 6.6. Introduces:
    /// <list type="bullet">
    /// <item>New atomic ops: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_6_Int64_and_Float_Atomics.html</item>
    /// <item>Dynamic resources: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_6_DynamicResources.html</item>
    /// <item>IsHelperLane(): https://microsoft.github.io/DirectX-Specs/d3d/HLSL_ShaderModel6_6.html</item>
    /// <item>Derivative Ops: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_6_Derivatives.html</item>
    /// <item>Pack and Unpack: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_6_Pack_Unpack_Intrinsics.html</item>
    /// <item>Wave size: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_6_WaveSize.html</item>
    /// <item>Raytracing payload access qualifiers: https://microsoft.github.io/DirectX-Specs/d3d/Raytracing.html#payload-access-qualifiers</item>
    /// </list>
    /// <para>See: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_ShaderModel6_6.html</para>
    /// <para>see: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.6</para>
    /// </summary>
    Model6_6 = 15,

    /// <summary>
    /// Shader model 6.7. Introduces:
    /// <list type="bullet">
    /// <item>Raw Gather: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_7_Advanced_Texture_Ops.html#raw-gather</item>
    /// <item>SampleCmpLevel: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_7_Advanced_Texture_Ops.html#samplecmplevel</item>
    /// <item>Programmable offsets: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_7_Advanced_Texture_Ops.html#programmable-offsets</item>
    /// <item>QuadAny and QuadAll: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_7_QuadAny_QuadAll.html</item>
    /// <item>Wave Ops helper lanes: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_7_Wave_Ops_Include_Helper_Lanes.html</item>
    /// </list>
    /// <para>See: https://devblogs.microsoft.com/directx/in-the-works-hlsl-shader-model-6-7//</para>
    /// <para>See: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_ShaderModel6_7.html</para>
    /// </summary>
    Model6_7 = 16,

    /// <summary>
    /// Sahder model 6.8. Introduces work graphs and wave matrices.
    /// <para>See: https://microsoft.github.io/DirectX-Specs/d3d/HLSL_ShaderModel6_8.html</para>
    /// <para>See: https://github.com/microsoft/DirectXShaderCompiler/milestone/1</para>
    /// </summary>
    Model6_8 = 17,
}
