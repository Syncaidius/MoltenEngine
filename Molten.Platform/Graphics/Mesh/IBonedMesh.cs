namespace Molten.Graphics
{
    public interface IBonedMesh<T> : IIndexedMesh<T> where T : struct, IVertexType
    {
        void SetBones<T>(T[] data) where T : struct;

        void SetBones<T>(T[] data, int count) where T : struct;

        void SetBones<T>(T[] data, int startIndex, int count) where T : struct;

        ///// <summary>Gets or sets a bone.<value>
        //IMeshBone this[string boneName] { get; set; }
    }
}
