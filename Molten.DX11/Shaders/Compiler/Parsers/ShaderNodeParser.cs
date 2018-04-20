using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal abstract class ShaderNodeParser
    {
        internal abstract string[] SupportedNodes { get; }

        internal abstract NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node);

        protected void InvalidValueMessage(ShaderCompilerContext context, XmlNode node, string friendlyTagName, string friendlyValueName)
        {
            context.Messages.Add($"Tag '{node.Name}' ({friendlyTagName}) has invalid value '{node.InnerText}'. Must be a {friendlyValueName} value");
        }

        protected void UnsupportedTagMessage(ShaderCompilerContext context, XmlNode node)
        {
            if(node.ParentNode != null)
                context.Messages.Add($"Ignoring unsupported {node.ParentNode.Name} tag '{node.Name}'.");
            else
                context.Messages.Add($"Ignoring unsupported root tag '{node.Name}'.");
        }

        protected void InvalidEnumMessage<T>(ShaderCompilerContext context, XmlNode node, string friendlyTagName)
            where T : struct, IComparable
        {
            Type t = typeof(T);
            bool isFlags = t.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

            T[] possibleVals = Enum.GetValues(t) as T[];
            if (possibleVals.Length == 0)
                return;

            int last = possibleVals.Length - 1;
            string strPossibleVals = "";
            for (int i = 0; i < possibleVals.Length; i++)
            {
                if (i == last)
                    strPossibleVals += isFlags ? " and " : " or ";
                else if (i > 0)
                    strPossibleVals += ", ";

                strPossibleVals += possibleVals[i].ToString();
            }

            if (isFlags)
                context.Messages.Add($"Tag '{node.Name}' ({friendlyTagName}) has invalid value '{node.InnerText}'. Must be a combination of {strPossibleVals}");
            else
                context.Messages.Add($"Tag '{node.Name}' ({friendlyTagName}) has invalid value '{node.InnerText}'. Must be {strPossibleVals}");
        }
    }
}
