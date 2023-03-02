using System.Reflection;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class FxcCompiler : ShaderCompiler
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
        }

        protected override void OnDispose()
        {
            Compiler.Dispose();
        }

        protected override unsafe ShaderReflection BuildReflection(ShaderCompilerContext context, void* ptrData)
        {
            ID3D10Blob* bCode = (ID3D10Blob*)ptrData;
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
                    result = new ShaderCodeResult(reflection, pByteCode, pByteCode->GetBufferSize());
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

        public override ShaderIOStructure BuildIO(ShaderCodeResult result, ShaderIOStructureType type)
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
            ShaderIOStructureDX11 structure = new ShaderIOStructureDX11((uint)count);

            for (int i = 0; i < count; i++)
            {
                ShaderParameterInfo pDesc = parameters[i];

                InputElementDesc el = new InputElementDesc()
                {
                    SemanticName = (byte*)pDesc.SemanticNamePtr,
                    SemanticIndex = pDesc.SemanticIndex,
                    InputSlot = 0, // This does not need to be set. A shader has a single layout, 
                    InstanceDataStepRate = 0, // This does not need to be set. The data is set via Context.DrawInstanced + vertex data/layout.
                    AlignedByteOffset = 16 * pDesc.Register,
                    InputSlotClass = InputClassification.PerVertexData,
                };

                ShaderComponentMaskFlags usageMask = (pDesc.Mask & ShaderComponentMaskFlags.ComponentX);
                usageMask |= (pDesc.Mask & ShaderComponentMaskFlags.ComponentY);
                usageMask |= (pDesc.Mask & ShaderComponentMaskFlags.ComponentZ);
                usageMask |= (pDesc.Mask & ShaderComponentMaskFlags.ComponentW);

                ShaderRegisterType comType = pDesc.ComponentType;
                switch (usageMask)
                {
                    case ShaderComponentMaskFlags.ComponentX:
                        if (comType == ShaderRegisterType.UInt32)
                            el.Format = Format.FormatR32Uint;
                        else if (comType == ShaderRegisterType.SInt32)
                            el.Format = Format.FormatR32Sint;
                        else if (comType == ShaderRegisterType.Float32)
                            el.Format = Format.FormatR32Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY:
                        if (comType == ShaderRegisterType.UInt32)
                            el.Format = Format.FormatR32G32Uint;
                        else if (comType == ShaderRegisterType.SInt32)
                            el.Format = Format.FormatR32G32Sint;
                        else if (comType == ShaderRegisterType.Float32)
                            el.Format = Format.FormatR32G32Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY |
                ShaderComponentMaskFlags.ComponentZ:
                        if (comType == ShaderRegisterType.UInt32)
                            el.Format = Format.FormatR32G32B32Uint;
                        else if (comType == ShaderRegisterType.SInt32)
                            el.Format = Format.FormatR32G32B32Sint;
                        else if (comType == ShaderRegisterType.Float32)
                            el.Format = Format.FormatR32G32B32Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY |
                ShaderComponentMaskFlags.ComponentZ | ShaderComponentMaskFlags.ComponentW:
                        if (comType == ShaderRegisterType.UInt32)
                            el.Format = Format.FormatR32G32B32A32Uint;
                        else if (comType == ShaderRegisterType.SInt32)
                            el.Format = Format.FormatR32G32B32A32Sint;
                        else if (comType == ShaderRegisterType.Float32)
                            el.Format = Format.FormatR32G32B32A32Float;
                        break;
                }

                // Store the element
                structure.Elements[i] = el;
                structure.Metadata[i] = new ShaderIOStructure.InputElementMetadata()
                {
                    Name = pDesc.SemanticName,
                    SemanticIndex = pDesc.SemanticIndex,
                    SystemValueType = pDesc.SystemValueType
                };
            }

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

        private void OnBuildVariableStructure(
            ShaderCompilerContext context,
            HlslElement shader, ShaderCodeResult result, ShaderResourceInfo info)
        {
            ComputeTask ct = shader as ComputeTask;
            if (ct == null)
                return;

            switch (info.Type)
            {
                case ShaderInputType.UavRWStructured:
                    OnBuildRWStructuredVariable(context, ct, info);
                    break;

                case ShaderInputType.UavRWTyped:
                    OnBuildRWTypedVariable(context, ct, info);
                    break;
            }
        }

        private void OnBuildRWStructuredVariable
            (ShaderCompilerContext context,
            ComputeTask shader, ShaderResourceInfo info)
        {
            RWBufferVariable rwBuffer = GetVariableResource<RWBufferVariable>(context, shader, info);
            uint bindPoint = info.BindPoint;

            if (bindPoint >= shader.UAVs.Length)
                EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

            shader.UAVs[bindPoint] = rwBuffer;
        }

        private void OnBuildRWTypedVariable(
            ShaderCompilerContext context,
            ComputeTask shader, ShaderResourceInfo info)
        {
            RWVariable resource = null;
            uint bindPoint = info.BindPoint;

            switch (info.Dimension)
            {
                case ShaderResourceDimension.Texture1D:
                    resource = GetVariableResource<RWTexture1DVariable>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texture2D:
                    resource = GetVariableResource<RWTexture2DVariable>(context, shader, info);
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
                    obj = GetVariableResource<Texture1DVariable>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texture2DMS:
                case ShaderResourceDimension.Texture2DMSArray:
                case ShaderResourceDimension.Texture2DArray:
                case ShaderResourceDimension.Texture2D:
                    obj = GetVariableResource<Texture2DVariable>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texturecube:
                    obj = GetVariableResource<TextureCubeVariable>(context, shader, info);
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
            ShaderConstantBuffer cBuffer = new ShaderConstantBuffer(shader.Device as DeviceDX11, BufferMode.DynamicDiscard, info);
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

        protected T GetVariableResource<T>(ShaderCompilerContext context,
            HlslShader shader, ShaderResourceInfo info)
            where T : class, IShaderValue
        {
            IShaderValue existing = null;
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
                BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                bVar = Activator.CreateInstance(typeof(T), bindFlags, null, new object[] { shader }, null) as T;
                bVar.Name = info.Name;

                shader.Variables.Add(bVar.Name, bVar);
            }

            return bVar;
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
