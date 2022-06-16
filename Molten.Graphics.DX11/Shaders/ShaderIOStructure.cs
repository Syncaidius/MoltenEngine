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
        internal ShaderIOStructure(FxcCompileResult result, ShaderIOStructureType type)
        {
            uint count = 0;
            switch (type)
            {
                case ShaderIOStructureType.Input:
                    count = result.Reflection.Desc.InputParameters;
                    break;

                case ShaderIOStructureType.Output:
                    count = result.Reflection.Desc.OutputParameters;
                    break;
            }

            Data = new InputElementData(count);

            for (uint i = 0; i < count; i++)
            {
                SignatureParameterDesc pDesc = new SignatureParameterDesc();

                switch (type)
                {
                    case ShaderIOStructureType.Input:
                        result.Reflection.Ptr->GetInputParameterDesc(i, ref pDesc);
                        break;

                    case ShaderIOStructureType.Output:
                        result.Reflection.Ptr->GetOutputParameterDesc(i, ref pDesc);
                        break;
                }

                InputElementDesc el = new InputElementDesc()
                {
                    SemanticName = pDesc.SemanticName,
                    SemanticIndex = pDesc.SemanticIndex,
                    InputSlot = 0, // This does not need to be set. A shader has a single layout, 
                    InstanceDataStepRate = 0, // This does not need to be set. The data is set via Context.DrawInstanced + vertex data/layout.
                    AlignedByteOffset = 16 * pDesc.Register,
                    InputSlotClass = InputClassification.InputPerVertexData,
                };

                RegisterComponentMaskFlags pDescMask = (RegisterComponentMaskFlags)pDesc.Mask;
                RegisterComponentMaskFlags usageMask = (pDescMask & RegisterComponentMaskFlags.ComponentX);
                usageMask |= (pDescMask & RegisterComponentMaskFlags.ComponentY);
                usageMask |= (pDescMask & RegisterComponentMaskFlags.ComponentZ);
                usageMask |= (pDescMask & RegisterComponentMaskFlags.ComponentW);

                D3DRegisterComponentType comType = pDesc.ComponentType;
                switch (usageMask)
                {
                    case RegisterComponentMaskFlags.ComponentX:
                        if (comType == D3DRegisterComponentType.D3DRegisterComponentUint32)
                            el.Format = Format.FormatR32Uint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentSint32)
                            el.Format = Format.FormatR32Sint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentFloat32)
                            el.Format = Format.FormatR32Float;
                        break;

                    case RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY:
                        if (comType == D3DRegisterComponentType.D3DRegisterComponentUint32)
                            el.Format = Format.FormatR32G32Uint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentSint32)
                            el.Format = Format.FormatR32G32Sint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentFloat32)
                            el.Format = Format.FormatR32G32Float;
                        break;

                    case RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY |
                RegisterComponentMaskFlags.ComponentZ:
                        if (comType == D3DRegisterComponentType.D3DRegisterComponentUint32)
                            el.Format = Format.FormatR32G32B32Uint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentSint32)
                            el.Format = Format.FormatR32G32B32Sint;
                        else if (comType == D3DRegisterComponentType.D3DRegisterComponentFloat32)
                            el.Format = Format.FormatR32G32B32Float;
                        break;

                    case RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY |
                RegisterComponentMaskFlags.ComponentZ | RegisterComponentMaskFlags.ComponentW:
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
                    Name = SilkMarshal.PtrToString((nint)pDesc.SemanticName),
                    SystemValueType = pDesc.SystemValueType
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
