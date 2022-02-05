using System.IO;
using System.Reflection;

namespace Molten.IO
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the asset/resource to be loaded.</param>
        /// <param name="assembly">The assembly from which to retrieve a stream. If null, <see cref="Assembly.GetExecutingAssembly()"/> will be used.</param>
        /// <returns></returns>
        public static Stream TryGetStream(string name, Assembly assembly = null)
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
