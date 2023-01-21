using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    /// <summary>Represents an automatically generated shader input layout. 
    /// Also generating useful metadata that can be used to validate vertex input at engine level.</summary>
    internal unsafe class ShaderIOStructure : EngineObject
    {
        internal InputElementData Data;

        // Reference: http://takinginitiative.wordpress.com/2011/12/11/directx-1011-basic-shader-reflection-automatic-input-layout-creation/

        /// <summary>Creates a new instance of ShaderInputLayout.</summary>
        /// <param name="shaderRef">The shader reflection instance to extract input layout data from.</param>
        /// <param name="desc"></param>
        internal ShaderIOStructure(ShaderClassResult result, ShaderIOStructureType type)
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

            int count = parameters.Count;
            Data = new InputElementData((uint)count);

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

                ShaderComponentMaskFlags pDescMask = (ShaderComponentMaskFlags)pDesc.Mask;
                ShaderComponentMaskFlags usageMask = (pDescMask & ShaderComponentMaskFlags.ComponentX);
                usageMask |= (pDescMask & ShaderComponentMaskFlags.ComponentY);
                usageMask |= (pDescMask & ShaderComponentMaskFlags.ComponentZ);
                usageMask |= (pDescMask & ShaderComponentMaskFlags.ComponentW);

                D3DRegisterComponentType comType = (D3DRegisterComponentType)pDesc.ComponentType;
                switch (usageMask)
                {
                    case ShaderComponentMaskFlags.ComponentX:
                        if (comType == D3DRegisterComponentType.D3DRegisterComponentUint32)
                            el.Format = Format.FormatR32Uint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentSint32)
                            el.Format = Format.FormatR32Sint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentFloat32)
                            el.Format = Format.FormatR32Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY:
                        if (comType == D3DRegisterComponentType.D3DRegisterComponentUint32)
                            el.Format = Format.FormatR32G32Uint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentSint32)
                            el.Format = Format.FormatR32G32Sint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentFloat32)
                            el.Format = Format.FormatR32G32Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY |
                ShaderComponentMaskFlags.ComponentZ:
                        if (comType == D3DRegisterComponentType.D3DRegisterComponentUint32)
                            el.Format = Format.FormatR32G32B32Uint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentSint32)
                            el.Format = Format.FormatR32G32B32Sint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentFloat32)
                            el.Format = Format.FormatR32G32B32Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY |
                ShaderComponentMaskFlags.ComponentZ | ShaderComponentMaskFlags.ComponentW:
                        if (comType == D3DRegisterComponentType.D3DRegisterComponentUint32)
                            el.Format = Format.FormatR32G32B32A32Uint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentSint32)
                            el.Format = Format.FormatR32G32B32A32Sint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentFloat32)
                            el.Format = Format.FormatR32G32B32A32Float;
                        break;
                }

                // Store the element
                Data.Elements[i] = el;
                Data.Metadata[i] = new InputElementData.InputElementMetadata()
                {
                    Name = pDesc.SemanticName,
                    SystemValueType = (D3DName)pDesc.SystemValueType
                };
            }
        }

        protected override void OnDispose()
        {
            Data.Dispose();
        }

        /// <summary>Tests to see if the layout of a vertex format matches the layout of the shader input structure.</summary>
        /// <param name="format"></param>
        /// <param name="startElement">The first element within the IO structure at which to start comparing.</param>
        /// <returns></returns>
        public bool IsCompatible(VertexFormat format, uint startElement)
        {
            return Data.IsCompatible(format.Data, startElement);
        }

        public bool IsCompatible(ShaderIOStructure other)
        {
            return Data.IsCompatible(other.Data);
        }
    }

    public enum ShaderIOStructureType
    {
        Input = 0,

        Output = 1,
    }
}
