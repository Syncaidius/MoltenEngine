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
        HlslCompilerContext _context;
        IDxcCompilerArgs* _dxcArgs;

        internal DxcArgumentBuilder(HlslCompilerContext context)
        {
            _args = new Dictionary<HlslCompilerArg, string>();
            _context = context;
            _dxcArgs = context.Compiler.Utils->ar
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
                    _context.AddError($"HLSL compiler argument '{arg}' has already been added. Arguments cannot be added twice.");
                }
            }
            else if (_parameterArgLookup.ContainsKey(arg))
            {
                _context.AddError($"HLSL parameterized compiler argument '{arg}' cannot be added as a non-parameterized argument.");
            }
            else
            {
                _context.AddError($"Invalid compiler argument of value '{arg}'");
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
                    _context.AddError($"HLSL compiler argument '{arg}' has already been added. Arguments cannot be added twice.");
                }
            }
            else if (_argLookup.ContainsKey(arg))
            {
                _context.AddError($"HLSL non-parameterized argument '{arg}' cannot be added as parameterized argument.");
            }
            else
            {
                _context.AddError($"Invalid compiler argument of value '{arg}'");
            }

            return false;
        }

        public override string ToString()
        {
            string s = "";
            bool first = true;
            foreach (KeyValuePair<HlslCompilerArg, string> p in _args)
            {
                s += first ? p.Value : $" {p.Value}";
                first = false;
            }

            return s;
        }

        internal uint Count => (uint)_args.Count;
    }
}
