namespace Molten.Graphics.Dxc
{
    /// <summary>
    /// See for info: https://github.com/microsoft/DirectXShaderCompiler/blob/dc7789738c51994559424c67629acc90f4ba69ad/include/dxc/dxcapi.h#L135
    /// </summary>
    public enum DxcCompilerArg
    {
        None = 0,

        DebugNameForBinary = 1,

        DebugNameForSource = 2,

        AllResourcesBound = 3,

        ResourcesMayAlias = 4,

        WarningsAreErrors = 5,

        OptimizationLevel3 = 6,

        OptimizationLevel2 = 7,

        OptimizationLevel1 = 8,

        OptimizationLevel0 = 9,

        IeeeStrictness = 10,

        EnableBackwardsCompatibility = 11,

        EnableStrictness = 12,

        PreferFlowControl = 13,

        AvoidFlowControl = 14,

        PackMatrixRowMajor = 15,

        PackMatrixColumnMajor = 16,

        Debug = 17,

        SkipValidation = 18,

        SkipOptimizations = 19,

        EntryPoint = 20,

        TargetProfile = 21,

        NoLogo = 22,

        /// <summary>
        /// Don't emit warnings for unused driver arguments
        /// </summary>
        IgnoreUnusedArgs = 23,

        OutputAssemblyFile = 24,

        OutputDebugFile = 25,

        OutputErrorFile = 26,

        OutputHeaderFile = 27,

        OutputObjectFile = 28,

        /// <summary>
        /// Output hexadecimal literals
        /// </summary>
        OutputHexLiterals = 29,

        /// <summary>
        /// Output instruction numbers in assembly listings
        /// </summary>
        OutputInstructionNumbers = 30,

        NoWarnings = 31,

        /// <summary>
        /// Output instruction byte offsets in assembly listings
        /// </summary>
        OutputInstructionOffsets = 32,

        StripDebug = 33,

        StripPrivate = 34,

        StripReflection = 35,

        StripRootSignature = 36,

        /// <summary>
        /// Send pre-processing results to file. This argument must be used alone.
        /// </summary>
        PreProcessToFile = 37,

        /// <summary>
        /// Tells DXC to compile HLSL to Spr-V bytecode.
        /// </summary>
        SpirV = 38,

        /// <summary>
        /// Tells DXC what version of HLSL we are compiling. e.g. "2021".
        /// </summary>
        HlslVersion = 39,

        /// <summary>
        /// The version of vulkan to target. e.g. "vulkan1.1", "vulkan1.2" or "vulkan1.3".
        /// </summary>
        VulkanVersion = 40,

        /// <summary>
        /// Emit additional SPIR-V instructions to aid reflection. Only works if <see cref="DxcCompilerArg.SpirV"/> is set.
        /// </summary>
        SpirVReflection = 40,
    }
}
