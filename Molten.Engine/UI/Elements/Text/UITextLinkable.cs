using System.Runtime.CompilerServices;

namespace Molten.UI
{
    public abstract class UITextLinkable<T> 
        where T : UITextLinkable<T>
    {
        /// <summary>
        /// Sets the next <typeparamref name="T"/> to the specified one, if any.
        /// </summary>
        /// <param name="next">The <typeparamref name="T"/> to be linked as the next.</param>
        /// <exception cref="Exception"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LinkNext(T next)
        {
            if (next == this)
                throw new Exception("Cannot link a UITextSegment to itself");

            // Allow the previous next to unlink.
            UnlinkNext();

            if (next != null)
            {
                next.UnlinkPrevious();
                next.Previous = this as T;
            }

            Next = next;
        }

        /// <summary>
        /// Sets the previous <typeparamref name="T"/> to the specified one, if any.
        /// </summary>
        /// <param name="prev">The <typeparamref name="T"/> to be linked as the previous.</param>
        /// <exception cref="Exception"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LinkPrevious(T prev)
        {
            if (prev == this)
                throw new Exception("Cannot link a UITextSegment to itself");

            UnlinkPrevious();

            if (prev != null)
            {
                prev.UnlinkNext();
                prev.Next = this as T;
            }

            Previous = prev;
        }

        /// <summary>
        /// Unlinks the next <typeparamref name="T"/>, if any.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnlinkNext()
        {
            if (Next != null)
            {
                Next.Previous = null;
                Next = null;
            }
        }

        /// <summary>
        /// Unlinks the previous <typeparamref name="T"/>, if any.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnlinkPrevious()
        {
            if (Previous != null)
            {
                Previous.Next = null;
                Previous = null;
            }
        }

        /// <summary>
        /// Gets the previous <typeparamref name="T"/>, or null if none.
        /// </summary>
        public T Previous { get; internal set; }

        /// <summary>
        /// Gets the next <typeparamref name="T"/>, or null if none.
        /// </summary>
        public T Next { get; internal set; }
    }
}
