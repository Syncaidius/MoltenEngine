using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Molten.Graphics.Shaders;
using Molten.Utility;

namespace Molten.Graphics
{
    internal class ShaderConstantBuffer : GraphicsBuffer
    {
        internal ConstantBufferFlags Flags;
        internal ConstantBufferType Type;
        internal ShaderConstantVariable[] Variables;
        internal bool DirtyVariables;
        internal Dictionary<string, ShaderConstantVariable> _varLookup;
        internal string BufferName;
        internal int Hash;
        internal IShader Parent;

        internal ShaderConstantBuffer(HlslShader parentShader, BufferMode flags, ConstantBuffer desc)
            : base(parentShader.Device, flags, BindFlags.ConstantBuffer, desc.Description.Size)
        {
            Parent = parentShader;
            _varLookup = new Dictionary<string, ShaderConstantVariable>();

            // Read sdescription data
            BufferName = desc.Description.Name;
            Flags = desc.Description.Flags;
            Type = desc.Description.Type;

            string hashString = BufferName;
            int variableCount = desc.Description.VariableCount;
            Variables = new ShaderConstantVariable[variableCount];

            // Read all variables from the constant buffer
            for (int c = 0; c < variableCount; c++)
            {
                ShaderReflectionVariable variable = desc.GetVariable(c);
                ShaderReflectionType t = variable.GetVariableType();

                ShaderConstantVariable sv = GetShaderVariable(t);

                //throw exception if the variable type is unsupported.
                if (sv == null) //TODO remove this exception!
                    throw new NotSupportedException("Shader pipeline does not support HLSL variables of type: " + t.Description.Type + " -- " + t.Description.Class);

                sv.Name = variable.Description.Name;
                sv.ByteOffset = variable.Description.StartOffset;

                _varLookup.Add(sv.Name, sv);
                Variables[c] = sv;

                // Append name to hash.
                hashString += sv.Name;
            }

            // Generate hash for comparing constant buffers.
            byte[] hashData = StringHelper.GetBytes(hashString, Encoding.Unicode);
            Hash = HashHelper.ComputeFNV(hashData);
            Description.SizeInBytes = desc.Description.Size;
        }

        internal override void Refresh(GraphicsPipe pipe, PipelineBindSlot slot)
        {
            // Setting data via shader variabls takes precedent. All standard buffer changes (set/append) will be ignored and wiped.
            if (DirtyVariables)
            {
                // Reset variable-specific dirty flag
                DirtyVariables = false;

                //write updated data into constant buffer
                DataStream bufferData;
                DataBox data = pipe.Context.MapSubresource(_buffer, MapMode.WriteDiscard, MapFlags.None, out bufferData);
                {
                    // Re-write all data to the variable buffer to maintain byte-ordering.
                    for (int i = 0; i < Variables.Length; i++)
                    {
                        bufferData.Position = Variables[i].ByteOffset;
                        Variables[i].Write(bufferData);
                    }
                }
                pipe.Context.UnmapSubresource(_buffer, 0);
            }
            else
            {
                ApplyChanges(pipe);
            }
        }

        /// <summary>Figures out what type to use for a shader variable.</summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private ShaderConstantVariable GetShaderVariable(ShaderReflectionType t)
        {
            ShaderTypeDescription desc = t.Description;
            int columns = desc.ColumnCount;
            int rows = desc.RowCount;

            switch (desc.Class)
            {
                default:
                    if (desc.ElementCount > 0)
                    {
                        switch (desc.Type)
                        {
                            case ShaderVariableType.Int:
                                return new ScalarArray<int>(this, desc.ElementCount);
                            case ShaderVariableType.UInt:
                                return new ScalarArray<uint>(this, desc.ElementCount);
                            case ShaderVariableType.Float:
                                return new ScalarArray<float>(this, desc.ElementCount);
                        }
                    }
                    else
                    {
                        switch (desc.Type)
                        {
                            case ShaderVariableType.Int:
                                return new ScalarVariable<int>(this, rows, columns);
                            case ShaderVariableType.UInt:
                                return new ScalarVariable<uint>(this, rows, columns);
                            case ShaderVariableType.Float:
                                return new ScalarVariable<float>(this, rows, columns);
                        }
                    }
                    break;

                case ShaderVariableClass.MatrixColumns:
                    if (desc.ElementCount > 0)
                    {
                        switch (desc.Type)
                        {
                            case ShaderVariableType.Float:
                                if (desc.ColumnCount == 4 && desc.RowCount == 4)
                                    return new ScalarFloat4x4ArrayVariable(this, desc.ElementCount);
                                else if (desc.ColumnCount == 3 && desc.RowCount == 3)
                                    return new ScalarFloat3x3ArrayVariable(this, desc.ElementCount);
                                else
                                    return new ScalarMatrixArray<float>(this, rows, columns, desc.ElementCount);

                            default:
                                return new ScalarMatrixArray<float>(this, rows, columns, desc.ElementCount);
                        }
                    }
                    else
                    {
                        switch (desc.Type)
                        {
                            case ShaderVariableType.Float:
                                if (desc.ColumnCount == 4 && desc.RowCount == 4)
                                    return new ScalarFloat4x4Variable(this);
                                else if (desc.ColumnCount == 2 && desc.RowCount == 3)
                                    return new ScalarFloat3x2Variable(this);
                                else if (desc.ColumnCount == 3 && desc.RowCount == 3)
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

        /// <summary>Gets or sets a shader variable within the constant buffer.</summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public object this[string variable]
        {
            set
            {
                //if the shader is invalid, skip applying data
                //if (!IsValid)
                //    return;

                ShaderConstantVariable varInstance = null;

                if (_varLookup.TryGetValue(variable, out varInstance))
                    _varLookup[variable].Value = value;
            }

            get
            {
                //if the shader is invalid, skip applying data
                //if (!IsValid)
                //    return null;

                ShaderConstantVariable varInstance = null;

                if (_varLookup.TryGetValue(variable, out varInstance))
                    return _varLookup[variable].Value;
                else
                    return null;
            }
        }

        public string ConstantBufferName
        {
            get { return BufferName; }
        }
    }
}
