using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Molten.Utility;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

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

        internal ShaderConstantBuffer(DeviceDX11 device, BufferMode flags, 
            ID3D11ShaderReflectionConstantBuffer* srConstBuffer, ShaderBufferDesc* desc)
            : base(device, flags, BindFlag.BindConstantBuffer, desc->Size)
        {
            _varLookup = new Dictionary<string, ShaderConstantVariable>();

            // Read sdescription data
            BufferName = desc->Name;
            Flags = (D3DShaderCBufferFlags)desc->UFlags;
            Type = desc->Type;

            string hashString = BufferName;
            uint variableCount = desc->Variables;
            Variables = new ShaderConstantVariable[variableCount];

            // Read all variables from the constant buffer
            for (uint c = 0; c < variableCount; c++)
            {
                ID3D11ShaderReflectionVariable* variable = srConstBuffer->GetVariableByIndex(c);
                ID3D11ShaderReflectionType* t = variable->GetType();

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
            Description.SizeInBytes = srConstBuffer.Description.Size;
        }

        internal override void Refresh(PipeDX11 pipe, PipelineBindSlot<DeviceDX11, PipeDX11> slot)
        {
            // Setting data via shader variabls takes precedent. All standard buffer changes (set/append) will be ignored and wiped.
            if (DirtyVariables)
            {
                // Reset variable-specific dirty flag
                DirtyVariables = false;

                //write updated data into constant buffer
                ResourceStream bufferData;
                MappedSubresource data = pipe.MapResource(Native, 0, Map.MapWriteDiscard, 0, out bufferData);
                {
                    // Re-write all data to the variable buffer to maintain byte-ordering.
                    for (int i = 0; i < Variables.Length; i++)
                    {
                        bufferData.Position = Variables[i].ByteOffset;
                        Variables[i].Write(bufferData);
                    }
                }
                pipe.UnmapResource(Native, 0);
            }
            else
            {
                ApplyChanges(pipe);
            }
        }

        /// <summary>Figures out what type to use for a shader variable.</summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private unsafe ShaderConstantVariable GetShaderVariable(ID3D11ShaderReflectionType* t)
        {
            ShaderTypeDesc* desc = null;
            t->GetDesc(desc);

            uint columns = desc->Columns;
            uint rows = desc->Rows;

            switch (desc->Class)
            {
                default:
                    if (desc->Elements > 0)
                    {
                        switch (desc->Type)
                        {
                            case D3DShaderVariableType.D3DSvtInt:
                                return new ScalarArray<int>(this, desc->Elements);
                            case D3DShaderVariableType.D3DSvtUint:
                                return new ScalarArray<uint>(this, desc->Elements);
                            case D3DShaderVariableType.D3DSvtFloat:
                                return new ScalarArray<float>(this, desc->Elements);
                        }
                    }
                    else
                    {
                        switch (desc->Type)
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
                    if (desc->Elements > 0)
                    {
                        switch (desc->Type)
                        {
                            case D3DShaderVariableType.D3DSvtFloat:
                                if (desc->Columns == 4 && desc->Rows == 4)
                                    return new ScalarFloat4x4ArrayVariable(this, desc->Elements);
                                else if (desc->Columns == 3 && desc->Rows == 3)
                                    return new ScalarFloat3x3ArrayVariable(this, desc->Elements);
                                else
                                    return new ScalarMatrixArray<float>(this, rows, columns, desc->Elements);

                            default:
                                return new ScalarMatrixArray<float>(this, rows, columns, desc->Elements);
                        }
                    }
                    else
                    {
                        switch (desc->Type)
                        {
                            case D3DShaderVariableType.D3DSvtFloat:
                                if (desc->Columns == 4 && desc->Rows == 4)
                                    return new ScalarFloat4x4Variable(this);
                                else if (desc->Columns == 2 && desc->Rows == 3)
                                    return new ScalarFloat3x2Variable(this);
                                else if (desc->Columns == 3 && desc->Rows == 3)
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
