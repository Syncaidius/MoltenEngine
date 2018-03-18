using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public static class LibraryDetection
    {
        public static Assembly LoadLibary(string assemblyPath)
        {            
            return Assembly.LoadFrom(assemblyPath);
        }

        public static IEnumerable<Type> FindType<T>(Assembly assembly)
        {
            Type t = typeof(T);
            Type[] types = assembly.GetTypes();
            types = types.Where(i => t.IsAssignableFrom(i)).ToArray();
            return types;
        }

        public static T LoadInstance<T>(Logger log, string libraryLabel, string objectLabel, SettingValue<string> typePath, SettingBank settings, string defaultTypePath, out Assembly loadedAssembly)
        {
            Type type = typeof(T);
            string[] parts = typePath.Value.Split(';');
            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim().ToLower();

            string path = parts[0];
            if (!File.Exists(path))
                path = defaultTypePath;

            log.WriteLine($"Attempting to load {libraryLabel} library {parts[0]}");

            // Default if provided one is not found.
            loadedAssembly = LibraryDetection.LoadLibary(parts[0]);
            List<Type> renderers = LibraryDetection.FindType<T>(loadedAssembly).ToList();
            if (renderers.Count == 0)
            {
                log.WriteLine($"Provided {libraryLabel} library has no implementations of {type.Name}");
                log.WriteLine($"Defaulting to {defaultTypePath} {libraryLabel} library, please restart program");
                typePath.Value = defaultTypePath;
                settings.Apply();
                return default(T);
            }

            // Attempt to instanciate renderer
            T instance = default(T);
            Type rType = null;
            foreach (Type t in renderers)
            {
                if (t.FullName.ToLower() == parts[1])
                {
                    log.WriteLine($"Provided {objectLabel} found: {parts[1]}");
                    rType = t;
                    break;
                }
            }

            // Instanciate
            if (rType != null)
            {
                try
                {
                    instance = (T)Activator.CreateInstance(rType);
                    log.WriteLine($"Created {objectLabel}");
                }
                catch (Exception e)
                {
                    log.WriteLine($"Failed to create {objectLabel} instance");
                    log.WriteError(e);
                }
            }
            else
            {
                log.WriteLine($"No renderers found in {parts[0]} matching the name {parts[1]}");
                return default(T);
            }

            return instance;
        }
    }
}
