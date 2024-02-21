using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using System.Reflection;
using System.Text;

namespace Molten.Graphics.DX11;

public unsafe class FxcCompiler : ShaderCompiler
{
    public const ShaderModel MIN_SHADER_MODEL = ShaderModel.Model5_0;
    public const ShaderModel MAX_SHADER_MODEL = ShaderModel.Model5_1;

    D3DCompiler _d3dCompiler;

    /// <summary>
    /// Creates a new instance of <see cref="FxcCompiler"/>.
    /// </summary>
    /// <param name="renderer">The renderer which owns the compiler.</param>
    /// <param name="log"></param>
    /// <param name="includePath">The default path for engine/game HLSL include files.</param>
    /// <param name="includeAssembly"></param>
    internal FxcCompiler(DeviceDX11 device, string includePath, Assembly includeAssembly) :
        base(device, includePath, includeAssembly)
    {
        Model = Device.Capabilities.MaxShaderModel.Clamp(MIN_SHADER_MODEL, MAX_SHADER_MODEL);
        _d3dCompiler = D3DCompiler.GetApi();
    }

    protected override void OnDispose(bool immediate)
    {
        _d3dCompiler.Dispose();
    }

    protected override bool Validate(ShaderPass pass, ShaderCompilerContext context, ShaderCodeResult result)
    {
        return true;
    }

    private unsafe ShaderReflection BuildReflection(ShaderCompilerContext context, ID3D10Blob* byteCode)
    {
        Guid guidReflect = ID3D11ShaderReflection.Guid;
        void* ppReflection = null;

        void* ppByteCode = byteCode->GetBufferPointer();
        nuint numBytes = byteCode->GetBufferSize();

        _d3dCompiler.Reflect(ppByteCode, numBytes, &guidReflect, &ppReflection);

        FxcReflection fxcReflection = new FxcReflection((ID3D11ShaderReflection*)ppReflection);
        ShaderReflection result = new ShaderReflection()
        {
            GSInputPrimitive = fxcReflection.Ptr->GetGSInputPrimitive().FromApi(),
        };

        // Get shader resource input bindings
        for (uint i = 0; i < fxcReflection.Desc.BoundResources; i++)
        {
            ShaderInputBindDesc rDesc = new();
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

            result.BoundResources.Add(bindInfo);

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

                    result.ConstantBuffers.Add(bindInfo.Name, cBufferInfo);

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
                            ptrDefault = result.NewPtr(desc.Size);
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

        PopulateReflectionParameters(result, fxcReflection, ShaderIOLayoutType.Input);
        PopulateReflectionParameters(result, fxcReflection, ShaderIOLayoutType.Output);

        fxcReflection.Dispose();
        return result;
    }

    private void PopulateReflectionParameters(ShaderReflection reflection, FxcReflection fxcReflection, ShaderIOLayoutType type)
    {
        uint count = 0;
        List<ShaderParameterInfo> parameters;

        switch (type)
        {
            case ShaderIOLayoutType.Input:
                count = fxcReflection.Desc.InputParameters;
                parameters = reflection.InputParameters;
                break;

            case ShaderIOLayoutType.Output:
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
                case ShaderIOLayoutType.Input:
                    fxcReflection.Ptr->GetInputParameterDesc(i, ref pDesc);
                    break;

                case ShaderIOLayoutType.Output:
                    fxcReflection.Ptr->GetOutputParameterDesc(i, ref pDesc);
                    break;
            }

            parameters.Add(new ShaderParameterInfo()
            {
                ComponentType = (ShaderRegisterType)pDesc.ComponentType,
                Mask = (ShaderComponentMaskFlags)pDesc.Mask,
                ReadWriteMask = (ShaderComponentMaskFlags)pDesc.ReadWriteMask,
                MinPrecision = (ShaderMinPrecision)pDesc.MinPrecision,
                Register = pDesc.Register,
                SemanticIndex = pDesc.SemanticIndex,
                SemanticName = SilkMarshal.PtrToString((nint)pDesc.SemanticName).ToUpper(),
                SemanticNamePtr = pDesc.SemanticName,
                Stream = pDesc.Stream,
                SystemValueType = (ShaderSVType)pDesc.SystemValueType,
            });
        }
    }

    /// <summary>Compiles HLSL source code and outputs the result. Returns true if successful, or false if there were errors.</summary>
    /// <param name="entryPoint"></param>
    /// <param name="type"></param>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    protected override ShaderCodeResult CompileNativeSource(string entryPoint, ShaderType type, ShaderCompilerContext context)
    {
        Encoding encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252); // Ansi codepage
        NativeStringEncoding nativeEncoding = NativeStringEncoding.LPStr;

        ulong numBytes = 0;
        string shaderProfile = Model.ToProfile(type);
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
            return new ShaderCodeResult(reflection, pByteCode, pByteCode->GetBufferSize(), null);
        }

        NativeUtil.ReleasePtr(ref pProcessedSrc);
        NativeUtil.ReleasePtr(ref pErrors);
        EngineUtil.Free(ref pSrc);
        EngineUtil.Free(ref pSourceName);
        SilkMarshal.Free((nint)pEntryPoint);
        SilkMarshal.Free((nint)pTarget);

        return null;
    }

    protected override unsafe void* BuildNativeShader(ShaderPass parent, ShaderType type, void* byteCode, nuint numBytes)
    {
        ID3D10Blob* dx11ByteCode = (ID3D10Blob*)byteCode;
        void* ptrBytecode = dx11ByteCode->GetBufferPointer();
        numBytes = dx11ByteCode->GetBufferSize();
        DeviceDX11 device = Device as DeviceDX11;
        ShaderPassDX11 passDX11 = parent as ShaderPassDX11;

        switch (type)
        {
            case ShaderType.Compute:
                passDX11.InputByteCode = byteCode;

                ID3D11ComputeShader* csShader = null;
                device.Ptr->CreateComputeShader(ptrBytecode, numBytes, null, &csShader);
                return csShader;

            case ShaderType.Vertex:
                passDX11.InputByteCode = byteCode;

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

    public override bool AllowStaticSamplers => false;
}
