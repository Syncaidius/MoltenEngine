using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;

namespace Molten.Graphics.Dxc
{
    /// <summary>
    /// See for argument details:
    /// https://github.com/microsoft/DirectXShaderCompiler/blob/master/include/dxc/Support/HLSLOptions.td
    /// See for important ones:
    /// <para>https://strontic.github.io/xcyclopedia/library/dxc.exe-0C1709D4E1787E3EB3E6A35C85714824.html</para>
    /// <para>https://simoncoenen.com/blog/programming/graphics/DxcCompiling</para>
    /// <para>For vulkan-specific arguments, see: https://github.com/Microsoft/DirectXShaderCompiler/blob/main/docs/SPIR-V.rst#vulkan-specific-options</para>
    /// </summary>
    internal unsafe class DxcArgumentBuilder
    {
        static Dictionary<DxcCompilerArg, string> _argLookup = new Dictionary<DxcCompilerArg, string>()
        {
            [DxcCompilerArg.AllResourcesBound] = DXC.ArgAllResourcesBound,
            [DxcCompilerArg.AvoidFlowControl] = DXC.ArgAvoidFlowControl,
            [DxcCompilerArg.Debug] = DXC.ArgDebug,
            [DxcCompilerArg.DebugNameForBinary] = DXC.ArgDebugNameForBinary,
            [DxcCompilerArg.DebugNameForSource] = DXC.ArgDebugNameForSource,
            [DxcCompilerArg.EnableBackwardsCompatibility] = DXC.ArgEnableBackwardsCompatibility,
            [DxcCompilerArg.EnableStrictness] = DXC.ArgEnableStrictness,
            [DxcCompilerArg.IeeeStrictness] = DXC.ArgIeeeStrictness,
            [DxcCompilerArg.None] = "",
            [DxcCompilerArg.SpirV] = "-spirv",
            [DxcCompilerArg.OptimizationLevel0] = DXC.ArgOptimizationLevel0,
            [DxcCompilerArg.OptimizationLevel1] = DXC.ArgOptimizationLevel1,
            [DxcCompilerArg.OptimizationLevel2] = DXC.ArgOptimizationLevel2,
            [DxcCompilerArg.OptimizationLevel3] = DXC.ArgOptimizationLevel3,
            [DxcCompilerArg.PackMatrixColumnMajor] = DXC.ArgPackMatrixColumnMajor,
            [DxcCompilerArg.PackMatrixRowMajor] = DXC.ArgPackMatrixRowMajor,
            [DxcCompilerArg.PreferFlowControl] = DXC.ArgPreferFlowControl,
            [DxcCompilerArg.ResourcesMayAlias] = DXC.ArgResourcesMayAlias,
            [DxcCompilerArg.SkipOptimizations] = DXC.ArgSkipOptimizations,
            [DxcCompilerArg.SkipValidation] = DXC.ArgSkipValidation,
            [DxcCompilerArg.WarningsAreErrors] = DXC.ArgWarningsAreErrors,
            [DxcCompilerArg.IgnoreUnusedArgs] = "-Qunused-arguments",
            [DxcCompilerArg.NoLogo] = "-nologo",
            [DxcCompilerArg.OutputHexLiterals] = "-Lx",
            [DxcCompilerArg.OutputInstructionNumbers] = "-Ni",
            [DxcCompilerArg.NoWarnings] = "-no-warnings",
            [DxcCompilerArg.OutputInstructionOffsets] = "-No",
            [DxcCompilerArg.StripDebug] = "_Qstrip_debug",
            [DxcCompilerArg.StripPrivate] = "-Qstrip_priv",
            [DxcCompilerArg.StripReflection] = "-Qstrip_reflect",
            [DxcCompilerArg.StripRootSignature] = "-Qstrip_rootsignature",
            [DxcCompilerArg.SpirVReflection] = "-fspv-reflect",

            // Parameterized arguments
            [DxcCompilerArg.EntryPoint] = "-E",
            [DxcCompilerArg.TargetProfile] = "-T",
            [DxcCompilerArg.OutputAssemblyFile] = "-Fc",
            [DxcCompilerArg.OutputDebugFile] = "-Fd",
            [DxcCompilerArg.OutputErrorFile] = "-Fe",
            [DxcCompilerArg.OutputHeaderFile] = "-Fh",
            [DxcCompilerArg.OutputObjectFile] = "-Fo",
            [DxcCompilerArg.PreProcessToFile] = "-P",
            [DxcCompilerArg.HlslVersion] = "-HV",
            [DxcCompilerArg.VulkanVersion] = "-fspv-target-env=",
        };

        Dictionary<DxcCompilerArg, string[]> _args;
        ShaderCompilerContext _context;

        internal DxcArgumentBuilder(ShaderCompilerContext context)
        {
            _args = new Dictionary<DxcCompilerArg, string[]>();
            _context = context;
        }

        internal bool Set(DxcCompilerArg arg, string parameterValue = null)
        {
            if (_argLookup.TryGetValue(arg, out string argString))
            {
                if (string.IsNullOrWhiteSpace(parameterValue))
                {
                    _args[arg] = new string[] { argString };
                }
                else
                {
                    // If the parameter is set using an = operator, we don't need to separate the arg and it's value
                    if (argString.EndsWith('='))
                        _args[arg] = new string[] { $"{argString}{parameterValue}" };
                    else
                        _args[arg] = new string[] { argString, parameterValue };
                }

                return true;
            }
            else
            {
                _context.AddError($"Invalid compiler argument of value '{arg}'");
            }

            return false;
        }

        internal void SetEntryPoint(string entryPoint)
        {
            Set(DxcCompilerArg.EntryPoint, entryPoint);
        }

        internal void SetShaderProfile(ShaderModel model, ShaderType type)
        {
            string profile = model.ToProfile(type, ShaderLanguage.Hlsl);
            Set(DxcCompilerArg.TargetProfile, profile);
        }

        public override string ToString()
        {
            string s = "";
            bool first = true;
            foreach (KeyValuePair<DxcCompilerArg, string[]> p in _args)
            {
                s += $"{(first ? "" : " ")}{string.Join(' ', p.Value)}";
                first = false;
            }

            return s;
        }

        public char** GetArgsPtr(NativeStringEncoding encoding, out uint argCount)
        {
            string[] args = GetArgsArray();
            argCount = (uint)args.Length;
            return (char**)SilkMarshal.StringArrayToPtr(args, encoding);
        }

        public string[] GetArgsArray()
        {
            List<string> args = new List<string>();
            foreach (string[] p in _args.Values)
                args.AddRange(p);

            return args.ToArray();
        }

        internal uint Count => (uint)_args.Count;
    }
}
