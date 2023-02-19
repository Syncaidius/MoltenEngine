using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class StateNodeParser : ShaderNodeParser
    {
        class PropertyBinding
        {
            public PropertyInfo Info;

            public ShaderNodeAttribute Attribute;
        }

        class PropertyCache
        {
            public Dictionary<string, PropertyBinding> Properties = new Dictionary<string, PropertyBinding>();
        }

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        Dictionary<Type, PropertyCache> _typeCache = new Dictionary<Type, PropertyCache>();

        protected void ParseProperties(ShaderHeaderNode node, ShaderCompilerContext context, object stateObject)
        {
            // Check if we need to cache the new stateObject type.
            if (!_typeCache.TryGetValue(stateObject.GetType(), out PropertyCache cache))
            {
                cache = new PropertyCache();
                PropertyInfo[] pInfo = stateObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo p in pInfo)
                {
                    ShaderNodeAttribute att = p.GetCustomAttribute<ShaderNodeAttribute>();

                    // Ignore non-node properties.
                    if (att == null)
                        continue;

                    cache.Properties.Add(p.Name.ToLower(), new PropertyBinding()
                    {
                        Info = p,
                        Attribute = att
                    });
                }

                _typeCache.Add(stateObject.GetType(), cache);
            }

            foreach ((string Name, string Value) c in node.ChildValues)
            {
                string lowName = c.Name.ToLower();
                if (!cache.Properties.TryGetValue(lowName, out PropertyBinding pBind))
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
                        if (EngineUtil.TryParseEnum(pBind.Info.PropertyType, c.Value, out object enumValue))
                            pBind.Info.SetValue(stateObject, enumValue);
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
                            if(ParseColor4(context, c.Value, true, out Color4 value))
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
                if (!cache.Properties.TryGetValue(lowName, out PropertyBinding pBind))
                {
                    UnsupportedTagMessage(context, node.Name, c.Name);
                    continue;
                }

                object pValue = pBind.Info.GetValue(stateObject);
                ParseProperties(c, context, pValue);

                if (pBind.Info.PropertyType.IsValueType)
                {
                    if (pBind.Info.SetMethod != null)
                        pBind.Info.SetValue(stateObject, pValue);
                    else
                        context.AddError($"Unable to apply definition '{node.Name}-{c.Name}' to '{pBind.Info.Name}' value-type property: No setter method available");
                }
            }
        }
    }
}
