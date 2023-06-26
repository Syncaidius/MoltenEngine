namespace Molten.Graphics
{
    public class ConstantBufferInfo
    {
        public string Name;

        public ConstantBufferType Type;

        public ConstantBufferFlags Flags;

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public uint Size;

        public List<ConstantBufferVariableInfo> Variables { get; } = new List<ConstantBufferVariableInfo>();

        public static GraphicsConstantVariable[] BuildBufferVariables(IConstantBuffer parent, ConstantBufferInfo info)
        {
            uint variableCount = (uint)info.Variables.Count;
            GraphicsConstantVariable[]  result = new GraphicsConstantVariable[variableCount];

            // Read all variables from the constant buffer
            for (int c = 0; c < info.Variables.Count; c++)
            {
                ConstantBufferVariableInfo variable = info.Variables[c];
                GraphicsConstantVariable sv = CreateConstantVariable(parent, variable, variable.Name);

                // Throw exception if the variable type is unsupported.
                if (sv == null) // TODO remove this exception!
                    throw new NotSupportedException("Shader pipeline does not support HLSL variables of type: " + variable.Type.Type + " -- " + variable.Type.Class);

                sv.ByteOffset = variable.StartOffset;

                unsafe
                {
                    if (variable.DefaultValue != null)
                        sv.ValueFromPtr(variable.DefaultValue);
                }

                result[c] = sv;
            }

            return result;
        }

        /// <summary>
        /// Returns the correct <see cref="GraphicsConstantVariable"/> based on the provided <see cref="ConstantBufferVariableInfo"/>.
        /// </summary>
        /// <param name="vInfo"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static unsafe GraphicsConstantVariable CreateConstantVariable(IConstantBuffer parent, ConstantBufferVariableInfo vInfo, string name)
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
                                return new ScalarArray<int>(parent, elementCount, name);
                            case ShaderVariableType.UInt:
                                return new ScalarArray<uint>(parent, elementCount, name);
                            case ShaderVariableType.Float:
                                return new ScalarArray<float>(parent, elementCount, name);
                        }
                    }
                    else
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Int:
                                return new ScalarVariable<int>(parent, rows, columns, name);
                            case ShaderVariableType.UInt:
                                return new ScalarVariable<uint>(parent, rows, columns, name);
                            case ShaderVariableType.Float:
                                return new ScalarVariable<float>(parent, rows, columns, name);
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
                                    return new ScalarFloat4x4ArrayVariable(parent, elementCount, name);
                                else if (columns == 3 && rows == 3)
                                    return new ScalarFloat3x3ArrayVariable(parent, elementCount, name);
                                else
                                    return new ScalarMatrixArray<float>(parent, rows, columns, elementCount, name);

                            default:
                                return new ScalarMatrixArray<float>(parent, rows, columns, elementCount, name);
                        }
                    }
                    else
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Float:
                                if (vInfo.Type.ColumnCount == 4 && rows == 4)
                                    return new ScalarFloat4x4Variable(parent, name);
                                else if (vInfo.Type.ColumnCount == 2 && rows == 3)
                                    return new ScalarFloat3x2Variable(parent, name);
                                else if (vInfo.Type.ColumnCount == 3 && rows == 3)
                                    return new ScalarFloat3x3Variable(parent, name);
                                else
                                    return new ScalarVariable<float>(parent, rows, columns, name);

                            default:
                                return new ScalarVariable<float>(parent, rows, columns, name);
                        }
                    }
            }

            return null;
        }

        public bool hasFlags(ConstantBufferFlags flags)
        {
            return (Flags & flags) == flags;
        }
    }
}
