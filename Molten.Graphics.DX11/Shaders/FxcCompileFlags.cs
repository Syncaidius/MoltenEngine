namespace Molten.Graphics.DX11;

/// <summary>
/// FXC Shader compiler flags.
/// 
/// <para>
/// Reference: https://github.com/apitrace/dxsdk/blob/d964b66467aaa734edbc24326da8119f5f063dd3/Include/d3dcompiler.h#L138
/// </para>
/// <para>
/// Reference: https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/d3dcompile-constants
/// </para>
/// </summary>
/// <remarks>
/// 
/// </remarks>
internal enum FxcCompileFlags
{
    None = 0,
    Debug = 1 << 0,
    SkipValidation = 1 << 1,
    SkipOptimization = 1 << 2,
    PackMatrixRowMajor = 1 << 3,
    PackMatrixColumnMajor = 1 << 4,
    PartialPrecision = 1 << 5,
    ForceVsSoftwareNoOpt = 1 << 6,
    ForstPsSoftwareNoOpt = 1 << 7,
    NoPreShader = 1 << 8,
    AvoidFlowControl = 1 << 9,
    PreferFlowControl = 1 << 10,
    EnableStrictness = 1 << 11,
    EnableBackwardsCompatibility = 1 << 12,
    IeeeStrictness = 1 << 13,
    OptimizationLevel0 = 1 << 14,
    OptimizationLevel1 = 0,
    OptimizationLevel3 = 1 << 15,
    OptimizationLevel2 = (OptimizationLevel0 | OptimizationLevel3),
    Reserved16 = 1 << 16,
    Reserved17 = 1 << 17,
    WarningsAreErrors = 1 << 18,
    ResourcesMayAlias = 1 << 19,
    EnableUnboundedDescriptorTables = 1 << 20,
    AllResourcesBound = 1 << 21,
    DebugNameForSource = 1 << 22,
    DebugNameForBinary = 1 << 23,
}

internal static class FxcCompilerFlagsExtensions
{
    public static FxcCompileFlags Translate(this ShaderCompileFlags flags)
    {
        FxcCompileFlags fxcFlags = FxcCompileFlags.None;
        ShaderCompileFlags[] values = Enum.GetValues<ShaderCompileFlags>();

        foreach(ShaderCompileFlags f in values)
        {
            if((flags & f) == f)
            {
                switch (f)
                {
                    case ShaderCompileFlags.Debug: 
                        fxcFlags |= FxcCompileFlags.Debug; 
                        break;

                    case ShaderCompileFlags.SkipValidation: 
                        fxcFlags |= FxcCompileFlags.SkipValidation; 
                        break;

                    case ShaderCompileFlags.SkipOptimization:
                        fxcFlags |= FxcCompileFlags.SkipOptimization;
                        break;

                    case ShaderCompileFlags.Level0Optimization:
                        fxcFlags |= FxcCompileFlags.OptimizationLevel0;
                        break;

                    case ShaderCompileFlags.Level1Optimization:
                        fxcFlags |= FxcCompileFlags.OptimizationLevel1;
                        break;

                    case ShaderCompileFlags.Level2Optimization:
                        fxcFlags |= FxcCompileFlags.OptimizationLevel2;
                        break;

                    case ShaderCompileFlags.Level3Optimization:
                        fxcFlags |= FxcCompileFlags.OptimizationLevel3;
                        break;

                    case ShaderCompileFlags.WarningsAreErrors:
                        fxcFlags |= FxcCompileFlags.WarningsAreErrors;
                        break;
                }
            }
        }

        return fxcFlags;
    }
}
