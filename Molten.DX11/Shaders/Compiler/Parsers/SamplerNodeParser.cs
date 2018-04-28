using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class SamplerNodeParser : ShaderNodeParser
    { 
        internal override string[] SupportedNodes => new string[] { "sampler" };

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            if (foundation is ComputeTask)
                return new NodeParseResult(NodeParseResultType.Ignored);

            int slotID = 0;
            StateConditions conditions = StateConditions.None;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                string attName = attribute.Name.ToLower();
                switch (attName)
                {
                    case "slot": // The blend state RT index. Blend states can provide per-render target/surface configuration.
                        if (!int.TryParse(attribute.InnerText, out slotID))
                            InvalidValueMessage(context, attribute, "sampler slot", "integer");
                        break;

                    case "condition":
                        if (!Enum.TryParse(attribute.InnerText, true, out conditions))
                            InvalidEnumMessage<StateConditions>(context, attribute, "sampler condition");
                        break;
                }
            }

            // Retrieve existing state
            ShaderSampler sampler = null;
            if (slotID < foundation.Samplers.Length)
                sampler = foundation.Samplers[slotID][conditions];
            
            bool existingSampler = sampler != null;
            sampler = sampler ?? new ShaderSampler();

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName)
                {
                    case "addressu":
                        if (Enum.TryParse(child.InnerText, true, out SamplerAddressMode uMode))
                            sampler.AddressU = uMode;
                        else
                            InvalidEnumMessage<SamplerAddressMode>(context, child, "U address mode");
                        break;

                    case "addressv":
                        if (Enum.TryParse(child.InnerText, true, out SamplerAddressMode vMode))
                            sampler.AddressU = vMode;
                        else
                            InvalidEnumMessage<SamplerAddressMode>(context, child, "V address mode");
                        break;

                    case "addressw":
                        if (Enum.TryParse(child.InnerText, true, out SamplerAddressMode wMode))
                            sampler.AddressU = wMode;
                        else
                            InvalidEnumMessage<SamplerAddressMode>(context, child, "W address mode");
                        break;

                    case "border": // Border color to use if SharpDX.Direct3D11.TextureAddressMode.Border is specified.
                        sampler.BorderColor = ParseColor4(context, child, true);
                        break;

                    case "comparison":
                        if (Enum.TryParse(child.InnerText, true, out ComparisonMode cMode))
                            sampler.ComparisonFunc = cMode;
                        else
                            InvalidEnumMessage<ComparisonMode>(context, child, "comparison mode");
                        break;

                    case "filter":
                        if (Enum.TryParse(child.InnerText, true, out SamplerFilter filter))
                            sampler.Filter = filter;
                        else
                            InvalidEnumMessage<SamplerFilter>(context, child, "filter");
                        break;

                    case "maxanisotropy": // Max anisotrophy
                        if (int.TryParse(child.InnerText, out int maxAnisotrophy))
                            sampler.MaxAnisotropy = maxAnisotrophy;
                        else
                            InvalidValueMessage(context, child, "max anisotrophy", "integer");
                        break;

                    case "maxmipmaplod": // Max mip-map LoD.
                        if (float.TryParse(child.InnerText, out float maxMipMapLod))
                            sampler.MaxMipMapLod = maxMipMapLod;
                        else
                            InvalidValueMessage(context, child, "maximum mip-map level-of-detail", "floating-point");
                        break;

                    case "minmipmaplod": // Min mip-map LoD.
                        if (float.TryParse(child.InnerText, out float minMipMapLoD))
                            sampler.MaxMipMapLod = minMipMapLoD;
                        else
                            InvalidValueMessage(context, child, "minimum mip-map level-of-detail", "floating-point");
                        break;

                    case "lodbias": // LoD bias.
                        if (float.TryParse(child.InnerText, out float lodBias))
                            sampler.LodBias = lodBias;
                        else
                            InvalidValueMessage(context, child, "level-of-detail (LoD) bias", "floating-point");
                        break;

                    default:
                        UnsupportedTagMessage(context, child);
                        break;
                }
            }

            // Check if an identical state exists (if we don't already have one) before returning the new one.
            if (!existingSampler)
            {
                foreach (ShaderSampler existing in context.Samplers)
                {
                    if (existing.Equals(sampler))
                    {
                        sampler.Dispose();
                        existingSampler = true;
                        sampler = existing;
                        break;
                    }
                }
            }

            // If the defined state still isn't an existing one, add it to the context.
            if (!existingSampler)
                context.Samplers.Add(sampler);

            if (slotID >= foundation.Samplers.Length)
            {
                int oldLength = foundation.Samplers.Length;
                Array.Resize(ref foundation.Samplers, slotID + 1);
                for (int i = oldLength; i < foundation.Samplers.Length; i++)
                    foundation.Samplers[i] = new ShaderStateBank<ShaderSampler>();
            }
            foundation.Samplers[slotID][conditions] = sampler;

            if (foundation is Material material)
            {
                // Apply to existing passes which do not have a rasterizer state yet.
                foreach (MaterialPass p in material.Passes)
                {
                    if (slotID >= p.Samplers.Length)
                    {
                        if (slotID >= p.Samplers.Length)
                        {
                            Array.Resize(ref p.Samplers, slotID + 1);
                            for (int i = slotID; i < p.Samplers.Length; i++)
                                p.Samplers[i] = new ShaderStateBank<ShaderSampler>();
                        }

                        p.Samplers[slotID][conditions] = sampler;
                    }
                    else if (p.Samplers[slotID] == null) // Only overwrite with root sampler if pass does not already have on for the current index.
                    {
                        p.Samplers[slotID][conditions] = sampler;
                    }
                }
            }

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
