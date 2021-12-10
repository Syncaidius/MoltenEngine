using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// See for info: https://github.com/microsoft/DirectXShaderCompiler/blob/dc7789738c51994559424c67629acc90f4ba69ad/include/dxc/dxcapi.h#L135
    /// </summary>
    [Flags]
    internal enum HlslCompilerFlags
    {
        None = 0,

        DebugNameForBinary = 1,

        DebugNameForSource = 1 << 1,

        AllResourcesBound = 1 << 2,

        ResourcesMayAlias = 1 << 4,

        WarningsAreErrors = 1 << 5,

        OptimizationLevel3 = 1 << 6,

        OptimizationLevel2 = 1 << 7,

        OptimizationLevel1 = 1 << 8,

        OptimizationLevel0 = 1 << 9,

        IeeeStrictness = 1 << 10,

        EnableBackwardsCompatibility = 1 << 11,

        EnableStrictness = 1 << 12,

        PreferFlowControl = 1 << 13,

        AvoidFlowControl = 1 << 14,

        PackMatrixRowMajor = 1 << 15,

        PackMatrixColumnMajor = 1 << 16,

        Debug = 1 << 17,

        SkipValidation = 1 << 18,

        SkipOptimizations = 1 << 19,
    }
}
