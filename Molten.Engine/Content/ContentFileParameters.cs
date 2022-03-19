using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class ContentFileParameters
    {
        internal Dictionary<string, object> Data { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Retrieves
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Get<T>(string name)
        {
            if (Data.TryGetValue(name.ToLower(), out object obj))
                return (T)obj;
            else
                throw new Exception($"Invalid content parameter '{name}'");
        }

        internal void Validate(ContentContext context, ContentProcessor processor)
        {
            // First gather unrecognised parameters
            List<string> unrecognised = new List<string>();
            foreach (string pName in Data.Keys)
            {
                if (!processor.ParametersByName.ContainsKey(pName))
                    unrecognised.Add(pName);
            }

            // Strip unrecognised parameters
            foreach (string u in unrecognised)
                Data.Remove(u);

            // Validate remaining parameters...
            foreach (ContentParameter p in processor.Parameters)
            {
                if (Data.ContainsKey(p.Name))
                {
                    Type valueType = Data[p.Name].GetType();
                    if (!p.ExpectedType.IsAssignableFrom(valueType))
                    {
                        try
                        {
                            Data[p.Name] = Convert.ChangeType(Data[p.Name], p.ExpectedType);
                        }
                        catch
                        {
                            context.Log.Warning($"[CONTENT] {context.File}: Parameter '{p.Name}' was passed a value of type '{valueType.Name}'. Expected '{p.ExpectedType}'");
                            Data.Remove(p.Name);
                        }
                    }
                }
                else // ... Add default value for missing parameter
                {
                    Data.Add(p.Name, p.DefaultValue);
                }
            }
        }
    }
}
