using System.Reflection;
using System.Text;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    public unsafe class FxcCompiler : ShaderCompiler
    {
        D3DCompiler _d3dCompiler;

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
            _d3dCompiler = D3DCompiler.GetApi();
        }

        protected override void OnDispose()
        {
            _d3dCompiler.Dispose();
        }

        protected override unsafe ShaderReflection BuildReflection(ShaderCompilerContext context, void* ptrData)
        {
            ID3D10Blob* bCode = (ID3D10Blob*)ptrData;
            Guid guidReflect = ID3D11ShaderReflection.Guid;
            void* ppReflection = null;

            void* ppByteCode = bCode->GetBufferPointer();
            nuint numBytes = bCode->GetBufferSize();

            _d3dCompiler.Reflect(ppByteCode, numBytes, &guidReflect, &ppReflection);
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

                ShaderResourceInfo bindInfo = new ShaderResourceInfo()
                {
                    Name = SilkMarshal.PtrToString((nint)rDesc.Name),
                    BindCount = rDesc.BindCount,
                    BindPoint = rDesc.BindPoint,
                    Dimension = rDesc.Dimension.FromApi(),
                    Type = rDesc.Type.FromApi(),
                    NumSamples = rDesc.NumSamples,
                    ResourceReturnType = rDesc.ReturnType.FromApi(),
                    Flags = ((D3DShaderInputFlags)rDesc.UFlags).FromApi()
                };

                r.BoundResources.Add(bindInfo);
                uint bindPoint = bindInfo.BindPoint;

                switch (bindInfo.Type)
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

                        for (uint v = 0; v < bufferDesc.Variables; v++)
                        {
                            ID3D11ShaderReflectionVariable* variable = buffer->GetVariableByIndex(v);
                            ShaderVariableDesc desc = new ShaderVariableDesc();
                            variable->GetDesc(&desc);

                            ID3D11ShaderReflectionType* rType = variable->GetType();
                            ShaderTypeDesc typeDesc = new ShaderTypeDesc();
                            rType->GetDesc(&typeDesc);

                            ShaderReflection.ReflectionPtr ptrDefault = null;
                            if (desc.DefaultValue != null)
                            {
                                ptrDefault = r.NewPtr(desc.Size);
                                System.Buffer.MemoryCopy(desc.DefaultValue, ptrDefault, desc.Size, desc.Size);
                            }

                            ConstantBufferVariableInfo cVarInfo = new ConstantBufferVariableInfo()
                            {
                                DefaultValue = ptrDefault,
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
                }
            }

            PopulateReflectionParamters(r, fxcReflection, ShaderIOStructureType.Input);
            PopulateReflectionParamters(r, fxcReflection, ShaderIOStructureType.Output);

            fxcReflection.Dispose();
            return r;
        }

        private void PopulateReflectionParamters(ShaderReflection reflection, FxcReflection fxcReflection, ShaderIOStructureType type)
        {
            uint count = 0;
            List<ShaderParameterInfo> parameters;

            switch (type)
            {
                case ShaderIOStructureType.Input:
                    count = fxcReflection.Desc.InputParameters;
                    parameters = reflection.InputParameters;
                    break;

                case ShaderIOStructureType.Output:
                    count = fxcReflection.Desc.OutputParameters;
                    parameters = reflection.OutputParameters;
                    break;

                default:
                    return;
            }

            for (uint i = 0; i < count; i++)
            {
                SignatureParameterDesc pDesc = new SignatureParameterDesc();

                switch (type)
                {
                    case ShaderIOStructureType.Input:
                        fxcReflection.Ptr->GetInputParameterDesc(i, ref pDesc);
                        break;

                    case ShaderIOStructureType.Output:
                        fxcReflection.Ptr->GetOutputParameterDesc(i, ref pDesc);
                        break;
                }

                parameters.Add(new ShaderParameterInfo()
                {
                    ComponentType = (ShaderRegisterType)pDesc.ComponentType,
                    Mask = (ShaderComponentMaskFlags)pDesc.Mask,
                    ReadWriteMask = pDesc.ReadWriteMask,
                    MinPrecision = (ShaderMinPrecision)pDesc.MinPrecision,
                    Register = pDesc.Register,
                    SemanticIndex = pDesc.SemanticIndex,
                    SemanticName = SilkMarshal.PtrToString((nint)pDesc.SemanticName).ToUpper(),
                    SemanticNamePtr = pDesc.SemanticName,
                    Stream = pDesc.Stream,
                    SystemValueType = (ShaderSVType)pDesc.SystemValueType
                });
            }
        }

        /// <summary>Compiles HLSL source code and outputs the result. Returns true if successful, or false if there were errors.</summary>
        /// <param name="entryPoint"></param>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool CompileSource(string entryPoint, ShaderType type, 
            ShaderCompilerContext context, out ShaderCodeResult result)
        {
            Encoding encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252); // Ansi codepage
            NativeStringEncoding nativeEncoding = NativeStringEncoding.LPStr;
            
            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            if (!context.Shaders.TryGetValue(entryPoint, out result))
            {
                ulong numBytes = 0;
                string shaderProfile = ShaderModel.Model5_0.ToProfile(type, ShaderLanguage.Hlsl);
                byte* pSourceName = EngineUtil.StringToPtr(context.Source.Filename, encoding);
                byte* pEntryPoint = (byte*)SilkMarshal.StringToPtr(entryPoint, nativeEncoding);
                byte* pTarget = (byte*)SilkMarshal.StringToPtr(shaderProfile, nativeEncoding);
                void* pSrc = EngineUtil.StringToPtr(context.Source.SourceCode, encoding, out numBytes);
                FxcCompileFlags compileFlags = context.Flags.Translate();

                ID3D10Blob* pByteCode = null;
                ID3D10Blob* pErrors = null;
                ID3D10Blob* pProcessedSrc = null;

                // Preprocess and check for errors
                HResult hr = _d3dCompiler.Preprocess(pSrc, (nuint)numBytes, pSourceName, null, null, &pProcessedSrc, &pErrors);
                ParseErrors(context, hr, pErrors);

                // Compile source and check for errors
                if (hr.IsSuccess)
                {
                    void* postProcessedSrc = pProcessedSrc->GetBufferPointer();
                    nuint postProcessedSize = pProcessedSrc->GetBufferSize();

                    hr = _d3dCompiler.Compile(postProcessedSrc, postProcessedSize, pSourceName, null, null, pEntryPoint, pTarget, (uint)compileFlags, 0, &pByteCode, &pErrors);
                    ParseErrors(context, hr, pErrors);
                }

                //Store shader result
                if (!context.HasErrors)
                {
                    ShaderReflection reflection = BuildReflection(context, pByteCode);
                    result = new ShaderCodeResult(reflection, pByteCode, pByteCode->GetBufferSize());
                    context.Shaders.Add(entryPoint, result);
                }

                SilkUtil.ReleasePtr(ref pProcessedSrc);
                SilkUtil.ReleasePtr(ref pErrors);
                EngineUtil.Free(ref pSrc);
                EngineUtil.Free(ref pSourceName);
                SilkMarshal.Free((nint)pEntryPoint);
                SilkMarshal.Free((nint) pTarget);
            }

            return !context.HasErrors;
        }

        public override ShaderIOStructure BuildIO(ShaderCodeResult result, ShaderType sType, ShaderIOStructureType type)
        {
            List<ShaderParameterInfo> parameters;

            switch (type)
            {
                case ShaderIOStructureType.Input:
                    parameters = result.Reflection.InputParameters;
                    break;

                case ShaderIOStructureType.Output:
                    parameters = result.Reflection.OutputParameters;
                    break;

                default:
                    return null;
            }

            int count = parameters.Count;
            ShaderIOStructureDX11 structure = new ShaderIOStructureDX11(result, sType, type);
            return structure;
        }

        public override bool BuildStructure(ShaderCompilerContext context,
            HlslShader shader, ShaderCodeResult result, ShaderComposition composition)
        {
            for (int r = 0; r < result.Reflection.BoundResources.Count; r++)
            {
                ShaderResourceInfo bindInfo = result.Reflection.BoundResources[r];
                uint bindPoint = bindInfo.BindPoint;

                switch (bindInfo.Type)
                {
                    case ShaderInputType.CBuffer:
                        ConstantBufferInfo bufferInfo = result.Reflection.ConstantBuffers[bindInfo.Name];

                        // Skip binding info buffers
                        if (bufferInfo.Type != ConstantBufferType.ResourceBindInfo)
                        {
                            if (bindPoint >= shader.ConstBuffers.Length)
                                Array.Resize(ref shader.ConstBuffers, (int)bindPoint + 1);

                            if (shader.ConstBuffers[bindPoint] != null && shader.ConstBuffers[bindPoint].BufferName != bindInfo.Name)
                                context.AddMessage($"Shader constant buffer '{shader.ConstBuffers[bindPoint].BufferName}' was overwritten by buffer '{bindInfo.Name}' at the same register (b{bindPoint}).",
                                    ShaderCompilerMessage.Kind.Warning);

                            shader.ConstBuffers[bindPoint] = GetConstantBuffer(context, shader, bufferInfo);
                            composition.ConstBufferIds.Add(bindPoint);
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
                                shader.SamplerVariables[i] = (i == bindPoint ? sampler : ShaderVariable.Create<ShaderSamplerVariable>(shader, bindInfo.Name));
                        }
                        else
                        {
                            shader.SamplerVariables[bindPoint] = sampler;
                        }
                        composition.SamplerIds.Add(bindPoint);
                        break;

                    case ShaderInputType.Structured:
                        ShaderResourceVariable bVar = GetVariableResource<ShaderResourceVariable<BufferDX11>>(context, shader, bindInfo);
                        if (bindPoint >= shader.Resources.Length)
                            EngineUtil.ArrayResize(ref shader.Resources, bindPoint + 1);

                        shader.Resources[bindPoint] = bVar;
                        composition.ResourceIds.Add(bindPoint);
                        break;

                    case ShaderInputType.UavRWStructured:
                        OnBuildRWStructuredVariable(context, shader, bindInfo);
                        composition.UnorderedAccessIds.Add(bindPoint);
                        break;

                    case ShaderInputType.UavRWTyped:
                        OnBuildRWTypedVariable(context, shader, bindInfo);
                        composition.UnorderedAccessIds.Add(bindPoint);
                        break;
                }
            }

            return true;
        }

        protected bool HasConstantBuffer(ShaderCompilerContext context,
    HlslShader shader, string bufferName, string[] varNames)
        {
            foreach (ShaderConstantBuffer buffer in shader.ConstBuffers)
            {
                if (buffer == null)
                    continue;

                if (buffer.BufferName == bufferName)
                {
                    if (buffer.Variables.Length != varNames.Length)
                    {
                        context.AddMessage($"Shader '{bufferName}' constant buffer does not have the correct number of variables ({varNames.Length})",
                            ShaderCompilerMessage.Kind.Error);
                        return false;
                    }

                    for (int i = 0; i < buffer.Variables.Length; i++)
                    {
                        ShaderConstantVariable variable = buffer.Variables[i];
                        string expectedName = varNames[i];

                        if (variable.Name != expectedName)
                        {
                            context.AddMessage($"Shader '{bufferName}' constant variable #{i + 1} is incorrect: Named '{variable.Name}' instead of '{expectedName}'",
                                ShaderCompilerMessage.Kind.Error);
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private void OnBuildRWStructuredVariable
            (ShaderCompilerContext context,
            HlslShader shader, ShaderResourceInfo info)
        {
            RWVariable rwBuffer = GetVariableResource<RWVariable<BufferDX11>>(context, shader, info);
            uint bindPoint = info.BindPoint;

            if (bindPoint >= shader.UAVs.Length)
                EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

            shader.UAVs[bindPoint] = rwBuffer;
        }

        private void OnBuildRWTypedVariable(
            ShaderCompilerContext context,
            HlslShader shader, ShaderResourceInfo info)
        {
            RWVariable resource = null;
            uint bindPoint = info.BindPoint;

            switch (info.Dimension)
            {
                case ShaderResourceDimension.Texture1DArray:
                case ShaderResourceDimension.Texture1D:
                    resource = GetVariableResource<RWVariable<ITexture1D>>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texture2DMS:
                case ShaderResourceDimension.Texture2DMSArray:
                case ShaderResourceDimension.Texture2DArray:
                case ShaderResourceDimension.Texture2D:
                    resource = GetVariableResource<RWVariable<ITexture2D>>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texture3D:
                    resource = GetVariableResource<RWVariable<ITexture3D>>(context, shader, info);
                    break;

                case ShaderResourceDimension.TextureCube:
                case ShaderResourceDimension.TextureCubeArray:
                    resource = GetVariableResource<RWVariable<ITextureCube>>(context, shader, info);
                    break;
            }

            if (bindPoint >= shader.UAVs.Length)
                EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

            // Store the resource variable
            shader.UAVs[bindPoint] = resource;
        }

        private void OnBuildTextureVariable(ShaderCompilerContext context,
            HlslShader shader, ShaderResourceInfo info)
        {
            ShaderResourceVariable obj = null;

            switch (info.Dimension)
            {
                case ShaderResourceDimension.Texture1DArray:
                case ShaderResourceDimension.Texture1D:
                    obj = GetVariableResource<ShaderResourceVariable<ITexture1D>>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texture2DMS:
                case ShaderResourceDimension.Texture2DMSArray:
                case ShaderResourceDimension.Texture2DArray:
                case ShaderResourceDimension.Texture2D:
                    obj = GetVariableResource<ShaderResourceVariable<ITexture2D>>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texture3D:
                    obj = GetVariableResource<ShaderResourceVariable<ITexture3D>>(context, shader, info);
                    break;

                case ShaderResourceDimension.TextureCube:
                    obj = GetVariableResource<ShaderResourceVariable<ITextureCube>>(context, shader, info);
                    break;
            }

            if (info.BindPoint >= shader.Resources.Length)
                EngineUtil.ArrayResize(ref shader.Resources, info.BindPoint + 1);

            //store the resource variable
            shader.Resources[info.BindPoint] = obj;
        }

        private unsafe ShaderConstantBuffer GetConstantBuffer(ShaderCompilerContext context,
            HlslShader shader, ConstantBufferInfo info)
        {
            ShaderConstantBuffer cBuffer = new ShaderConstantBuffer(shader.Device as DeviceDX11, info);
            string localName = cBuffer.BufferName;

            if (cBuffer.BufferName == "$Globals")
                localName += $"_{shader.Name}";

            // Duplication checks.
            if (context.TryGetResource(localName, out ShaderConstantBuffer existing))
            {
                // Check for duplicates
                if (existing != null)
                {
                    // Compare buffers. If identical, 
                    if (existing.Hash == cBuffer.Hash)
                    {
                        // Dispose of new buffer, use existing.
                        cBuffer.Dispose();
                        cBuffer = existing;
                    }
                    else
                    {
                        context.AddMessage($"Constant buffers with the same name ('{localName}') do not match. Differing layouts.");
                    }
                }
                else
                {
                    context.AddMessage($"Constant buffer creation failed. A resource with the name '{localName}' already exists!");
                }
            }
            else
            {
                // Register all of the new buffer's variables
                foreach (ShaderConstantVariable v in cBuffer.Variables)
                {
                    // Check for duplicate variables
                    if (shader.Variables.ContainsKey(v.Name))
                    {
                        context.AddMessage($"Duplicate variable detected: {v.Name}");
                        continue;
                    }

                    shader.Variables.Add(v.Name, v);
                }

                // Register the new buffer
                context.AddResource(localName, cBuffer);
            }

            return cBuffer;
        }

        protected T GetVariableResource<T>(ShaderCompilerContext context, HlslShader shader, ShaderResourceInfo info)
            where T : ShaderVariable, new()
        {
            ShaderVariable existing = null;
            T bVar = null;
            Type t = typeof(T);

            if (shader.Variables.TryGetValue(info.Name, out existing))
            {
                T other = existing as T;

                if (other != null)
                {
                    // If valid, use existing buffer variable.
                    if (other.GetType() == t)
                        bVar = other;
                }
                else
                {
                    context.AddMessage($"Resource '{t.Name}' creation failed. A resource with the name '{info.Name}' already exists!");
                }
            }
            else
            {
                bVar = ShaderVariable.Create<T>(shader, info.Name);
                shader.Variables.Add(bVar.Name, bVar);
            }

            return bVar;
        }

        public override unsafe void* BuildShader(HlslPass parent, ShaderType type, void* byteCode)
        {
            ID3D10Blob* dx11ByteCode = (ID3D10Blob*)byteCode;
            void* ptrBytecode = dx11ByteCode->GetBufferPointer();
            nuint numBytes = dx11ByteCode->GetBufferSize();
            DeviceDX11 device = Renderer.Device as DeviceDX11;

            switch (type)
            {
                case ShaderType.Compute:
                    parent.InputByteCode = byteCode;

                    ID3D11ComputeShader* csShader = null;
                    device.Ptr->CreateComputeShader(ptrBytecode, numBytes, null, &csShader);
                    return csShader;

                case ShaderType.Vertex:
                    parent.InputByteCode = byteCode;

                    ID3D11VertexShader* vsShader = null;
                    device.Ptr->CreateVertexShader(ptrBytecode, numBytes, null, &vsShader);
                    return vsShader;

                case ShaderType.Hull:
                    ID3D11HullShader* hsShader = null;
                    device.Ptr->CreateHullShader(ptrBytecode, numBytes, null, &hsShader);
                    return hsShader;

                case ShaderType.Domain:
                    ID3D11DomainShader* dsShader = null;
                    device.Ptr->CreateDomainShader(ptrBytecode, numBytes, null, &dsShader);
                    return dsShader;

                case ShaderType.Geometry:
                    ID3D11GeometryShader* gsShader = null;
                    device.Ptr->CreateGeometryShader(ptrBytecode, numBytes, null, &gsShader);
                    return gsShader;

                case ShaderType.Pixel:
                    ID3D11PixelShader* psShader = null;
                    device.Ptr->CreatePixelShader(ptrBytecode, numBytes, null, &psShader);
                    return psShader;
            }

            return null;
        }

        private void ParseErrors(ShaderCompilerContext context, HResult hr, ID3D10Blob* errors)
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
