namespace Molten
{
    public interface IContentProcessor
    {
        bool ReadPart(ContentLoadHandle handle, Stream stream);

        bool BuildAsset(ContentLoadHandle assetHandle);

        bool Write(ContentSaveHandle handle, Stream stream);

        Type[] AcceptedTypes { get; }

        Type[] RequiredServices { get; }

        Type ParameterType { get; }

        Type PartType { get; }
    }
}
