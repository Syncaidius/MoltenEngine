using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    /// <summary>
    /// A struct which represents sprite data on both the CPU and GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpriteGpuData
    {
        public struct ExtraData
        {
            public float D1;
            public float D2;
            public float D3;
            public float D4;
        }

        public Vector2F Position;

        public Vector2F Size;

        public Vector2F Origin;

        public Vector4F UV;

        /// <summary>
        /// The rotation.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The rotation (X) and array slice index (Y).
        /// </summary>
        public float ArraySlice;

        public Color4 Color;

        /// <summary>
        /// A field for storing extra data about the sprite
        /// </summary>
        public ExtraData Data;
    }
}
