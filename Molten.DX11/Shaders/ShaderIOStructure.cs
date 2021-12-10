using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    /// <summary>A kind of helper class that automatically generates a shader input layout ready for use,
    /// while also generating useful metadata that can be used to validate vertex input at engine level.</summary>
    public unsafe class ShaderIOStructure
    {
        public InputElementDesc[] Elements;
        public int HashKey { get; private set; }

        // Reference: http://takinginitiative.wordpress.com/2011/12/11/directx-1011-basic-shader-reflection-automatic-input-layout-creation/

        /// <summary>Creates a new instance of ShaderInputLayout.</summary>
        /// <param name="shaderRef">The shader reflection instance to extract input layout data from.</param>
        /// <param name="desc"></param>
        public ShaderIOStructure(ID3D11ShaderReflection* shaderRef, ref ShaderDesc desc, ShaderIOStructureType type)
        {
            string signature = "";
            uint count = 0;
            switch (type)
            {
                case ShaderIOStructureType.Input:
                    count = desc.InputParameters;
                    break;

                case ShaderIOStructureType.Output:
                    count = desc.OutputParameters;
                    break;
            }

            Elements = new InputElementDesc[count];

            for (uint i = 0; i < count; i++)
            {
                SignatureParameterDesc pDesc = new SignatureParameterDesc();

                switch (type)
                {
                    case ShaderIOStructureType.Input:
                        shaderRef->GetInputParameterDesc(i, ref pDesc);
                        break;

                    case ShaderIOStructureType.Output:
                        shaderRef->GetOutputParameterDesc(i, ref pDesc);
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

                //A piece of hax to fix some derpy bug: https://github.com/sharpdx/SharpDX/issues/553
                //but bit-shifting to the right by 8bits seems to fix it.
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
                Elements[i] = el;
                string semName = EngineInterop.StringFromBytes(el.SemanticName);
                signature += $"{el.Format}{el.SemanticIndex}{el.InputSlotClass}{el.AlignedByteOffset}{semName}";
                byte[] bytes = Encoding.UTF8.GetBytes(signature);
                HashKey = HashHelper.ComputeFNV(bytes);
            }
        }

        /// <summary>Tests to see if the layout of a vertex format matches the layout of the shader input structure.</summary>
        /// <param name="format"></param>
        /// <param name="startElement">The first element within the IO structure at which to start comparing.</param>
        /// <returns></returns>
        public bool IsCompatible(VertexFormat format, uint startElement)
        {
            if (startElement >= Elements.Length)
            {
                return false;
            }
            else
            {
                uint end = startElement + (uint)format.Elements.Length;
                if (end > Elements.Length)
                {
                    return false;
                }
                else
                {
                    // Run comparison test
                    for (uint i = startElement; i < end; i++)
                    {
                        if (format.Elements[i].SemanticName != Elements[i].SemanticName)
                            return false;

                        if (format.Elements[i].SemanticIndex != Elements[i].SemanticIndex)
                            return false;
                    }
                }
            }

            return true;
        }

        public bool IsCompatible(ShaderIOStructure other)
        {
            bool valid = true;

            if (other.Elements.Length != Elements.Length)
            {
                valid = false;
            }
            else
            {
                for (int i = 0; i < other.Elements.Length; i++)
                {
                    if (other.Elements[i].SemanticName != Elements[i].SemanticName)
                        valid = false;

                    if (other.Elements[i].SemanticIndex != Elements[i].SemanticIndex)
                        valid = false;

                    if (!valid)
                        break;
                }
            }

            return true;
        }
    }

    public enum ShaderIOStructureType
    {
        Input = 0,

        Output = 1,
    }
}
