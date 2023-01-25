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

        internal ShaderConstantBuffer(DeviceDX11 device, BufferMode flags, ConstantBufferInfo desc)
            : base(device, flags, BindFlag.ConstantBuffer, desc.Size)
        {
            _varLookup = new Dictionary<string, ShaderConstantVariable>();
            _constData = (byte*)EngineUtil.Alloc(desc.Size);

            // Read sdescription data
            BufferName = desc.Name;
            Flags = (D3DShaderCBufferFlags)desc.Flags;
            Type = (D3DCBufferType)desc.Type;

            string hashString = BufferName;
            uint variableCount = (uint)desc.Variables.Count;
            Variables = new ShaderConstantVariable[variableCount];

            // Read all variables from the constant buffer
            for (int c = 0; c < desc.Variables.Count; c++)
            {
                ConstantBufferVariableInfo variable = desc.Variables[c];
                ShaderConstantVariable sv = GetShaderVariable(variable);

                // Throw exception if the variable type is unsupported.
                if (sv == null) // TODO remove this exception!
                    throw new NotSupportedException("Shader pipeline does not support HLSL variables of type: " + variable.Type.Type + " -- " + variable.Type.Class);

                sv.Name = variable.Name;
                sv.ByteOffset = variable.StartOffset;

                if(variable.DefaultValue != null)
                    sv.ValueFromPtr(variable.DefaultValue);

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

        public override void GraphicsRelease()
        {
            if(_constData != null)
                EngineUtil.Free(ref _constData);
            base.GraphicsRelease();
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            CommandQueueDX11 dx11Cmd = cmd as CommandQueueDX11;

            // Setting data via shader variabls takes precedent. All standard buffer changes (set/append) will be ignored and wiped.
            if (DirtyVariables)
            {
                // Reset variable-specific dirty flag
                DirtyVariables = false;

                // Re-write all data to the variable buffer to maintain byte-ordering.
                foreach(ShaderConstantVariable v in Variables)
                    v.Write(_constData + v.ByteOffset);

                MappedSubresource data = dx11Cmd.MapResource(NativePtr, 0, Map.WriteDiscard, 0);
                Buffer.MemoryCopy(_constData, data.PData, data.DepthPitch, Description.ByteWidth);
                dx11Cmd.UnmapResource(NativePtr, 0);
            }
            else
            {
                ApplyChanges(cmd);
            }
        }

        /// <summary>Figures out what type to use for a shader variable.</summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private unsafe ShaderConstantVariable GetShaderVariable(ConstantBufferVariableInfo vInfo)
        {
            uint columns = vInfo.Type.ColumnCount;
            uint rows = vInfo.Type.RowCount;
            uint elementCount = vInfo.Type.Elements;
            uint offset = vInfo.Type.Offset;

            switch (vInfo.Type.Class)
            {
                default:
                    if (elementCount > 0)
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Int:
                                return new ScalarArray<int>(this, elementCount);
                            case ShaderVariableType.Uint:
                                return new ScalarArray<uint>(this, elementCount);
                            case ShaderVariableType.Float:
                                return new ScalarArray<float>(this, elementCount);
                        }
                    }
                    else
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Int:
                                return new ScalarVariable<int>(this, rows, columns);
                            case ShaderVariableType.Uint:
                                return new ScalarVariable<uint>(this, rows, columns);
                            case ShaderVariableType.Float:
                                return new ScalarVariable<float>(this, rows, columns);
                        }
                    }
                    break;

                case ShaderVariableClass.MatrixColumns:
                    if (elementCount > 0)
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Float:
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
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Float:
                                if (vInfo.Type.ColumnCount == 4 && rows == 4)
                                    return new ScalarFloat4x4Variable(this);
                                else if (vInfo.Type.ColumnCount == 2 && rows == 3)
                                    return new ScalarFloat3x2Variable(this);
                                else if (vInfo.Type.ColumnCount == 3 && rows == 3)
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
