using Silk.NET.Core.Native;
using System.Text.RegularExpressions;

namespace Molten
{
    public unsafe static class SilkUtil
    {
        /// <summary>Releases the specified pointer, sets it to null and returns the updated, unmanaged reference count.</summary>
        /// <typeparam name="T">The type of pointer.</typeparam>
        /// <param name="ptr">The pointer.</param>
        /// <returns>The new pointer reference count.</returns>
        public static uint ReleasePtr<T>(ref T* ptr)
            where T : unmanaged
        {
            if (ptr == null)
                return 0;

            uint r = ((IUnknown*)ptr)->Release();
            ptr = null;
            return r;
        }

        /// <summary>
        /// Try parsing an enum value using Silk.NET's possible naming conventions. e.g. DepthWriteMask.DepthWriteMaskAll or ComparisonFunc.ComparisonLessEqual.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static  bool TryParseEnum<T>(string strValue, out T value)
            where T : struct, IComparable
        {
            Type t = typeof(T);
            strValue = strValue.ToLower();

            // First try to parse the value as-is.
            // If we fail, prefix it with the enum name to match Silk.NET's enum naming scheme. e.g. DepthWriteMask.DepthWriteMaskAll.
            if (!Enum.TryParse(strValue, true, out value))
            {
                // Now try adding the enum name into the value string.
                string strFullValue = $"{t.Name.ToLower()}{strValue}";

                if (!Enum.TryParse(strFullValue, true, out value))
                {
                    string[] split = Regex.Split(t.Name, "(?<!^)(?=[A-Z][\\d+]?)");
                    foreach (string s in split)
                        strValue = strValue.Replace(s, "");

                    // Try to parse by adding one word at a time, then two, etc.
                    for (int pCount = 1; pCount <= split.Length; pCount++)
                    {
                        string strNextValue = "";
                        for (int i = 0; i < pCount; i++)
                            strNextValue += split[i];

                        strNextValue += strValue;

                        if (Enum.TryParse(strNextValue, true, out value))
                            return true;
                    }

                    return false;
                }
            }

            return true;
        }
    }
}
