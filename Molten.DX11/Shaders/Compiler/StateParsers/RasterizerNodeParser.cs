using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class RasterizerNodeParser
    {
        internal GraphicsRasterizerState Parse(HlslShader shader, ShaderCompilerContext context, XmlNode node)
        {
            GraphicsRasterizerState state = new GraphicsRasterizerState();

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName)
                {
                    case "cullmode":
                        if (Enum.TryParse(child.InnerText, out CullMode mode))
                            state.CullMode = mode;                        
                        break;

                    case "depthbias":
                        if (int.TryParse(child.InnerText, out int bias))
                            state.DepthBias = bias;
                        break;

                    case "depthbiasclamp":
                        if (float.TryParse(child.InnerText, out float biasClamp))
                            state.DepthBiasClamp = biasClamp;
                        break;

                    case "fillmode":
                        if (Enum.TryParse(child.InnerText, out FillMode fillMode))
                            state.FillMode = fillMode;
                        break;

                    case "aaline": // IsAntialiasedLineEnabled
                        if (bool.TryParse(child.InnerText, out bool aaLineEnabled))
                            state.IsAntialiasedLineEnabled = aaLineEnabled;
                        break;

                    case "depthclip":
                        if (bool.TryParse(child.InnerText, out bool depthClipEnabled))
                            state.IsDepthClipEnabled = depthClipEnabled;
                        break;

                    case "frontisccw":
                        if (bool.TryParse(child.InnerText, out bool frontIsCCW))
                            state.IsFrontCounterClockwise = frontIsCCW;
                        break;

                    case "multisample":
                        if(bool.TryParse(child.InnerText, out bool multisampleEnabled))
                        state.IsMultisampleEnabled = multisampleEnabled;
                        break;

                    case "scissortest":
                        if (bool.TryParse(child.InnerText, out bool scissorEnabled))
                            state.IsScissorEnabled = scissorEnabled;
                        break;

                    case "scaledslopebias":
                        if (float.TryParse(child.InnerText, out float scaledSlopeDepthBias))
                            state.SlopeScaledDepthBias = scaledSlopeDepthBias;
                        break;
                }
            }

            // Check if an identical state exists before returning the new one.
            foreach(GraphicsRasterizerState existing in context.RasterStates)
            {
                if (existing.Equals(state))
                {
                    state.Dispose();
                    return existing;
                }
            }

            // If we've reached this far, the state is new and unique.
            context.RasterStates.Add(state);
            return state;
        }
    }
}
