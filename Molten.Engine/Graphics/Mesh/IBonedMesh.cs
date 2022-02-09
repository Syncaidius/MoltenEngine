namespace Molten.Graphics
{
    public interface IBonedMesh<T> : IIndexedMesh<T> where T : unmanaged, IVertexType
    {
        void SetBones<T>(T[] data) where T : unmanaged;

        void SetBones<T>(T[] data, uint count) where T : unmanaged;

        void SetBones<T>(T[] data, uint startIndex, uint count) where T : unmanaged;

        ///// <summary>Gets or sets a bone.<value>
        //IMeshBone this[string boneName] { get; set; }
    }
}
