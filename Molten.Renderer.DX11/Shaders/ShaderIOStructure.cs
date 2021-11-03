using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    /// <summary>A kind of helper class that automatically generates a shader input layout ready for use,
    /// while also generating useful metadata that can be used to validate vertex input at engine level.</summary>
    public class ShaderIOStructure
    {
        public InputElement[] Elements;
        public int HashKey { get; private set; }

        // Reference: http://takinginitiative.wordpress.com/2011/12/11/directx-1011-basic-shader-reflection-automatic-input-layout-creation/

        /// <summary>Creates a new instance of ShaderInputLayout.</summary>
        /// <param name="shaderRef">The shader reflection instance to extract input layout data from.</param>
        /// <param name="desc"></param>
        public ShaderIOStructure(ShaderReflection shaderRef, ref ShaderDescription desc, ShaderIOStructureType type)
        {
            string signature = "";
            int count = 0;
            switch (type)
            {
                case ShaderIOStructureType.Input:
                    count = desc.InputParameters;
                    break;

                case ShaderIOStructureType.Output:
                    count = desc.OutputParameters;
                    break;
            }

            Elements = new InputElement[count];

            for (int i = 0; i < count; i++)
            {
                ShaderParameterDescription pDesc = new ShaderParameterDescription();

                switch (type)
                {
                    case ShaderIOStructureType.Input:
                        pDesc = shaderRef.GetInputParameterDescription(i);
                        break;

                    case ShaderIOStructureType.Output:
                        pDesc = shaderRef.GetOutputParameterDescription(i);
                        break;
                }

                InputElement el = new InputElement()
                {
                    SemanticName = pDesc.SemanticName.ToUpper(),
                    SemanticIndex = pDesc.SemanticIndex,
                    Slot = 0, // This does not need to be set. A shader has a single layout, 
                    InstanceDataStepRate = 0, // This does not need to be set. The data is set via Context.DrawInstanced + vertex data/layout.
                    AlignedByteOffset = 16 * pDesc.Register,
                    Classification = InputClassification.PerVertexData,
                };

                //A piece of hax to fix some derpy bug: https://github.com/sharpdx/SharpDX/issues/553
                //but bit-shifting to the right by 8bits seems to fix it.
                RegisterComponentMaskFlags usageMask = (pDesc.UsageMask & RegisterComponentMaskFlags.ComponentX);
                usageMask |= (pDesc.UsageMask & RegisterComponentMaskFlags.ComponentY);
                usageMask |= (pDesc.UsageMask & RegisterComponentMaskFlags.ComponentZ);
                usageMask |= (pDesc.UsageMask & RegisterComponentMaskFlags.ComponentW);

                RegisterComponentType comType = pDesc.ComponentType;
                switch (usageMask)
                {
                    case RegisterComponentMaskFlags.ComponentX:
                        if (comType == RegisterComponentType.UInt32)
                            el.Format = Format.R32_UInt;
                        else if (comType == RegisterComponentType.SInt32)
                            el.Format = Format.R32_SInt;
                        else if (comType == RegisterComponentType.Float32)
                            el.Format = Format.R32_Float;
                        break;

                    case RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY:
                        if (comType == RegisterComponentType.UInt32)
                            el.Format = Format.R32G32_UInt;
                        else if (comType == RegisterComponentType.SInt32)
                            el.Format = Format.R32G32_SInt;
                        else if (comType == RegisterComponentType.Float32)
                            el.Format = Format.R32G32_Float;
                        break;

                    case RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY |
                RegisterComponentMaskFlags.ComponentZ:
                        if (comType == RegisterComponentType.UInt32)
                            el.Format = Format.R32G32B32_UInt;
                        else if (comType == RegisterComponentType.SInt32)
                            el.Format = Format.R32G32B32_SInt;
                        else if (comType == RegisterComponentType.Float32)
                            el.Format = Format.R32G32B32_Float;
                        break;

                    case RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY |
                RegisterComponentMaskFlags.ComponentZ | RegisterComponentMaskFlags.ComponentW:
                        if (comType == RegisterComponentType.UInt32)
                            el.Format = Format.R32G32B32A32_UInt;
                        else if (comType == RegisterComponentType.SInt32)
                            el.Format = Format.R32G32B32A32_SInt;
                        else if (comType == RegisterComponentType.Float32)
                            el.Format = Format.R32G32B32A32_Float;
                        break;
                }

                //store the element
                Elements[i] = el;
                signature += $"{el.Format}{el.SemanticIndex}{el.Classification}{el.AlignedByteOffset}{el.SemanticName}";
                byte[] bytes = Encoding.UTF8.GetBytes(signature);
                HashKey = HashHelper.ComputeFNV(bytes);
            }
        }

        /// <summary>Tests to see if the layout of a vertex format matches the layout of the shader input structure.</summary>
        /// <param name="format"></param>
        /// <param name="startElement">The first element within the IO structure at which to start comparing.</param>
        /// <returns></returns>
        public bool IsCompatible(VertexFormat format, int startElement)
        {
            if (startElement >= Elements.Length)
            {
                return false;
            }
            else
            {
                int end = startElement + format.Elements.Length;
                if (end > Elements.Length)
                {
                    return false;
                }
                else
                {
                    // Run comparison test
                    for (int i = startElement; i < end; i++)
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
