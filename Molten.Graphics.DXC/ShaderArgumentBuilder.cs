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
    internal unsafe class ShaderArgumentBuilder
    {
        static Dictionary<ShaderCompilerArg, string> _argLookup = new Dictionary<ShaderCompilerArg, string>()
        {
            [ShaderCompilerArg.AllResourcesBound] = DXC.ArgAllResourcesBound,
            [ShaderCompilerArg.AvoidFlowControl] = DXC.ArgAvoidFlowControl,
            [ShaderCompilerArg.Debug] = DXC.ArgDebug,
            [ShaderCompilerArg.DebugNameForBinary] = DXC.ArgDebugNameForBinary,
            [ShaderCompilerArg.DebugNameForSource] = DXC.ArgDebugNameForSource,
            [ShaderCompilerArg.EnableBackwardsCompatibility] = DXC.ArgEnableBackwardsCompatibility,
            [ShaderCompilerArg.EnableStrictness] = DXC.ArgEnableStrictness,
            [ShaderCompilerArg.IeeeStrictness] = DXC.ArgIeeeStrictness,
            [ShaderCompilerArg.None] = "",
            [ShaderCompilerArg.OptimizationLevel0] = DXC.ArgOptimizationLevel0,
            [ShaderCompilerArg.OptimizationLevel1] = DXC.ArgOptimizationLevel1,
            [ShaderCompilerArg.OptimizationLevel2] = DXC.ArgOptimizationLevel2,
            [ShaderCompilerArg.OptimizationLevel3] = DXC.ArgOptimizationLevel3,
            [ShaderCompilerArg.PackMatrixColumnMajor] = DXC.ArgPackMatrixColumnMajor,
            [ShaderCompilerArg.PackMatrixRowMajor] = DXC.ArgPackMatrixRowMajor,
            [ShaderCompilerArg.PreferFlowControl] = DXC.ArgPreferFlowControl,
            [ShaderCompilerArg.ResourcesMayAlias] = DXC.ArgResourcesMayAlias,
            [ShaderCompilerArg.SkipOptimizations] = DXC.ArgSkipOptimizations,
            [ShaderCompilerArg.SkipValidation] = DXC.ArgSkipValidation,
            [ShaderCompilerArg.WarningsAreErrors] = DXC.ArgWarningsAreErrors,
            [ShaderCompilerArg.IgnoreUnusedArgs] = "-Qunused-arguments",
            [ShaderCompilerArg.NoLogo] = "-nologo",
            [ShaderCompilerArg.OutputHexLiterals] = "-Lx",
            [ShaderCompilerArg.OutputInstructionNumbers] = "-Ni",
            [ShaderCompilerArg.NoWarnings] = "-no-warnings",
            [ShaderCompilerArg.OutputInstructionOffsets] = "-No",
            [ShaderCompilerArg.StripDebug] = "_Qstrip_debug",
            [ShaderCompilerArg.StripPrivate] = "-Qstrip_priv",
            [ShaderCompilerArg.StripReflection] = "-Qstrip_reflect",
            [ShaderCompilerArg.StripRootSignature] = "Qstrip_rootsignature"
        };

        static Dictionary<ShaderCompilerArg, string> _parameterArgLookup = new Dictionary<ShaderCompilerArg, string>()
        {
            [ShaderCompilerArg.EntryPoint] = "-E",
            [ShaderCompilerArg.TargetProfile] = "-T",
            [ShaderCompilerArg.OutputAssemblyFile] = "-Fc",
            [ShaderCompilerArg.OutputDebugFile] = "-Fd",
            [ShaderCompilerArg.OutputErrorFile] = "-Fe",
            [ShaderCompilerArg.OutputHeaderFile] = "-Fh",
            [ShaderCompilerArg.OutputObjectFile] = "-Fo",
            [ShaderCompilerArg.PreProcessToFile] = "-P",
        };

        Dictionary<ShaderCompilerArg, string> _args;
        ShaderCompilerContext _context;

        internal ShaderArgumentBuilder(ShaderCompilerContext context)
        {
            _args = new Dictionary<ShaderCompilerArg, string>();
            _context = context;
        }

        internal bool Set(ShaderCompilerArg arg)
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

        internal bool Set(ShaderCompilerArg arg, string parameterValue)
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
            Set(ShaderCompilerArg.EntryPoint, entryPoint);
        }

        internal void SetShaderProfile(ShaderModel model, ShaderType type)
        {
            string profile = model.ToProfile(type);
            Set(ShaderCompilerArg.TargetProfile, profile);
        }

        public override string ToString()
        {
            string s = "";
            bool first = true;
            foreach (KeyValuePair<ShaderCompilerArg, string> p in _args)
            {
                s += first ? p.Value : $" {p.Value}";
                first = false;
            }

            return s;
        }

        public char** GetArgsPtr()
        {
            IReadOnlyList<string> args = _args.Values.ToList().AsReadOnly();
            return (char**)SilkMarshal.StringArrayToPtr(args, NativeStringEncoding.UTF8);
        }

        internal uint Count => (uint)_args.Count;
    }
}
