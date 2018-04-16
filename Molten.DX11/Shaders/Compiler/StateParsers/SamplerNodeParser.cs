using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class SamplerNodeParser
    {
        internal ShaderSampler Parse(HlslShader shader, ShaderCompilerContext context, XmlNode node)
        {
            ShaderSampler sampler = new ShaderSampler();

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName)
                {
                    case "addressu":
                        if (Enum.TryParse(child.InnerText, out SamplerAddressMode modeu))
                            sampler.AddressU = modeu;                        
                        break;

                    case "addressv":
                        if (Enum.TryParse(child.InnerText, out SamplerAddressMode modev))
                            sampler.AddressV = modev;
                        break;

                    case "addressw":
                        if (Enum.TryParse(child.InnerText, out SamplerAddressMode modew))
                            sampler.AddressV = modew;
                        break;
                }
            }

            //// Check if an identical state exists before returning the new one.
            //foreach(ShaderSampler existing in context.Samplers)
            //{
            //    if (existing.EqualsPleaseImplement(sampler))
            //    {
            //        sampler.Dispose();
            //        return existing;
            //    }
            //}

            // If we've reached this far, the state is new and unique.
            context.Samplers.Add(sampler);
            return sampler;
        }
    }
}
