using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using System;
using System.Net;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms;

namespace Molten.Graphics
{
    internal unsafe class FxcCompiler : ShaderCompiler<RendererDX11, HlslFoundation>
    {
        internal D3DCompiler Compiler { get; }

        /// <summary>
        /// Creates a new instance of <see cref="FxcCompiler"/>.
        /// </summary>
        /// <param name="renderer">The renderer which owns the compiler.</param>
        /// <param name="log"></param>
        /// <param name="includePath">The default path for engine/game HLSL include files.</param>
        /// <param name="includeAssembly"></param>
        internal FxcCompiler(RendererDX11 renderer, Logger log, string includePath, Assembly includeAssembly) :
            base(renderer, includePath, includeAssembly)
        {
            Compiler = D3DCompiler.GetApi();

            AddClassCompiler<MaterialCompiler>();
            AddClassCompiler<ComputeCompiler>();
        }

        protected override void OnDispose()
        {
            Compiler.Dispose();
        }

        protected override unsafe ShaderReflection BuildReflection(ShaderCompilerContext<RendererDX11, HlslFoundation> context, void* byteCode)
        {
            ID3D10Blob* bCode = (ID3D10Blob*)byteCode;
            Guid guidReflect = ID3D11ShaderReflection.Guid;
            void* ppReflection = null;

            void* ppByteCode = bCode->GetBufferPointer();
            nuint numBytes = bCode->GetBufferSize();

            Compiler.Reflect(ppByteCode, numBytes, &guidReflect, &ppReflection);
            FxcReflection fxcReflection = new FxcReflection((ID3D11ShaderReflection*)ppReflection);
            ShaderReflection r = new ShaderReflection()
            {
                GSInputPrimitive = fxcReflection.Ptr->GetGSInputPrimitive().FromApi(),
            };

            // Get shader resource input bindings
            for (uint i = 0; i < fxcReflection.Desc.BoundResources; i++)
            {
                ShaderInputBindDesc rDesc = new ShaderInputBindDesc();
                fxcReflection.Ptr->GetResourceBindingDesc(i, &rDesc);

                ShaderInputInfo bindInfo = new ShaderInputInfo()
                {
                    Name = SilkMarshal.PtrToString((nint)rDesc.Name),
                    BindCount = rDesc.BindCount,
                    BindPoint = rDesc.BindPoint,
                    Dimension = rDesc.Dimension.FromApi(),
                    InputType = rDesc.Type.FromApi(),
                    NumSamples = rDesc.NumSamples,
                    ResourceReturnType = rDesc.ReturnType.FromApi(),
                    Flags = ((D3DShaderInputFlags)rDesc.UFlags).FromApi()
                };

                r.Inputs.Add(bindInfo);
                uint bindPoint = bindInfo.BindPoint;

                switch (bindInfo.InputType)
                {
                    case ShaderInputType.CBuffer:
                        ID3D11ShaderReflectionConstantBuffer* buffer = fxcReflection.Ptr->GetConstantBufferByName(bindInfo.Name);
                        ShaderBufferDesc bufferDesc = new ShaderBufferDesc();
                        buffer->GetDesc(ref bufferDesc);

                        // Skip binding info buffers
                        if (bufferDesc.Type == D3DCBufferType.D3DCTResourceBindInfo)
                            continue;

                        ConstantBufferInfo cBufferInfo = new ConstantBufferInfo()
                        {
                            Name = bindInfo.Name,
                            Type = bufferDesc.Type.FromApi(),
                            Flags = (ConstantBufferFlags)bufferDesc.UFlags,
                            Size = bufferDesc.Size,
                        };

                        r.ConstantBuffers.Add(bindInfo.Name, cBufferInfo);

                        for(uint v = 0; v < bufferDesc.Variables; v++)
                        {
                            ID3D11ShaderReflectionVariable* variable = buffer->GetVariableByIndex(v);
                            ShaderVariableDesc desc = new ShaderVariableDesc();
                            variable->GetDesc(&desc);

                            ID3D11ShaderReflectionType* rType = variable->GetType();
                            ShaderTypeDesc typeDesc = new ShaderTypeDesc();
                            rType->GetDesc(&typeDesc);

                            ConstantBufferVariableInfo cVarInfo = new ConstantBufferVariableInfo()
                            {
                                DefaultValue = desc.DefaultValue, // TODO copy the value to EngineUtil memory so it isn't lost
                                Name = SilkMarshal.PtrToString((nint)desc.Name),
                                Size = desc.Size,
                                StartOffset = desc.StartOffset,
                                SamplerSize = desc.SamplerSize,
                                StartSampler = desc.StartSampler,
                                StartTexture = desc.StartTexture,
                                TextureSize = desc.TextureSize,
                                Flags = (ShaderVariableFlags)desc.UFlags,
                            };

                            cBufferInfo.Variables.Add(cVarInfo);
                            cVarInfo.Type.Name = SilkMarshal.PtrToString((nint)typeDesc.Name);
                            cVarInfo.Type.Offset = typeDesc.Offset;
                            cVarInfo.Type.Type = (ShaderVariableType)typeDesc.Type;
                            cVarInfo.Type.Class = (ShaderVariableClass)typeDesc.Class;
                            cVarInfo.Type.ColumnCount = typeDesc.Columns;
                            cVarInfo.Type.RowCount = typeDesc.Rows;
                            cVarInfo.Type.Elements = typeDesc.Elements;
                        }
                        break;

                    case ShaderInputType.Texture:
                        OnBuildTextureVariable(context, shader, bindInfo);
                        composition.ResourceIds.Add(bindInfo.BindPoint);
                        break;

                    case ShaderInputType.Sampler:
                        bool isComparison = bindInfo.HasInputFlags(ShaderInputFlags.ComparisonSampler);
                        ShaderSamplerVariable sampler = GetVariableResource<ShaderSamplerVariable>(context, shader, bindInfo);

                        if (bindPoint >= shader.SamplerVariables.Length)
                        {
                            int oldLength = shader.SamplerVariables.Length;
                            EngineUtil.ArrayResize(ref shader.SamplerVariables, bindPoint + 1);
                            for (int i = oldLength; i < shader.SamplerVariables.Length; i++)
                                shader.SamplerVariables[i] = (i == bindPoint ? sampler : new ShaderSamplerVariable(shader));
                        }
                        else
                        {
                            shader.SamplerVariables[bindPoint] = sampler;
                        }
                        composition.SamplerIds.Add(bindPoint);
                        break;

                    case ShaderInputType.Structured:
                        BufferVariable bVar = GetVariableResource<BufferVariable>(context, shader, bindInfo);
                        if (bindPoint >= shader.Resources.Length)
                            EngineUtil.ArrayResize(ref shader.Resources, bindPoint + 1);

                        shader.Resources[bindPoint] = bVar;
                        composition.ResourceIds.Add(bindPoint);
                        break;

                    default:
                        OnBuildVariableStructure(context, shader, result, bindInfo);
                        break;
                }
            }

            // TODO populate ShaderReflection object

            fxcReflection.Dispose();
            return r;
        }

