namespace Molten.Graphics
{
    public interface IBonedMesh<T, B> : IIndexedMesh<T> 
        where T : unmanaged, IVertexType
        where B : unmanaged
    {
        void SetBones(B[] data);

        void SetBones(B[] data, uint count);

        void SetBones(B[] data, uint startIndex, uint count);

        ///// <summary>Gets or sets a bone.<value>
        //IMeshBone this[string boneName] { get; set; }
    }
}
