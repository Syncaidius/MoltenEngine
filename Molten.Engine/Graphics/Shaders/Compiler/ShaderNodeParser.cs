using System.Xml;

namespace Molten.Graphics
{
    public abstract class ShaderNodeParser<R, S>
        where R : RenderService
        where S : IShaderElement
    {
        static string[] _colorDelimiters = new string[] { ",", " " };

        /// <summary>
        /// Gets a list of <see cref="IShaderElement"/> types that the current <see cref="ShaderNodeParser{R, S}"/> accepts. If null, all <typeparamref name="S"/> will be accepted.
        /// </summary>
        public abstract Type[] TypeFilter { get; }

        public abstract ShaderNodeType NodeType { get; }

        public void Parse(S foundation, ShaderCompilerContext<R, S> context, XmlNode node)
        {
            if(TypeFilter != null)
            {
                bool isValid = false;
                int validFilterCount = 0;
                foreach(Type t in TypeFilter)
                {
                    if (typeof(IShaderElement).IsAssignableFrom(t))
                    {
                        if (t.IsAssignableFrom(foundation.GetType()))
                        {
                            isValid = true;
                            validFilterCount++;
                            break;
                        }
                    }
                    else
                    {
                        context.AddWarning($"Ignoring invalid filter type '{t.Name}' provided in '{this.GetType().Name}' shader node parser");
                    }
                }

                if(!isValid && validFilterCount > 0)
                {
                    context.AddWarning($"Ignoring unsupported '{node.Name}' node in '{foundation.GetType().Name}' definition");
                    return;
                }    
            }

            ShaderHeaderNode shn = ParseNode(context, node);
            OnParse(foundation, context, shn);
        }

        private ShaderHeaderNode ParseNode(ShaderCompilerContext<R, S> context, XmlNode node)
        {
            ShaderHeaderNode shn = new ShaderHeaderNode(node);

            // Parse attributes
            if (node.Attributes != null)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    string nName = att.Name.ToLower();
                    switch (nName)
                    {
                        case "value":
                            shn.Value = att.InnerText;
                            shn.ValueType = ShaderHeaderValueType.Value;
                            break;

                        case "preset":
                            if (shn.ValueType != ShaderHeaderValueType.Value)
                            {
                                shn.Value = att.InnerText;
                                shn.ValueType = ShaderHeaderValueType.Preset;
                            }
                            break;

                        case "index":
                            if (!int.TryParse(att.InnerText, out int index))
                                index = 0;

                            shn.SlotID = index;
                            break;
                    }
                }
            }

            // Parse sub-values
            if (node.ChildNodes.Count > 0)
            {
                if(node.ChildNodes.Count == 1)
                {
                    XmlNode cNode = node.ChildNodes[0];
                    if (cNode.Name == "#text")
                        shn.Value = node.InnerText;
                }

                foreach (XmlNode c in node.ChildNodes)
                {
                    string cName = c.Name.ToLower();

                    if (cName == "depth" && c.ChildNodes.Count > 0)
                    {

                    }

                    ShaderHeaderNode cNode = ParseNode(context, c);

                    switch (cName)
                    {
                        case "condition":
                            if (Enum.TryParse(cNode.Value, true, out StateConditions sc))
                                shn.Conditions |= sc;
                            else
                                InvalidEnumMessage<StateConditions>(context, (c.Name, c.InnerText), "state condition");
                            break;

                        default:
                            if (c.ChildNodes.Count > 0)
                                shn.ChildNodes.Add(cNode);
                            else
                                shn.ChildValues.Add((cName, cNode.Value));

                            break;
                    }
                }
            }

            return shn;
        }

        protected abstract void OnParse(S foundation, ShaderCompilerContext<R, S> context, ShaderHeaderNode node);

        protected void InvalidValueMessage(ShaderCompilerContext<R, S> context, (string Name, string Value) node, string friendlyTagName, string friendlyValueName)
        {
            context.AddWarning($"Tag '{node.Name}' ({friendlyTagName}) has invalid value '{node.Value}'. Must be a {friendlyValueName} value");
        }

        protected void UnsupportedTagMessage(ShaderCompilerContext<R, S> context, string parentName, (string Name, string Value) node)
        {
            parentName = parentName ?? "root";
            context.AddWarning($"Ignoring unsupported {parentName} tag '{node.Name}'.");
        }

        protected void InvalidEnumMessage<T>(ShaderCompilerContext<R, S> context, (string Name, string Value) node, string friendlyTagName)
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

            string mustBe = isFlags ? "a combination of " : "";
            context.AddWarning($"Tag '{node.Name}' ({friendlyTagName}) has invalid value '{node.Value}'. Must be {mustBe}{strPossibleVals}");
        }

        protected Color4 ParseColor4(ShaderCompilerContext<R, S> context, string value, bool fromRgb)
        {
            string[] vals = value.Split(_colorDelimiters, StringSplitOptions.RemoveEmptyEntries);
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
