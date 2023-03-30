using System.Text;
using Molten.IO;
using Molten.Utility;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class ShaderConstantBuffer : BufferDX11, IConstantBuffer
    {
        internal D3DCBufferType Type;
        internal ShaderConstantVariable[] Variables;
        internal bool DirtyVariables;
        internal Dictionary<string, ShaderConstantVariable> _varLookup;
        internal int Hash;
        byte* _constData;

        internal ShaderConstantBuffer(DeviceDX11 device, ConstantBufferInfo desc)
            : base(device, GraphicsBufferType.Constant,
                  GraphicsResourceFlags.NoShaderAccess | GraphicsResourceFlags.CpuWrite, 
                  BindFlag.ConstantBuffer, 1, desc.Size)
        {
            _varLookup = new Dictionary<string, ShaderConstantVariable>();
            _constData = (byte*)EngineUtil.Alloc(desc.Size);

            // Read sdescription data
            BufferName = desc.Name;
            Type = (D3DCBufferType)desc.Type;

            string hashString = BufferName;
            uint variableCount = (uint)desc.Variables.Count;
            Variables = new ShaderConstantVariable[variableCount];

            // Read all variables from the constant buffer
            for (int c = 0; c < desc.Variables.Count; c++)
            {
                ConstantBufferVariableInfo variable = desc.Variables[c];
                ShaderConstantVariable sv = GetVariable(variable, variable.Name);

                // Throw exception if the variable type is unsupported.
                if (sv == null) // TODO remove this exception!
                    throw new NotSupportedException("Shader pipeline does not support HLSL variables of type: " + variable.Type.Type + " -- " + variable.Type.Class);

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
            Desc.ByteWidth = desc.Size;
        }

        public override void GraphicsRelease()
        {
            EngineUtil.Free(ref _constData);
            base.GraphicsRelease();
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            CommandQueueDX11 dx11Cmd = cmd as CommandQueueDX11;

            // Setting data via shader variabls takes precedent. All standard buffer changes (set/append) will be ignored and wiped.
            if (DirtyVariables)
            {
                DirtyVariables = false;

                // Re-write all data to the variable buffer to maintain byte-ordering.
                foreach(ShaderConstantVariable v in Variables)
                    v.Write(_constData + v.ByteOffset);

                using (GraphicsStream stream = dx11Cmd.MapResource(this, 0, 0, GraphicsMapType.Discard))
                    stream.WriteRange(_constData, Desc.ByteWidth);
            }
            else
            {
                ApplyChanges(cmd);
            }
        }

        /// <summary>Figures out what type to use for a shader variable.</summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private unsafe ShaderConstantVariable GetVariable(ConstantBufferVariableInfo vInfo, string name)
        {
            uint columns = vInfo.Type.ColumnCount;
            uint rows = vInfo.Type.RowCount;
            uint elementCount = vInfo.Type.Elements;

            switch (vInfo.Type.Class)
            {
                default:
                    if (elementCount > 0)
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Int:
                                return new ScalarArray<int>(this, elementCount, name);
                            case ShaderVariableType.Uint:
                                return new ScalarArray<uint>(this, elementCount, name);
                            case ShaderVariableType.Float:
                                return new ScalarArray<float>(this, elementCount, name);
                        }
                    }
                    else
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Int:
                                return new ScalarVariable<int>(this, rows, columns, name);
                            case ShaderVariableType.Uint:
                                return new ScalarVariable<uint>(this, rows, columns, name);
                            case ShaderVariableType.Float:
                                return new ScalarVariable<float>(this, rows, columns, name);
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
                                    return new ScalarFloat4x4ArrayVariable(this, elementCount, name);
                                else if (columns == 3 && rows == 3)
                                    return new ScalarFloat3x3ArrayVariable(this, elementCount, name);
                                else
                                    return new ScalarMatrixArray<float>(this, rows, columns, elementCount, name);

                            default:
                                return new ScalarMatrixArray<float>(this, rows, columns, elementCount, name);
                        }
                    }
                    else
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Float:
                                if (vInfo.Type.ColumnCount == 4 && rows == 4)
                                    return new ScalarFloat4x4Variable(this, name);
                                else if (vInfo.Type.ColumnCount == 2 && rows == 3)
                                    return new ScalarFloat3x2Variable(this, name);
                                else if (vInfo.Type.ColumnCount == 3 && rows == 3)
                                    return new ScalarFloat3x3Variable(this, name);
                                else
                                    return new ScalarVariable<float>(this, rows, columns, name);

                            default:
                                return new ScalarVariable<float>(this, rows, columns, name);
                        }
                    }
            }

            return null;
        }


        public string BufferName { get; }

        internal byte* DataPtr => _constData;
    }
}
