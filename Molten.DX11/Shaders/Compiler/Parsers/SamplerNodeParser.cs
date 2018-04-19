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
        static string[] _colorDelimiters = new string[] { ",", " " };

        internal override string[] SupportedNodes => new string[] { "sampler" };

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            if (foundation is ComputeTask)
                return new NodeParseResult(NodeParseResultType.Ignored);

            int sIndex = 0;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                string attName = attribute.Name.ToLower();
                switch (attName)
                {
                    case "index": // The blend state RT index. Blend states can provide per-render target/surface configuration.
                        int.TryParse(attribute.InnerText, out sIndex);
                        break;
                }
            }

            // Retrieve existing state
            ShaderSampler sampler = null;
            switch (foundation)
            {
                case Material material:
                    if(sIndex < material.Samplers.Length)
                        sampler = material.Samplers[sIndex];
                    break;

                case MaterialPass pass:
                    if(sIndex < pass.Samplers.Length)
                        sampler = pass.Samplers[sIndex];
                    break;
            }

            
            bool existingState = sampler != null;
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
                        string[] vals = child.InnerText.Split(_colorDelimiters, StringSplitOptions.RemoveEmptyEntries);
                        Color col = Color.Black;
                        int maxVals = Math.Min(4, vals.Length);
                        for (int i = 0; i < maxVals; i++)
                        {
                            if (byte.TryParse(vals[i], out byte cVal))
                                col[i] = cVal;
                            else
                                context.Messages.Add($"Invalid sampler border color component '{vals[i]}'. A maximum of 4 space-separated values is allowed, each between 0 and 255.");
                        }

                        sampler.BorderColor = col.ToColor4();
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
                }
            }

            // Check if an identical state exists (if we don't already have one) before returning the new one.
            if (!existingState)
            {
                foreach (ShaderSampler existing in context.Samplers)
                {
                    if (existing.Equals(sampler))
                    {
                        sampler.Dispose();
                        existingState = true;
                        sampler = existing;
                        break;
                    }
                }
            }

            // If the defined state still isn't an existing one, add it to the context.
            if (!existingState)
                context.Samplers.Add(sampler);

            switch (foundation)
            {
                case Material material:
                    if (sIndex >= material.Samplers.Length)
                        Array.Resize(ref material.Samplers, sIndex + 1);

                    material.Samplers[sIndex] = sampler;

                    // Apply to existing passes which do not have a rasterizer state yet.
                    foreach (MaterialPass p in material.Passes)
                    {
                        if (sIndex >= p.Samplers.Length)
                        {
                            if (sIndex >= p.Samplers.Length)
                                Array.Resize(ref p.Samplers, sIndex + 1);

                            p.Samplers[sIndex] = sampler;
                        }
                        else if (p.Samplers[sIndex] == null) // Only overwrite with root sampler if pass does not already have on for the current index.
                        {
                            p.Samplers[sIndex] = sampler;
                        }
                    }
                    break;

                case MaterialPass pass:
                    if (sIndex >= pass.Samplers.Length)
                        Array.Resize(ref pass.Samplers, sIndex + 1);
                    pass.Samplers[sIndex] = sampler;
                    break;
            }

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
