namespace Molten
{
    /// <summary>A helper class for handling the sharing of values across multiple instances. Extremely useful for static variables that need to be disposed correctly once
    /// all user classes have finished with it.</summary>
    /// <typeparam name="T"></typeparam>
    public class ReferencedObject<T> where T : class
    {
        public T Value = default(T);

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencedObject{T}"/> class and sets the reference count to 1.
        /// </summary>
        public ReferencedObject() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencedObject{T}"/> class and sets the reference count to 1.
        /// </summary>
        /// <param name="value">The initial value.</param>
        public ReferencedObject(T value)
        {
            Value = value;
            ReferenceCount = 1;
        }

        /// <summary>Register a reference to the object.</summary>
        public void Reference()
        {
            ReferenceCount++;
        }

        /// <summary>Register a de-reference to the object. If the reference count hits 0, the object will be disposed and dereferenced completely.</summary>
        public void Dereference()
        {
            ReferenceCount--;
            if (ReferenceCount == 0)
            {
                IDisposable d = Value as IDisposable;
                if (d != null)
                    d.Dispose();

                Value = null;
            }
        }

        public static implicit operator T(ReferencedObject<T> refObject)
        {
            return refObject.Value;
        }

        public int ReferenceCount { get; private set; }
    }
}
