using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Molten
{
    public static class EmbeddedResource
    {
        /// <summary>Reads a resource file embedded into the assembly.</summary>
        /// <param name="name">The name of the resource to load.</param>
        /// <returns></returns>
        public static string ReadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string result = "";

            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            return result;
        }

        public static Stream GetStream(string name, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();

            Stream stream = null;
            try
            {
                stream = assembly.GetManifestResourceStream(name);
            }
            finally { } 

            return stream;
        }
    }
}
