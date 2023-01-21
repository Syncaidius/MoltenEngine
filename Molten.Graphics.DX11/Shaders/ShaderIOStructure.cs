using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    /// <summary>Represents an automatically generated shader input layout. 
    /// Also generating useful metadata that can be used to validate vertex input at engine level.</summary>
    internal unsafe class ShaderIOStructure : EngineObject
    {
        internal struct InputElementMetadata
        {
            public string Name;

            public D3DName SystemValueType;
        }

        internal InputElementDesc[] Elements { get; }

        /// <summary>
        /// Contains extra/helper information about input elements
        /// </summary>
        internal InputElementMetadata[] Metadata { get; }

        // Reference: http://takinginitiative.wordpress.com/2011/12/11/directx-1011-basic-shader-reflection-automatic-input-layout-creation/

        /// <summary>
        /// Creates a new, empty instance of <see cref="ShaderIOStructure"/>.
        /// </summary>
        /// <param name="elementCount"></param>
        internal ShaderIOStructure(uint elementCount)
        {
            Elements = new InputElementDesc[elementCount];
            Metadata = new InputElementMetadata[elementCount];
        }

        /// <summary>Creates a new instance of <see cref="ShaderIOStructure"/> from reflection info.</summary>
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
            Elements = new InputElementDesc[count];
            Metadata = new InputElementMetadata[count];

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
                Elements[i] = el;
                Metadata[i] = new InputElementMetadata()
                {
                    Name = pDesc.SemanticName,
                    SystemValueType = (D3DName)pDesc.SystemValueType
                };
            }
        }

        public bool IsCompatible(ShaderIOStructure other)
        {
            return IsCompatible(other, 0);
        }

        public bool IsCompatible(ShaderIOStructure other, uint startIndex)
        {
            if (startIndex >= Elements.Length)
            {
                return false;
            }
            else
            {
                uint endIndex = startIndex + (uint)other.Elements.Length;
                if (endIndex > Elements.Length)
                {
                    // If the remaining elements are system values (SV_ prefix), allow them.
                    for (int i = Elements.Length; i < endIndex; i++)
                    {
                        if (other.Metadata[i].SystemValueType == D3DName.D3DNameUndefined)
                            return false;
                    }
                }
                else
                {
                    uint oi = 0;
                    for (uint i = startIndex; i < endIndex; i++)
                    {
                        if (other.Metadata[oi].Name != Metadata[i].Name ||
                            other.Elements[oi].SemanticIndex != Elements[i].SemanticIndex)
                            return false;

                        oi++;
                    }
                }
            }

            return true;
        }

        protected override void OnDispose()
        {
            // Dispose of element string pointers, since they were statically-allocated by Silk.NET
            for (uint i = 0; i < Elements.Length; i++)
                SilkMarshal.Free((nint)Elements[i].SemanticName);
        }

        /// <summary>Tests to see if the layout of a vertex format matches the layout of the shader input structure.</summary>
        /// <param name="format"></param>
        /// <param name="startElement">The first element within the IO structure at which to start comparing.</param>
        /// <returns></returns>
        public bool IsCompatible(VertexFormat format, uint startElement)
        {
            return IsCompatible(format.Structure, startElement);
        }
    }

    public enum ShaderIOStructureType
    {
        Input = 0,

        Output = 1,
    }
}
