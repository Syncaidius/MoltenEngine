using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        /// <summary>
        /// A struct which represents sprite data to be sent to the GPU via a structured buffer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        protected struct GpuData
        {
            public struct ExtraData
            {
                public float D1;
                public float D2;
                public float D3;
                public float D4;
            }

            public Vector4F UV;

            public Color4 Color;

            public Color4 Color2;

            /// <summary>
            /// A field for storing extra data about the sprite
            /// </summary>
            public ExtraData Extra;

            public Vector2F Position;

            public Vector2F Size;

            public Vector2F Origin;

            /// <summary>
            /// The rotation.
            /// </summary>
            public float Rotation;

            /// <summary>
            /// The rotation (X) and array slice index (Y).
            /// </summary>
            public float ArraySlice;
        }
    }
}
