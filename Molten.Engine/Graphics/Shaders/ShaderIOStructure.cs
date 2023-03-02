
namespace Molten.Graphics
{
    public enum ShaderIOStructureType
    {
        Input = 0,

        Output = 1,
    }

    /// <summary>Represents an automatically generated shader input layout. 
    /// Also generating useful metadata that can be used to validate vertex input at engine level.</summary>
    public unsafe abstract class ShaderIOStructure : EngineObject
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
        /// Creates a new, empty instance of <see cref="ShaderIOStructure"/>.
        /// </summary>
        /// <param name="elementCount"></param>
        protected ShaderIOStructure(uint elementCount)
        {
            Metadata = new InputElementMetadata[elementCount];
            Initialize(elementCount);
        }

        /// <summary>Creates a new instance of <see cref="ShaderIOStructure"/> from reflection info.</summary>
        /// <param name="result">The <see cref="ShaderCodeResult"/> reflection object.</param>
        /// <param name="type">The type of IO structure to build from reflection.param>
        protected ShaderIOStructure(ShaderCodeResult result, ShaderIOStructureType type)
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
                    return;
            }

            uint count = (uint)parameters.Count;
            Metadata = new InputElementMetadata[count];
            Initialize(count);

            for (int i = 0; i < count; i++)
            {
                ShaderParameterInfo pInfo = parameters[i];
                ShaderComponentMaskFlags usageMask = (pInfo.Mask & ShaderComponentMaskFlags.ComponentX);
                usageMask |= (pInfo.Mask & ShaderComponentMaskFlags.ComponentY);
                usageMask |= (pInfo.Mask & ShaderComponentMaskFlags.ComponentZ);
                usageMask |= (pInfo.Mask & ShaderComponentMaskFlags.ComponentW);

                ShaderRegisterType comType = pInfo.ComponentType;
                GraphicsFormat eFormat = GraphicsFormat.Unknown;
                switch (usageMask)
                {
                    case ShaderComponentMaskFlags.ComponentX:
                        if (comType == ShaderRegisterType.UInt32)
                            eFormat = GraphicsFormat.R32_UInt;
                        else if (comType == ShaderRegisterType.SInt32)
                            eFormat = GraphicsFormat.R32_SInt;
                        else if (comType == ShaderRegisterType.Float32)
                            eFormat = GraphicsFormat.R32_Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY:
                        if (comType == ShaderRegisterType.UInt32)
                            eFormat = GraphicsFormat.R32G32_UInt;
                        else if (comType == ShaderRegisterType.SInt32)
                            eFormat = GraphicsFormat.R32G32_SInt;
                        else if (comType == ShaderRegisterType.Float32)
                            eFormat = GraphicsFormat.R32G32_Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY |
                ShaderComponentMaskFlags.ComponentZ:
                        if (comType == ShaderRegisterType.UInt32)
                            eFormat = GraphicsFormat.R32G32B32_UInt;
                        else if (comType == ShaderRegisterType.SInt32)
                            eFormat = GraphicsFormat.R32G32B32_SInt;
                        else if (comType == ShaderRegisterType.Float32)
                            eFormat = GraphicsFormat.R32G32B32_Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY |
                ShaderComponentMaskFlags.ComponentZ | ShaderComponentMaskFlags.ComponentW:
                        if (comType == ShaderRegisterType.UInt32)
                            eFormat = GraphicsFormat.R32G32B32A32_UInt;
                        else if (comType == ShaderRegisterType.SInt32)
                            eFormat = GraphicsFormat.R32G32B32A32_SInt;
                        else if (comType == ShaderRegisterType.Float32)
                            eFormat = GraphicsFormat.R32G32B32A32_Float;
                        break;
                }

                // Store the element
                BuildElement(result, type, pInfo, eFormat, i);
                Metadata[i] = new InputElementMetadata()
                {
                    Name = pInfo.SemanticName,
                    SystemValueType = pInfo.SystemValueType,
                    SemanticIndex = pInfo.SemanticIndex
                };
            }
        }

        protected abstract void Initialize(uint elementCount);

        protected abstract void BuildElement(ShaderCodeResult result,
            ShaderIOStructureType type,
            ShaderParameterInfo pInfo,
            GraphicsFormat format,
            int index);

        public bool IsCompatible(ShaderIOStructure other)
        {
            return IsCompatible(other, 0);
        }

        public bool IsCompatible(ShaderIOStructure other, int startIndex)
        {
            int count = Math.Min(Metadata.Length - startIndex, other.Metadata.Length);
            for(int i = 0; i < count; i++)
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
