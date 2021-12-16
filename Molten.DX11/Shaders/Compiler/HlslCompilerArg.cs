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
    internal enum HlslCompilerArg
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
    }
}
