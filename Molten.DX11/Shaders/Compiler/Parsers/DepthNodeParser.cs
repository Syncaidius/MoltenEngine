using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class DepthNodeParser : ShaderNodeParser
    {
        internal override string[] SupportedNodes => new string[] { "depth" };

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            if (foundation is ComputeTask)
                return new NodeParseResult(NodeParseResultType.Ignored);

            GraphicsDepthState state = new GraphicsDepthState(foundation.Device.GetPreset(DepthStencilPreset.Default));

            foreach (XmlAttribute attribute in node.Attributes)
            {
                string attName = attribute.Name.ToLower();
                switch (attName)
                {
                    case "preset":
                        if (Enum.TryParse(attribute.InnerText, true, out DepthStencilPreset preset))
                            state = new GraphicsDepthState(foundation.Device.GetPreset(preset));
                        break;
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName)
                {
                    case "enabled":
                        if (bool.TryParse(child.InnerText, out bool depthEnabled))
                            state.IsDepthEnabled = depthEnabled;
                        break;

                    case "stencilenabled":
                        if (bool.TryParse(child.InnerText, out bool stencilEnabled))
                            state.IsDepthEnabled = stencilEnabled;
                        break;

                    case "writemask":
                        if (Enum.TryParse(child.InnerText, true, out DepthWriteMask fillMode))
                            state.DepthWriteMask = fillMode;
                        break;

                    case "comparison":
                        if (Enum.TryParse(child.InnerText, true, out Comparison comparison))
                            state.DepthComparison = comparison;
                        break;

                    case "stencilreadmask":
                        if (byte.TryParse(child.InnerText, out byte stencilReadMask))
                            state.StencilReadMask = stencilReadMask;
                        break;

                    case "stencilwritemask":
                        if (byte.TryParse(child.InnerText, out byte stencilWriteMask))
                            state.StencilWriteMask = stencilWriteMask;
                        break;

                    case "front":
                        ParseFaceNode(child, state.FrontFace);
                        break;

                    case "back":
                        ParseFaceNode(child, state.BackFace);
                        break;
                }
            }

            bool existingState = false;
            // Check if an identical state exists before returning the new one.
            foreach (GraphicsDepthState existing in context.DepthStates)
            {
                if (existing.Equals(state))
                {
                    state.Dispose();
                    existingState = true;
                    state = existing;
                    break;
                }
            }

            if (!existingState)
                context.DepthStates.Add(state);

            switch (foundation)
            {
                case Material material:
                    material.DepthState = state;

                    // Apply to existing passes which do not have a rasterizer state yet.
                    foreach (MaterialPass p in material.Passes)
                    {
                        if (p.DepthState == null)
                            p.DepthState = state;
                    }
                    break;

                case MaterialPass pass:
                    pass.DepthState = state;
                    break;
            }

            return new NodeParseResult(NodeParseResultType.Success);
        }

        private void ParseFaceNode(XmlNode faceNode, GraphicsDepthState.Face face)
        {
            foreach(XmlNode child in faceNode.ChildNodes)
            {
                string nodeName = faceNode.Name.ToLower();
                switch (nodeName) {
                    case "comparison":
                        if (Enum.TryParse(child.InnerText, true, out Comparison comparison))
                            face.Comparison = comparison;
                        break;

                    case "stencilpass":
                        if (Enum.TryParse(child.InnerText, true, out StencilOperation stencilpassOp))
                            face.PassOperation = stencilpassOp;
                        break;

                    case "stencilfail":
                        if (Enum.TryParse(child.InnerText, true, out StencilOperation stencilFailOp))
                            face.FailOperation = stencilFailOp;
                        break;

                    case "fail":
                        if (Enum.TryParse(child.InnerText, true, out StencilOperation failOp))
                            face.DepthFailOperation = failOp;
                        break;
                }
            }
        }
    }
}
