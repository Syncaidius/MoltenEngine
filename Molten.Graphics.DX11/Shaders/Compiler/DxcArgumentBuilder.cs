using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// See for argument details:
    /// https://github.com/microsoft/DirectXShaderCompiler/blob/master/include/dxc/Support/HLSLOptions.td
    /// See for important ones:
    /// https://simoncoenen.com/blog/programming/graphics/DxcCompiling
    /// </summary>
    internal unsafe class DxcArgumentBuilder
    {
        static Dictionary<HlslCompilerArg, string> _argLookup = new Dictionary<HlslCompilerArg, string>()
        {
            [HlslCompilerArg.AllResourcesBound] = DXC.ArgAllResourcesBound,
            [HlslCompilerArg.AvoidFlowControl] = DXC.ArgAvoidFlowControl,
            [HlslCompilerArg.Debug] = DXC.ArgDebug,
            [HlslCompilerArg.DebugNameForBinary] = DXC.ArgDebugNameForBinary,
            [HlslCompilerArg.DebugNameForSource] = DXC.ArgDebugNameForSource,
            [HlslCompilerArg.EnableBackwardsCompatibility] = DXC.ArgEnableBackwardsCompatibility,
            [HlslCompilerArg.EnableStrictness] = DXC.ArgEnableStrictness,
            [HlslCompilerArg.IeeeStrictness] = DXC.ArgIeeeStrictness,
            [HlslCompilerArg.None] = "",
            [HlslCompilerArg.OptimizationLevel0] = DXC.ArgOptimizationLevel0,
            [HlslCompilerArg.OptimizationLevel1] = DXC.ArgOptimizationLevel1,
            [HlslCompilerArg.OptimizationLevel2] = DXC.ArgOptimizationLevel2,
            [HlslCompilerArg.OptimizationLevel3] = DXC.ArgOptimizationLevel3,
            [HlslCompilerArg.PackMatrixColumnMajor] = DXC.ArgPackMatrixColumnMajor,
            [HlslCompilerArg.PackMatrixRowMajor] = DXC.ArgPackMatrixRowMajor,
            [HlslCompilerArg.PreferFlowControl] = DXC.ArgPreferFlowControl,
            [HlslCompilerArg.ResourcesMayAlias] = DXC.ArgResourcesMayAlias,
            [HlslCompilerArg.SkipOptimizations] = DXC.ArgSkipOptimizations,
            [HlslCompilerArg.SkipValidation] = DXC.ArgSkipValidation,
            [HlslCompilerArg.WarningsAreErrors] = DXC.ArgWarningsAreErrors,
        };

        static Dictionary<HlslCompilerArg, string> _parameterArgLookup = new Dictionary<HlslCompilerArg, string>()
        {

        };

        Dictionary<HlslCompilerArg, string> _args;
        int _argCount;
        Logger _log;

        internal DxcArgumentBuilder(Logger log)
        {
            _args = new Dictionary<HlslCompilerArg, string>();
            _log = log;
        }

        internal bool Add(HlslCompilerArg arg)
        {
            // Ensure we're using a valid argument
            if (_argLookup.TryGetValue(arg, out string argString))
            {
                if (!_args.ContainsKey(arg))
                {
                    _args[arg] = argString;
                    return true;
                }
                else
                {
                    _log.WriteError($"HLSL compiler argument '{arg}' has already been added. Arguments cannot be added twice.");
                }
            }
            else if (_parameterArgLookup.ContainsKey(arg))
            {
                _log.WriteError($"HLSL parameterized compiler argument '{arg}' cannot be added as a non-parameterized argument.");
            }
            else
            {
                _log.WriteError($"Invalid compiler argument of value '{arg}'");
            }

            return false;
        }

        internal bool Add(HlslCompilerArg arg, string parameterValue)
        {
            if (_parameterArgLookup.TryGetValue(arg, out string argString))
            {
                if (!_args.ContainsKey(arg))
                {
                    _args[arg] = $"{argString} {parameterValue}";
                    return true;
                }
                else
                {
                    _log.WriteError($"HLSL compiler argument '{arg}' has already been added. Arguments cannot be added twice.");
                }
            }
            else if (_argLookup.ContainsKey(arg))
            {
                _log.WriteError($"HLSL non-parameterized argument '{arg}' cannot be added as parameterized argument.");
            }
            else
            {
                _log.WriteError($"Invalid compiler argument of value '{arg}'");
            }

            return false;
        }

        internal IDxcCompilerArgs* BuildArgs()
        {
            string s = "";
            bool first = true;
            foreach (KeyValuePair<HlslCompilerArg, string> p in _args)
            {
                if (!first)
                {
                    s += $" {p.Value}";
                }
                else
                {
                    s += p.Value;
                    first = false;
                }
            }

            return s;
        }
    }
}
