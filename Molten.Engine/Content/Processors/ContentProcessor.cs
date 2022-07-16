namespace Molten
{
    public interface IContentProcessor
    {
        Type GetParameterType();

        bool Read(ParameterizedContentHandle handle, object existingAsset, out object asset);

        bool Write(ParameterizedContentHandle handle, object asset);

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

        bool IContentProcessor.Read(ParameterizedContentHandle handle, object existingAsset, out object asset)
        {
            P parameters = (P)handle.Parameters;
            return OnRead(handle, parameters, existingAsset, out asset);
        }

        bool IContentProcessor.Write(ParameterizedContentHandle handle, object asset)
        {
            P parameters = (P)handle.Parameters;
            return OnWrite(handle, parameters, asset);
        }

        /// <summary>
        /// Invoked when content matching <see cref="AcceptedTypes"/> needs to be read.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="parameters"></param>
        /// <param name="existingAsset"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        protected abstract bool OnRead(ParameterizedContentHandle handle, P parameters, object existingAsset, out object asset);

        /// <summary>
        /// Invoked when content matching <see cref="AcceptedTypes"/> needs to be written.
        /// </summary>
        /// <param name="handle">A <see cref="ParameterizedContentHandle"/> which holds the currnet state of an asset.</param>
        /// <param name="parameters"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        protected abstract bool OnWrite(ParameterizedContentHandle handle, P parameters, object asset);

        /// <summary>Gets a list of accepted </summary>
        public abstract Type[] AcceptedTypes { get; }

        /// <summary>
        /// Gets a list of types for required <see cref="EngineService"/>. 
        /// If the bound <see cref="Engine"/> instance is missing any of them, the content processor will not be used.
        /// </summary>
        public abstract Type[] RequiredServices { get; }
    }
}
