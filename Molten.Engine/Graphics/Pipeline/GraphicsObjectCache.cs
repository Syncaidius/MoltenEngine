using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GraphicsObjectCache<T>
        where T : GraphicsObject, IEquatable<T>, IDisposable
    {
        protected List<T> Objects { get; } = new List<T>();

        /// <summary>
        /// Attempts to find a matching <typeparamref name="T"/> instace, based on the given <typeparamref name="T"/> instance.
        /// </summary>
        /// <param name="existing">The existing <typeparamref name="T"/> instance to match against.</param>
        /// <returns>True if a match is found. <paramref name="existing"/> is disposed replaced with the match.</returns>
        internal bool Get(ref T existing)
        {
            foreach(T obj in Objects)
            {
                if (obj.Equals(existing))
                {
                    existing?.Dispose();
                    existing = obj;
                    return true;
                }
            }

            Objects.Add(existing);
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="DESC"></typeparam>
    internal class PipelineCacheVK<T, DESC> : GraphicsObjectCache<T>
        where T : GraphicsObject, IEquatable<T>, IDisposable, IEquatable<DESC>
        where DESC : struct
    {
        /// <summary>
        /// Attempts to find a matching <typeparamref name="T"/> instace, based on the given <typeparamref name="DESC"/> value. 
        /// If no match is found, <paramref name="createCallback"/> will be invoked to create a new <typeparamref name="T"/> instance. If no callback is provided, null will be returned.
        /// </summary>
        /// <param name="desc">The description or info struct to match against.</param>
        /// <param name="createCallback">The callback to invoke if no match is found for the provided <paramref name="desc"/>.</param>
        /// <returns></returns>
        internal T Get(ref DESC desc, Func<T> createCallback = null)
        {
            foreach (T obj in Objects)
            {
                if (obj.Equals(desc))
                    return obj;
            }

            // Create a new object if a callback is provided.
            if(createCallback != null)
            {
                T newObj = createCallback();
                Objects.Add(newObj);
                return newObj;
            }

            return null;
        }
    }
}
