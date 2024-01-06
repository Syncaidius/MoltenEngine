namespace Molten;

/// <summary>
/// Provides a base implementation for content processors, used by a <see cref="ContentManager"/>.
/// </summary>
public abstract class ContentProcessor<P> : IContentProcessor
    where P : ContentParameters, new()
{
    public Type ParameterType => typeof(P);

    public abstract Type PartType { get; }

    bool IContentProcessor.ReadPart(ContentLoadHandle handle, Stream stream)
    {
        P parameters = (P)handle.Parameters;
        return OnReadPart(handle, stream, parameters, handle.Asset, out handle.Asset);
    }

    bool IContentProcessor.BuildAsset(ContentLoadHandle handle)
    {
        P parameters = (P)handle.Parameters;
        return OnBuildAsset(handle, handle.PartInfo.Handles, parameters, handle.Asset, out handle.Asset);
    }

    bool IContentProcessor.Write(ContentSaveHandle handle, Stream stream)
    {
        P parameters = (P)handle.Parameters;
        return OnWrite(handle, stream, parameters, handle.Asset);
    }

    /// <summary>
    /// Invoked when content matching <see cref="AcceptedTypes"/> needs to be read.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="parameters"></param>
    /// <param name="partAsset"></param>
    /// <returns></returns>
    protected abstract bool OnReadPart(ContentLoadHandle handle, Stream stream, P parameters, object existingPart, out object partAsset);

    protected abstract bool OnBuildAsset(ContentLoadHandle handle, ContentLoadHandle[] parts, P parameters, object existingAsset, out object asset);

    /// <summary>
    /// Invoked when content matching <see cref="AcceptedTypes"/> needs to be written.
    /// </summary>
    /// <param name="handle">A <see cref="ParameterizedContentHandle"/> which holds the currnet state of an asset.</param>
    /// <param name="parameters"></param>
    /// <param name="asset"></param>
    /// <returns></returns>
    protected abstract bool OnWrite(ContentHandle handle, Stream stream, P parameters, object asset);

    /// <summary>Gets a list of accepted </summary>
    public abstract Type[] AcceptedTypes { get; }

    /// <summary>
    /// Gets a list of types for required <see cref="EngineService"/>. 
    /// If the bound <see cref="Engine"/> instance is missing any of them, the content processor will not be used.
    /// </summary>
    public abstract Type[] RequiredServices { get; }
}
