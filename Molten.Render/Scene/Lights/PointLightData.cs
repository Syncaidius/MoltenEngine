using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Scene.Lights
{
    /// <summary>
    /// A vertex structure for storing point light data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PointLightData : ILightData, IVertexType
    {
        /// <summary>
        /// The light position.
        /// </summary>
        public Vector3F Position;

        /// <summary>
        /// The reciprocal light range.
        /// </summary>
        public float RangeRcp;

        /// <summary>
        /// The light color, as 3 floats.
        /// </summary>
        public Color3 Color3;

        /// <summary>
        /// The light intensity.
        /// </summary>
        public float Intensity;

        /// <summary>
        /// The light transform.
        /// </summary>
        public Matrix4F Transform;

        /// <summary>
        /// The tessellation factor. A Factor of 0 will disable the light.
        /// </summary>
        public float TessFactor;

        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        public Color Color
        {
            get => (Color)Color3;
            set => Color3 = (Color3)value;
        }

        public void Remove()
        {
            TessFactor = 0;
        }
    }
}