        /// <summary>Compiles HLSL source code and outputs the result. Returns true if successful, or false if there were errors.</summary>
        /// <param name="entryPoint"></param>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal bool CompileSource(string entryPoint, ShaderType type, 
            ShaderCompilerContext<RendererDX11, HlslFoundation> context, out ShaderClassResult result)
        {
            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            if (!context.Shaders.TryGetValue(entryPoint, out result))
            {
                string shaderProfile = ShaderModel.Model5_0.ToProfile(type, ShaderLanguage.Hlsl);

                byte* pSourceName = (byte*)SilkMarshal.StringToPtr(context.Source.Filename, NativeStringEncoding.LPStr);
                byte* pEntryPoint = (byte*)SilkMarshal.StringToPtr(entryPoint, NativeStringEncoding.LPStr);
                byte* pTarget = (byte*)SilkMarshal.StringToPtr(shaderProfile, NativeStringEncoding.LPStr);
                void* pSrc = (void*)SilkMarshal.StringToPtr(context.Source.SourceCode, NativeStringEncoding.LPStr);
                FxcCompileFlags compileFlags = context.Flags.Translate();

                ID3D10Blob* pByteCode = null;
                ID3D10Blob* pErrors = null;
                ID3D10Blob* pProcessedSrc = null;

                uint numBytes = (uint)SilkMarshal.GetMaxSizeOf(context.Source.SourceCode, NativeStringEncoding.LPStr);

                // Preprocess and check for errors
                HResult hr = Compiler.Preprocess(pSrc, numBytes, pSourceName, null, null, &pProcessedSrc, &pErrors);
                ParseErrors(context, hr, pErrors);

                // Compile source and check for errors
                if (hr.IsSuccess)
                {
                    void* postProcessedSrc = pProcessedSrc->GetBufferPointer();
                    nuint postProcessedSize = pProcessedSrc->GetBufferSize();

                    hr = Compiler.Compile(postProcessedSrc, postProcessedSize, pSourceName, null, null, pEntryPoint, pTarget, (uint)compileFlags, 0, &pByteCode, &pErrors);
                    ParseErrors(context, hr, pErrors);
                }

                //Store shader result
                if (!context.HasErrors)
                {
                    ShaderReflection reflection = BuildReflection(context, pByteCode);
                    result = new ShaderClassResult(reflection, pByteCode, pByteCode->GetBufferSize());
                    context.Shaders.Add(entryPoint, result);
                }

                SilkUtil.ReleasePtr(ref pProcessedSrc);
                SilkUtil.ReleasePtr(ref pErrors);
                SilkMarshal.Free((nint)pSrc);
                SilkMarshal.Free((nint)pSourceName);
                SilkMarshal.Free((nint)pEntryPoint);
                SilkMarshal.Free((nint)pTarget);
            }

            return !context.HasErrors;
        }

        private void ParseErrors(ShaderCompilerContext<RendererDX11, HlslFoundation> context, HResult hr, ID3D10Blob* errors)
        {
            if (errors == null)
                return;

            void* ptrErrors = errors->GetBufferPointer();
            nuint numBytes = errors->GetBufferSize();
            string strErrors = SilkMarshal.PtrToString((nint)ptrErrors, NativeStringEncoding.UTF8);
            string[] errorList = strErrors.Split('\r', '\n');

            if (hr.IsSuccess)
            {
                for (int i = 0; i < errorList.Length; i++)
                    context.AddWarning(errorList[i]);
            }
            else
            {
                for (int i = 0; i < errorList.Length; i++)
                    context.AddError(errorList[i]);
            }
        }
    }
}
