using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public static class TestHelper
    {
        /// <summary>Produces a short string that can be used to retrieve the type via Type.GetType.</summary>
        /// <param name="type">The type to create the string for.</param>
        /// <returns></returns>
        public static string GetQualifiedTypeName(Type type)
        {
            string[] delims = new string[1] { ", " };

            return type + ", " + type.Assembly.FullName.Split(delims, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        /// <summary>Capitalizes the first letter of a string.</summary>
        /// <param name="input">The input string to capitalize.</param>
        /// <returns></returns>
        public static string CapitalizeString(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static byte[] GetBytes(object o)
        {
            int size = Marshal.SizeOf(o);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(o, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public static byte[] GetBytes(string str, Encoding encoding)
        {
            return encoding.GetBytes(str); ;
        }

        public static T GetLastValue<T>() where T : struct, IConvertible
        {
            Type t = typeof(T);

            if (t.IsEnum == false)
                throw new InvalidOperationException("Type must be an enum");

            return Enum.GetValues(t).Cast<T>().Last();
        }

        public static int NumberOfBitsSet(int value, int bitSize)
        {
            int count = 0;

            for (int i = 0; i < bitSize; i++)
            {
                int v = 1 << i;
                if ((value & v) == v)
                    count++;
            }

            return count;
        }

        /// <summary>Finds all types that derive from the provided class type.</summary>
        /// <typeparam name="T">The base type of other classes to search for.</typeparam>
        /// <returns></returns>
        public static List<Type> FindType<T>()
        {
            Type bType = typeof(T);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> result = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<Type> types = assembly.GetTypes().Where(t => t.IsSubclassOf(bType));
                result.AddRange(types);
            }

            return result;
        }

        /// <summary>Gets the name of a type. Includes its namespace and name only.</summary>
        /// <param name="type">The type of which to retrieve the name.</param>
        /// <returns></returns>
        public static string GetTypeName(Type type)
        {
            string typeName = "";

            typeName = type.Namespace + "." + type.Name;
            if (type.IsGenericType)
            {
                typeName += "<";
                Type[] generics = type.GetGenericArguments();
                for (int i = 0; i < generics.Length; i++)
                {
                    if (i > 0)
                        typeName += ", " + GetTypeName(generics[i]);
                    else
                        typeName += GetTypeName(generics[i]);
                }

                typeName += ">";
            }

            return typeName;
        }

        public static IEnumerable<Type> FindType<T>(Assembly assembly)
        {
            Type bType = typeof(T);

            return assembly.GetTypes().Where(t => t.IsSubclassOf(bType));
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);

            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                folder += Path.DirectorySeparatorChar;

            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
