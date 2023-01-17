using System.Xml;

namespace Molten.Graphics
{
    internal class SamplerNodeParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Sampler;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            ShaderSampler sampler = null;
            SamplerPreset preset = SamplerPreset.Default;

            if (node.ValueType == ShaderHeaderValueType.Preset)
            {
                if (!Enum.TryParse(node.Value, true, out preset))
                    InvalidEnumMessage<SamplerPreset>(context, (node.Name, node.Value), "sampler preset");
            }

            // Retrieve existing state if available and create a new one from it to avoid editing the existing one.
            sampler = new ShaderSampler(foundation.NativeDevice, foundation.NativeDevice.SamplerBank.GetPreset(preset));

            foreach ((string Name, string Value) c in node.ChildValues)
            {
                string nodeName = c.Name.ToLower();
                switch (nodeName)
                {
                    case "addressu":
                        if (Enum.TryParse(c.Value, true, out SamplerAddressMode uMode))
                            sampler.AddressU = uMode;
                        else
                            InvalidEnumMessage<SamplerAddressMode>(context, c, "U address mode");
                        break;

                    case "addressv":
                        if (Enum.TryParse(c.Value, true, out SamplerAddressMode vMode))
                            sampler.AddressV = vMode;
                        else
                            InvalidEnumMessage<SamplerAddressMode>(context, c, "V address mode");
                        break;

                    case "addressw":
                        if (Enum.TryParse(c.Value, true, out SamplerAddressMode wMode))
                            sampler.AddressW = wMode;
                        else
                            InvalidEnumMessage<SamplerAddressMode>(context, c, "W address mode");
                        break;

                    case "border": // Border color to use if SharpDX.Direct3D11.TextureAddressMode.Border is specified.
                        sampler.BorderColor = ParseColor4(context, c.Value, true);
                        break;

                    case "comparison":
                        if (Enum.TryParse(c.Value, true, out ComparisonMode cMode))
                            sampler.ComparisonFunc = cMode;
                        else
                            InvalidEnumMessage<ComparisonMode>(context, c, "comparison mode");
                        break;

                    case "filter":
                        if (Enum.TryParse(c.Value, true, out SamplerFilter filter))
                            sampler.FilterMode = filter;
                        else
                            InvalidEnumMessage<SamplerFilter>(context, c, "filter");
                        break;

                    case "maxanisotropy": // Max anisotrophy
                        if (uint.TryParse(c.Value, out uint maxAnisotrophy))
                            sampler.MaxAnisotropy = maxAnisotrophy;
                        else
                            InvalidValueMessage(context, c, "max anisotrophy", "integer");
                        break;

                    case "maxmipmaplod": // Max mip-map LoD.
                        if (float.TryParse(c.Value, out float maxMipMapLod))
                            sampler.MaxMipMapLod = maxMipMapLod;
                        else
                            InvalidValueMessage(context, c, "maximum mip-map level-of-detail", "floating-point");
                        break;

                    case "minmipmaplod": // Min mip-map LoD.
                        if (float.TryParse(c.Value, out float minMipMapLoD))
                            sampler.MaxMipMapLod = minMipMapLoD;
                        else
                            InvalidValueMessage(context, c, "minimum mip-map level-of-detail", "floating-point");
                        break;

                    case "lodbias": // LoD bias.
                        if (float.TryParse(c.Value, out float lodBias))
                            sampler.LodBias = lodBias;
                        else
                            InvalidValueMessage(context, c, "level-of-detail (LoD) bias", "floating-point");
                        break;

                    default:
                        UnsupportedTagMessage(context, node.Name, c);
                        break;
                }
            }


            // Initialize shader state bank for the sampler if needed.
            if (node.SlotID >= foundation.Samplers.Length)
            {
                int oldLength = foundation.Samplers.Length;
                Array.Resize(ref foundation.Samplers, node.SlotID + 1);
                for (int i = oldLength; i < foundation.Samplers.Length; i++)
                    foundation.Samplers[i] = new ShaderStateBank<ShaderSampler>();
            }

            sampler = foundation.NativeDevice.SamplerBank.AddOrRetrieveExisting(sampler);
            foundation.Samplers[node.SlotID][node.Conditions] = sampler;
        }
    }
}
