namespace Molten
{
    public interface IContentProcessor
    {
        Type GetParameterType();

        object Get(Engine engine, Type contentType, IContentParameters p, IList<object> groupContent);

        void Read(ContentContext context);

        void Write(ContentContext context);

        Type[] AcceptedTypes { get; }

        Type[] RequiredServices { get; }
    }

    /// <summary>
    /// Provides a base implementation for content processors, used by a <see cref="ContentManager"/>.
    /// </summary>
    public abstract class ContentProcessor<P> : IContentProcessor
        where P : class, IContentParameters, new()
    {
        Type IContentProcessor.GetParameterType()
        {
            return typeof(P);
        }

        /// <summary>Invoked when a content retrieval request was called with metadata tags (prefixed with @) or when multiple objects are available which were loaded from the same file.</summary>
        /// <param name="engine">The <see cref="Engine"/> instance that content should be bound to.</param>
        /// <param name="contentType">The type of content being loaded.</param>
        /// <param name="parameters">An array of parameters that were attached to the request.</param>
        /// <param name="groupContent">A list of viable objects which match the requested filename</param>
        /// <returns></returns>
        object IContentProcessor.Get(Engine engine, Type contentType, IContentParameters p, IList<object> groupContent)
        {
            P pt = (P)p;
            return OnGet(engine, contentType, pt, groupContent);
        }

        void IContentProcessor.Read(ContentContext context)
        {
            P pt = (P)context.Parameters;
            OnRead(context, pt);
        }

        void IContentProcessor.Write(ContentContext context)
        {
            P pt = (P)context.Parameters;
            OnWrite(context, pt);
        }

        /// <summary>Invoked when a content retrieval request was called with metadata tags (prefixed with @) or when multiple objects are available which were loaded from the same file.</summary>
        /// <param name="engine">The <see cref="Engine"/> instance that content should be bound to.</param>
        /// <param name="contentType">The type of content being loaded.</param>
        /// <param name="parameters">An array of parameters that were attached to the request.</param>
        /// <param name="groupContent">A list of viable objects which match the requested filename</param>
        /// <returns></returns>
        protected virtual object OnGet(Engine engine, Type contentType, P parameters, IList<object> groupContent) { return groupContent[0]; }

        /// <summary>
        /// Invoked when content matching <see cref="AcceptedTypes"/> needs to be read.
        /// </summary>
        /// <param name="context">The <see cref="ContentContext"/> with which to read content.</param>
        protected abstract void OnRead(ContentContext context, P p);

        /// <summary>
        /// Invoked when content matching <see cref="AcceptedTypes"/> needs to be written.
        /// </summary>
        /// <param name="context">The <see cref="ContentContext"/> with which to write content.</param>
        protected abstract void OnWrite(ContentContext context, P p);

        /// <summary>Gets a list of accepted </summary>
        public abstract Type[] AcceptedTypes { get; }

        /// <summary>
        /// Gets a list of types for required <see cref="EngineService"/>. 
        /// If the bound <see cref="Engine"/> instance is missing any of them, the content processor will not be used.
        /// </summary>
        public abstract Type[] RequiredServices { get; }
    }
}
