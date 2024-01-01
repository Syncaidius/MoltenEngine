namespace Molten.Graphics
{
    public enum ShaderIOLayoutType
    {
        Input = 0,

        Output = 1,
    }

    /// <summary>Represents an automatically generated shader input layout. 
    /// Also generating useful metadata that can be used to validate vertex input at engine level.</summary>
    public unsafe abstract class ShaderIOLayout : EngineObject
    {
        public struct InputElementMetadata
        {
            public string Name;

            public ShaderSVType SystemValueType;

            public uint SemanticIndex;
        }

        /// <summary>
        /// Contains extra/helper information about input elements
        /// </summary>
        public InputElementMetadata[] Metadata { get; }


        // Reference: http://takinginitiative.wordpress.com/2011/12/11/directx-1011-basic-shader-reflection-automatic-input-layout-creation/

        /// <summary>
        /// Creates a new, empty instance of <see cref="ShaderIOLayout"/>.
        /// </summary>
        /// <param name="elementCount"></param>
        protected ShaderIOLayout(uint elementCount)
        {
            Metadata = new InputElementMetadata[elementCount];
            Initialize(elementCount);
        }

        /// <summary>Creates a new instance of <see cref="ShaderIOLayout"/> from reflection info.</summary>
        /// <param name="result">The <see cref="ShaderCodeResult"/> reflection object.</param>
        /// <param name="type">The type of IO structure to build from reflection.param>
        protected ShaderIOLayout(ShaderCodeResult result, ShaderType sType, ShaderIOLayoutType type)
        {
            List<ShaderParameterInfo> parameters;

            switch (type)
            {
                case ShaderIOLayoutType.Input:
                    parameters = result.Reflection.InputParameters;
                    break;

                case ShaderIOLayoutType.Output:
                    parameters = result.Reflection.OutputParameters;
                    break;

                default:
                    return;
            }

            uint count = (uint)parameters.Count;
            bool isVertex = sType == ShaderType.Vertex;
            Metadata = new InputElementMetadata[count];
            Initialize(isVertex ? count : 0);

            for (int i = 0; i < count; i++)
            {
                ShaderParameterInfo pInfo = parameters[i];

                // Store the element
                if (isVertex)
                {
                    GraphicsFormat eFormat = GetFormatFromMask(pInfo.Mask, pInfo.ComponentType);
                    BuildVertexElement(result, type, pInfo, eFormat, i);
                }

                Metadata[i] = new InputElementMetadata()
                {
                    Name = pInfo.SemanticName,
                    SystemValueType = pInfo.SystemValueType,
                    SemanticIndex = pInfo.SemanticIndex
                };
            }
        }

        private GraphicsFormat GetFormatFromMask(ShaderComponentMaskFlags usageMask, ShaderRegisterType comType)
        {
            uint regSize = 8;
            string regType = "";
            string formatName = "";

            // Get format type
            switch (comType)
            {
                case ShaderRegisterType.SInt8:
                case ShaderRegisterType.SInt16:
                case ShaderRegisterType.SInt32:
                case ShaderRegisterType.SInt64:
                    regType = "Int";
                    break;

                case ShaderRegisterType.UInt8:
                case ShaderRegisterType.UInt16:
                case ShaderRegisterType.UInt32:
                case ShaderRegisterType.UInt64:
                    regType = "UInt";
                    break;

                case ShaderRegisterType.Float16:
                case ShaderRegisterType.Float32:
                case ShaderRegisterType.Float64:
                    regType = "Float";
                    break;
            }

            // Get format component size
            switch (comType)
            {
                case ShaderRegisterType.SInt8:
                case ShaderRegisterType.UInt8:
                    regSize = 8;
                    break;

                case ShaderRegisterType.SInt16:
                case ShaderRegisterType.UInt16:
                case ShaderRegisterType.Float16:
                    regSize = 16;
                    break;

                case ShaderRegisterType.SInt32:
                case ShaderRegisterType.UInt32:
                case ShaderRegisterType.Float32:
                    regSize = 32;
                    break; ;

                case ShaderRegisterType.SInt64:
                case ShaderRegisterType.UInt64:
                case ShaderRegisterType.Float64:
                    regSize = 64;
                    break;
            }

            // Build the format string based on the provided component mask flags.
            switch (usageMask)
            {
                case ShaderComponentMaskFlags.ComponentX:
                    formatName = $"R{regSize}_{regType}";
                    break;

                case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY:
                    formatName = $"R{regSize}G{regSize}_{regType}";
                    break;

                case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY | ShaderComponentMaskFlags.ComponentZ:
                    formatName = $"R{regSize}G{regSize}B{regSize}_{regType}";
                    break;

                case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY | ShaderComponentMaskFlags.ComponentZ | ShaderComponentMaskFlags.ComponentW:
                    formatName = $"R{regSize}G{regSize}B{regSize}A{regSize}_{regType}";
                    break;
            }

            if (Enum.TryParse(formatName, true, out GraphicsFormat format))
                return format;
            else
                return GraphicsFormat.Unknown;
        }

        protected abstract void Initialize(uint vertexElementCount);

        protected abstract void BuildVertexElement(ShaderCodeResult result,
            ShaderIOLayoutType type,
            ShaderParameterInfo pInfo,
            GraphicsFormat format,
            int index);

        public bool IsCompatible(ShaderIOLayout other)
        {
            return IsCompatible(other, 0);
        }

        public bool IsCompatible(ShaderIOLayout other, int startIndex)
        {
            int count = Math.Min(Metadata.Length - startIndex, other.Metadata.Length);
            for (int i = 0; i < count; i++)
            {
                int selfIndex = i + startIndex;
                if (other.Metadata[i].Name != Metadata[selfIndex].Name ||
                            other.Metadata[i].SemanticIndex != Metadata[selfIndex].SemanticIndex)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
