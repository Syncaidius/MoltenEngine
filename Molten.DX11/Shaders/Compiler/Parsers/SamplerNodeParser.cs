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
            ShaderSampler sampler = null;

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

                    case "preset":
                        if (Enum.TryParse(attribute.InnerText, true, out SamplerPreset preset))
                            sampler = new ShaderSampler(foundation.Device.SamplerBank.GetPreset(preset));
                        else
                            InvalidEnumMessage<SamplerPreset>(context, attribute, "sampler preset");
                        break;
                }
            }

            // Retrieve existing state if available and create a new one from it to avoid editing the existing one.
            sampler = sampler ?? foundation.Device.SamplerBank.GetPreset(SamplerPreset.Default);

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
                            sampler.AddressV = vMode;
                        else
                            InvalidEnumMessage<SamplerAddressMode>(context, child, "V address mode");
                        break;

                    case "addressw":
                        if (Enum.TryParse(child.InnerText, true, out SamplerAddressMode wMode))
                            sampler.AddressW = wMode;
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


            // Initialize shader state bank for the sampler if needed.
            if (slotID >= foundation.Samplers.Length)
            {
                int oldLength = foundation.Samplers.Length;
                Array.Resize(ref foundation.Samplers, slotID + 1);
                for (int i = oldLength; i < foundation.Samplers.Length; i++)
                    foundation.Samplers[i] = new ShaderStateBank<ShaderSampler>();
            }

            sampler = foundation.Device.SamplerBank.AddOrRetrieveExisting(sampler);
            foundation.Samplers[slotID][conditions] = sampler;
            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
