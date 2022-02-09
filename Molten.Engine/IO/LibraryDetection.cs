using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Molten
{
    public static class LibraryDetection
    {
        public static IEnumerable<Type> FindType<T>(Assembly assembly)
        {
            Type t = typeof(T);
            Type[] types = assembly.GetTypes();
            types = types.Where(i => t.IsAssignableFrom(i)).ToArray();
            return types;
        }

        private static string[] ParsePathString(string path)
        {
            string[] result = path.Split(';');
            for (int i = 0; i < result.Length; i++)
                result[i] = result[i].Trim();

            return result;
        }

        public static T LoadInstance<T>(Logger log, string libraryLabel, string objectLabel, SettingValue<string> typePath, SettingBank settings, string defaultTypePath, out Assembly loadedAssembly)
        {
            // Part 0 = assembly filename
            // Part 1 = namespace and object name of library implementation.
            Type type = typeof(T);
            string[] parts = ParsePathString(typePath.Value);
            string fullPath = Path.GetFullPath(parts[0]);
            if (!File.Exists(parts[0]))
            {
                log.WriteError($"Attempt to load {libraryLabel} library failed. Assembly file does not exist.");
                loadedAssembly = null;
                return default;
            }

            log.WriteLine($"Attempting to load {libraryLabel} library {parts[0]}");

            // Default if provided one is not found.
            loadedAssembly = Assembly.LoadFrom(parts[0]);
            List<Type> renderers = FindType<T>(loadedAssembly).ToList();
            if (renderers.Count == 0)
            {
                log.WriteLine($"Provided {libraryLabel} library has no implementations of {type.Name}");
                log.WriteLine($"Defaulting to {defaultTypePath} {libraryLabel} library, please restart program");
                typePath.Value = defaultTypePath;
                settings.Apply();
                return default;
            }

            // Attempt to instanciate library object
            T instance = default(T);
            Type rType = null;
            foreach (Type t in renderers)
            {
                if (t.FullName == parts[1])
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
                return default;
            }

            return instance;
        }
    }
}
