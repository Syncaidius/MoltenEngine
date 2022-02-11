using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    public abstract class ShaderNodeParser<R, S, CR>
        where R : RenderService
        where S : IShader
        where CR : ShaderCompileResult<S>
    {
        static string[] _colorDelimiters = new string[] { ",", " " };

        internal abstract string[] SupportedNodes { get; }

        internal abstract NodeParseResult Parse(S shader, ShaderCompilerContext<R, S, CR> context, XmlNode node);

        protected void InvalidValueMessage(ShaderCompilerContext<R, S, CR> context, XmlNode node, string friendlyTagName, string friendlyValueName)
        {
            context.AddWarning($"Tag '{node.Name}' ({friendlyTagName}) has invalid value '{node.InnerText}'. Must be a {friendlyValueName} value");
        }

        protected void UnsupportedTagMessage(ShaderCompilerContext<R, S, CR> context, XmlNode node)
        {
            if(node.ParentNode != null)
                context.AddWarning($"Ignoring unsupported {node.ParentNode.Name} tag '{node.Name}'.");
            else
                context.AddWarning($"Ignoring unsupported root tag '{node.Name}'.");
        }

        protected void InvalidEnumMessage<T>(ShaderCompilerContext<R, S, CR> context, XmlNode node, string friendlyTagName)
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
                context.AddWarning($"Tag '{node.Name}' ({friendlyTagName}) has invalid value '{node.InnerText}'. Must be a combination of {strPossibleVals}");
            else
                context.AddWarning($"Tag '{node.Name}' ({friendlyTagName}) has invalid value '{node.InnerText}'. Must be {strPossibleVals}");
        }

        protected Color4 ParseColor4(ShaderCompilerContext<R, S, CR> context, XmlNode node, bool fromRgb)
        {
            string[] vals = node.InnerText.Split(_colorDelimiters, StringSplitOptions.RemoveEmptyEntries);
            int maxVals = Math.Min(4, vals.Length);

            if (fromRgb)
            {
                Color col = Color.Black;
                for (int i = 0; i < maxVals; i++)
                {
                    if (byte.TryParse(vals[i], out byte cVal))
                        col[i] = cVal;
                    else
                        context.AddWarning($"Invalid sampler border color component '{vals[i]}'. A maximum of 4 space-separated values is allowed, each between 0 and 255.");
                }

                return col.ToColor4();
            }
            else
            {
                Color4 col = Color4.Black;
                for (int i = 0; i < maxVals; i++)
                {
                    if (float.TryParse(vals[i], out float cVal))
                        col[i] = cVal;
                    else
                        context.AddWarning($"Invalid sampler border color component '{vals[i]}'. A maximum of 4 space-separated values is allowed, each between 0 and 255.");
                }

                return col;
            }
        }
    }
}
