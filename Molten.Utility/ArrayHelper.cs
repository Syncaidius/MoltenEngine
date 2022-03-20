namespace Molten
{
    public static class ArrayHelper
    {
        /// <summary>Concatenates several arrays into a single array.</summary>
        /// <typeparam name="T">The array element type.</typeparam>
        /// <param name="arrays">The arrays to be concatenated</param>
        /// <returns></returns>
        public static T[] ConcatMany<T>(params T[][] arrays)
        {
            long len = 0;
            for (int i = 0; i < arrays.Length; i++)
                len += arrays[i].Length;

            T[] result = new T[len];
            int startIndex = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                Array.Copy(arrays[i], 0, result, startIndex, arrays[i].Length);
                startIndex += arrays[i].Length;
            }

            return result;
        }

        /// <summary>Concatenates two arrays into a single new array.</summary>
        /// <typeparam name="T">The array element type.</typeparam>
        /// <param name="left">The left array. It's elements are positioned first within the new array</param>
        /// <param name="right">The right array. It's elements are positioned after the elements of the left array, within the new array.</param>
        /// <returns></returns>
        public static T[] Concat<T>(T[] left, T[] right)
        {
            long len = left.Length + right.Length;
            T[] result = new T[len];
            Array.Copy(left, 0, result, 0, left.Length);
            Array.Copy(right, 0, result, left.Length, right.Length);

            return result;
        }
    }
}
