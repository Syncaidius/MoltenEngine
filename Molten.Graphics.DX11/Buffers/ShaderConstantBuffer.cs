using Molten.Utility;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System.Text;

namespace Molten.Graphics
{
    internal unsafe class ShaderConstantBuffer : GraphicsBuffer
    {
        internal D3DShaderCBufferFlags Flags;
        internal D3DCBufferType Type;
        internal ShaderConstantVariable[] Variables;
        internal bool DirtyVariables;
        internal Dictionary<string, ShaderConstantVariable> _varLookup;
        internal string BufferName;
        internal int Hash;
        byte* _constData;

        internal ShaderConstantBuffer(Device device, BufferMode flags, 
            ID3D11ShaderReflectionConstantBuffer* srConstBuffer, ref ShaderBufferDesc desc)
            : base(device, flags, BindFlag.BindConstantBuffer, desc.Size)
        {
            _varLookup = new Dictionary<string, ShaderConstantVariable>();
            _constData = (byte*)EngineUtil.Alloc(desc.Size);

            // Read sdescription data
            BufferName = SilkMarshal.PtrToString((nint)desc.Name);
            Flags = (D3DShaderCBufferFlags)desc.UFlags;
            Type = desc.Type;

            string hashString = BufferName;
            uint variableCount = desc.Variables;
            Variables = new ShaderConstantVariable[variableCount];

            // Read all variables from the constant buffer
            for (uint c = 0; c < variableCount; c++)
            {
                ID3D11ShaderReflectionVariable* variable = srConstBuffer->GetVariableByIndex(c);
                ShaderVariableInfo info = new ShaderVariableInfo(variable);
                ShaderConstantVariable sv = GetShaderVariable(info);

                // Throw exception if the variable type is unsupported.
                if (sv == null) // TODO remove this exception!
                    throw new NotSupportedException("Shader pipeline does not support HLSL variables of type: " + info.TypeDesc->Type + " -- " + info.TypeDesc->Class);

                sv.Name = SilkMarshal.PtrToString((nint)info.Desc->Name);
                sv.ByteOffset = info.Desc->StartOffset;

                _varLookup.Add(sv.Name, sv);
                Variables[c] = sv;

                // Append name to hash.
                hashString += sv.Name;
            }

            // Generate hash for comparing constant buffers.
            byte[] hashData = StringHelper.GetBytes(hashString, Encoding.Unicode);
            Hash = HashHelper.ComputeFNV(hashData);
            Description.ByteWidth = desc.Size;
        }

        internal override void PipelineRelease()
        {
            EngineUtil.Free(ref _constData);
            base.PipelineRelease();
        }

        protected override void OnApply(DeviceContext context)
        {
            // Setting data via shader variabls takes precedent. All standard buffer changes (set/append) will be ignored and wiped.
            if (DirtyVariables)
            {
                // Reset variable-specific dirty flag
                DirtyVariables = false;

                // Re-write all data to the variable buffer to maintain byte-ordering.
                foreach(ShaderConstantVariable v in Variables)
                    v.Write(_constData + v.ByteOffset);

                MappedSubresource data = context.MapResource(NativePtr, 0, Map.MapWriteDiscard, 0);
                Buffer.MemoryCopy(_constData, data.PData, data.DepthPitch, Description.ByteWidth);
                context.UnmapResource(NativePtr, 0);
            }
            else
            {
                ApplyChanges(context);
            }
        }

        /// <summary>Figures out what type to use for a shader variable.</summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private unsafe ShaderConstantVariable GetShaderVariable(ShaderVariableInfo vInfo)
        {
            uint columns = vInfo.TypeDesc->Columns;
            uint rows = vInfo.TypeDesc->Rows;
            uint elementCount = vInfo.TypeDesc->Elements;
            uint offset = vInfo.TypeDesc->Offset;

            switch (vInfo.TypeDesc->Class)
            {
                default:
                    if (elementCount > 0)
                    {
                        switch (vInfo.TypeDesc->Type)
                        {
                            case D3DShaderVariableType.D3DSvtInt:
                                return new ScalarArray<int>(this, elementCount);
                            case D3DShaderVariableType.D3DSvtUint:
                                return new ScalarArray<uint>(this, elementCount);
                            case D3DShaderVariableType.D3DSvtFloat:
                                return new ScalarArray<float>(this, elementCount);
                        }
                    }
                    else
                    {
                        switch (vInfo.TypeDesc->Type)
                        {
                            case D3DShaderVariableType.D3DSvtInt:
                                return new ScalarVariable<int>(this, rows, columns);
                            case D3DShaderVariableType.D3DSvtUint:
                                return new ScalarVariable<uint>(this, rows, columns);
                            case D3DShaderVariableType.D3DSvtFloat:
                                return new ScalarVariable<float>(this, rows, columns);
                        }
                    }
                    break;

                case D3DShaderVariableClass.D3DSvcMatrixColumns:
                    if (elementCount > 0)
                    {
                        switch (vInfo.TypeDesc->Type)
                        {
                            case D3DShaderVariableType.D3DSvtFloat:
                                if (columns == 4 && rows == 4)
                                    return new ScalarFloat4x4ArrayVariable(this, elementCount);
                                else if (columns == 3 && rows == 3)
                                    return new ScalarFloat3x3ArrayVariable(this, elementCount);
                                else
                                    return new ScalarMatrixArray<float>(this, rows, columns, elementCount);

                            default:
                                return new ScalarMatrixArray<float>(this, rows, columns, elementCount);
                        }
                    }
                    else
                    {
                        switch (vInfo.TypeDesc->Type)
                        {
                            case D3DShaderVariableType.D3DSvtFloat:
                                if (vInfo.TypeDesc->Columns == 4 && rows == 4)
                                    return new ScalarFloat4x4Variable(this);
                                else if (vInfo.TypeDesc->Columns == 2 && rows == 3)
                                    return new ScalarFloat3x2Variable(this);
                                else if (vInfo.TypeDesc->Columns == 3 && rows == 3)
                                    return new ScalarFloat3x3Variable(this);
                                else
                                    return new ScalarVariable<float>(this, rows, columns);

                            default:
                                return new ScalarVariable<float>(this, rows, columns);
                        }
                    }
            }

            return null;
        }


        public string ConstantBufferName => BufferName;

        internal byte* DataPtr => _constData;
    }
}
