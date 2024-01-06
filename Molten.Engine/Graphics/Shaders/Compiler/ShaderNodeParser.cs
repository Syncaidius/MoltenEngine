using System.Reflection;
using System.Xml;

namespace Molten.Graphics;

internal abstract class ShaderNodeParser
{
    class FieldBinding
    {
        public FieldInfo Info;

        public ShaderNodeAttribute Attribute;
    }

    class FieldCache
    {
        public Dictionary<string, FieldBinding> Fields { get; } = new Dictionary<string, FieldBinding>();
    }

    static string[] _colorDelimiters = new string[] { ",", " " };

    public abstract ShaderNodeType NodeType { get; }

    internal void Parse(ShaderDefinition def, ShaderPassDefinition passDef, ShaderCompilerContext context, XmlNode node)
    {
        ShaderHeaderNode shn = ParseNode(context, node);
        OnParse(def, passDef, context, shn);
    }

    private ShaderHeaderNode ParseNode(ShaderCompilerContext context, XmlNode node)
    {
        ShaderHeaderNode hNode = new ShaderHeaderNode(node);

        // Parse attributes
        if (node.Attributes != null)
        {
            foreach (XmlAttribute att in node.Attributes)
            {
                string nName = att.Name.ToLower();
                if (string.IsNullOrWhiteSpace(att.InnerText))
                {
                    context.AddWarning($"Skipping empty attribute '{nName}' in '{hNode.Name}' node");
                    continue;
                }

                switch (nName)
                {
                    case "value":
                        hNode.Values[ShaderHeaderValueType.Value] = att.InnerText;
                        break;

                    case "preset":
                        hNode.Values[ShaderHeaderValueType.Preset] = att.InnerText;
                        break;

                    case "blend":
                        hNode.Values[ShaderHeaderValueType.BlendPreset] = att.InnerText;
                        break;

                    case "rasterizer":
                        hNode.Values[ShaderHeaderValueType.RasterizerPreset] = att.InnerText;
                        break;

                    case "depth":
                        hNode.Values[ShaderHeaderValueType.DepthPreset] = att.InnerText;
                        break;

                    case "index":
                        hNode.Values[ShaderHeaderValueType.SlotID] = att.InnerText;
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
                    hNode.Values[ShaderHeaderValueType.Value] = node.InnerText;
            }

            foreach (XmlNode c in node.ChildNodes)
            {
                if (c.Name == "#text" || c.Name == "#comment")
                    continue;

                string cName = c.Name.ToLower();
                ShaderHeaderNode cNode = ParseNode(context, c);
                string cValue = null;
                cNode.Values.TryGetValue(ShaderHeaderValueType.Value, out cValue);

                switch (cName)
                {
                    default:
                        if (cNode.ChildNodes.Count > 0)
                            hNode.ChildNodes.Add(cNode);
                        else
                            hNode.ChildValues.Add((cName, cValue));

                        break;
                }
            }
        }

        return hNode;
    }

    protected abstract void OnParse(ShaderDefinition def, ShaderPassDefinition passDef, ShaderCompilerContext context, ShaderHeaderNode node);

    protected void InvalidValueMessage(ShaderCompilerContext context, (string Name, string Value) node, string friendlyTagName, string friendlyValueName)
    {
        context.AddWarning($"Tag '{node.Name}' ({friendlyTagName}) has invalid value '{node.Value}'. Must be a {friendlyValueName} value");
    }

    protected void UnsupportedTagMessage(ShaderCompilerContext context, string parentName, string nodeName)
    {
        parentName ??= "root";
        context.AddWarning($"Ignoring unsupported '{parentName}' tag '{nodeName}'.");
    }

    protected void InvalidEnumMessage<T>(ShaderCompilerContext context, (string Name, string Value) node, string friendlyTagName)
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

    protected bool ParseColor4(ShaderCompilerContext context, string value, bool fromRgb, out Color4 result)
    {
        string[] vals = value.Split(_colorDelimiters, StringSplitOptions.RemoveEmptyEntries);
        int maxVals = Math.Min(4, vals.Length);
        result = Color4.Black;

        if (fromRgb)
        {
            Color col = Color.Black;
            for (int i = 0; i < maxVals; i++)
            {
                if (byte.TryParse(vals[i], out byte cVal))
                {
                    col[i] = cVal;
                }
                else
                {
                    context.AddWarning($"Invalid sampler border color component '{vals[i]}'. A maximum of 4 space-separated values is allowed, each between 0 and 255.");
                    return false;
                }
            }

            result = col.ToColor4();
        }
        else
        {
            Color4 col = Color4.Black;
            for (int i = 0; i < maxVals; i++)
            {
                if (float.TryParse(vals[i], out float cVal))
                {
                    col[i] = cVal;
                }
                else
                {
                    context.AddWarning($"Invalid sampler border color component '{vals[i]}'. A maximum of 4 space-separated values is allowed, each between 0 and 255.");
                    return false;
                }
            }

            result = col;
        }

        return true;
    }


    Dictionary<Type, FieldCache> _typeCache = new Dictionary<Type, FieldCache>();

    protected void ParseFields<T>(ShaderHeaderNode node, ShaderCompilerContext context, ref T stateObject)
    {
        // Check if we need to cache the new stateObject type.
        if (!_typeCache.TryGetValue(stateObject.GetType(), out FieldCache cache))
        {
            cache = new FieldCache();
            FieldInfo[] fInfo = stateObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo f in fInfo)
            {
                ShaderNodeAttribute att = f.GetCustomAttribute<ShaderNodeAttribute>();

                // Ignore non-node properties.
                if (att == null)
                    continue;

                cache.Fields.Add(f.Name.ToLower(), new FieldBinding()
                {
                    Info = f,
                    Attribute = att
                });
            }

            _typeCache.Add(stateObject.GetType(), cache);
        }

        foreach ((string Name, string Value) c in node.ChildValues)
        {
            string lowName = c.Name.ToLower();
            if (!cache.Fields.TryGetValue(lowName, out FieldBinding pBind))
            {
                UnsupportedTagMessage(context, node.Name, c.Name);
                continue;
            }

            switch (pBind.Attribute.ParseType)
            {
                case ShaderNodeParseType.Bool:
                    {
                        if (bool.TryParse(c.Value, out bool value))
                            pBind.Info.SetValue(stateObject, value);
                        else
                            InvalidValueMessage(context, c, pBind.Info.Name, pBind.Attribute.ParseType.ToString());
                    }
                    break;


                case ShaderNodeParseType.Enum:
                    if (EngineUtil.TryParseEnum(pBind.Info.FieldType, c.Value, out object enumValue))
                    {
                        if (stateObject.GetType().IsValueType)
                        {
                            object tmp = (object)stateObject;
                            pBind.Info.SetValue(tmp, enumValue);
                            stateObject = (T)tmp;
                        }
                        else
                        {
                            pBind.Info.SetValue(stateObject, enumValue);
                        }
                    }
                    else
                        InvalidEnumMessage<GraphicsDepthWritePermission>(context, c, pBind.Info.Name);
                    break;

                case ShaderNodeParseType.UInt32:
                    {
                        if (uint.TryParse(c.Value, out uint value))
                            pBind.Info.SetValue(stateObject, value);
                        else
                            InvalidValueMessage(context, c, pBind.Info.Name, pBind.Attribute.ParseType.ToString());
                    }
                    break;

                case ShaderNodeParseType.Int32:
                    {
                        if (int.TryParse(c.Value, out int value))
                            pBind.Info.SetValue(stateObject, value);
                        else
                            InvalidValueMessage(context, c, pBind.Info.Name, pBind.Attribute.ParseType.ToString());
                    }
                    break;

                case ShaderNodeParseType.Byte:
                    {
                        if (byte.TryParse(c.Value, out byte value))
                            pBind.Info.SetValue(stateObject, value);
                        else
                            InvalidValueMessage(context, c, pBind.Info.Name, pBind.Attribute.ParseType.ToString());
                    }
                    break;

                case ShaderNodeParseType.Float:
                    {
                        if (float.TryParse(c.Value, out float value))
                            pBind.Info.SetValue(stateObject, value);
                        else
                            InvalidValueMessage(context, c, pBind.Info.Name, pBind.Attribute.ParseType.ToString());
                    }
                    break;


                case ShaderNodeParseType.Color:
                    {
                        if (ParseColor4(context, c.Value, true, out Color4 value))
                            pBind.Info.SetValue(stateObject, value);
                        else
                            InvalidValueMessage(context, c, pBind.Info.Name, pBind.Attribute.ParseType.ToString());
                    }
                    break;

                default:
                    UnsupportedTagMessage(context, node.Name, c.Name);
                    break;
            }
        }

        // Iterate over object properties
        foreach (ShaderHeaderNode c in node.ChildNodes)
        {
            string lowName = c.Name.ToLower();
            if (!cache.Fields.TryGetValue(lowName, out FieldBinding pBind))
            {
                UnsupportedTagMessage(context, node.Name, c.Name);
                continue;
            }

            object pValue = pBind.Info.GetValue(stateObject);
            ParseFields(c, context, ref pValue);

            if (pBind.Info.FieldType.IsValueType)
                pBind.Info.SetValue(stateObject, pValue);
        }
    }

    protected string InitializeEntryPoint(ShaderPassDefinition passDef, ShaderCompilerContext context, ShaderHeaderNode node, ShaderType type)
    {
        if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string entryPoint))
            passDef.EntryPoints[type] = entryPoint;
        else
            context.AddError($"<{type.ToString().ToLower()}> entry-point tag is missing a value");

        return entryPoint;
    }
}
