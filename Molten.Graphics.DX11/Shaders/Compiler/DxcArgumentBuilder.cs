using Silk.NET.Core.Native;
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
    /// <para>https://strontic.github.io/xcyclopedia/library/dxc.exe-0C1709D4E1787E3EB3E6A35C85714824.html</para>
    /// <para>https://simoncoenen.com/blog/programming/graphics/DxcCompiling</para>
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
            [HlslCompilerArg.IgnoreUnusedArgs] = "-Qunused-arguments",
            [HlslCompilerArg.NoLogo] = "-nologo",
            [HlslCompilerArg.OutputHexLiterals] = "-Lx",
            [HlslCompilerArg.OutputInstructionNumbers] = "-Ni",
            [HlslCompilerArg.NoWarnings] = "-no-warnings",
            [HlslCompilerArg.OutputInstructionOffsets] = "-No",
            [HlslCompilerArg.StripDebug] = "_Qstrip_debug",
            [HlslCompilerArg.StripPrivate] = "-Qstrip_priv",
            [HlslCompilerArg.StripReflection] = "-Qstrip_reflect",
            [HlslCompilerArg.StripRootSignature] = "Qstrip_rootsignature"
        };

        static Dictionary<HlslCompilerArg, string> _parameterArgLookup = new Dictionary<HlslCompilerArg, string>()
        {
            [HlslCompilerArg.EntryPoint] = "-E",
            [HlslCompilerArg.TargetProfile] = "-T",
            [HlslCompilerArg.OutputAssemblyFile] = "-Fc",
            [HlslCompilerArg.OutputDebugFile] = "-Fd",
            [HlslCompilerArg.OutputErrorFile] = "-Fe",
            [HlslCompilerArg.OutputHeaderFile] = "-Fh",
            [HlslCompilerArg.OutputObjectFile] = "-Fo",
            [HlslCompilerArg.PreProcessToFile] = "-P",
        };

        Dictionary<HlslCompilerArg, string> _args;
        HlslCompilerContext _context;

        internal DxcArgumentBuilder(HlslCompilerContext context)
        {
            _args = new Dictionary<HlslCompilerArg, string>();
            _context = context;
        }

        internal bool Set(HlslCompilerArg arg)
        {
            // Ensure we're using a valid argument
            if (_argLookup.TryGetValue(arg, out string argString))
            {
                _args[arg] = argString;
                return true;
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

        internal bool Set(HlslCompilerArg arg, string parameterValue)
        {
            if (_parameterArgLookup.TryGetValue(arg, out string argString))
            {
                _args[arg] = $"{argString} {parameterValue}";
                return true;
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

        internal void SetEntryPoint(string entryPoint)
        {
            Set(HlslCompilerArg.EntryPoint, entryPoint);
        }

        internal void SetShaderProfile(ShaderModel model, ShaderType type)
        {
            string profile = model.ToProfile(type);
            Set(HlslCompilerArg.TargetProfile, profile);
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

        public char** GetArgsPtr()
        {
            IReadOnlyList<string> args = _args.Values.ToList().AsReadOnly();
            return (char**)SilkMarshal.StringArrayToPtr(args, NativeStringEncoding.LPWStr);
        }

        internal uint Count => (uint)_args.Count;
    }
}
