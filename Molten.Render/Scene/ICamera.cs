using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ICamera
    {
        Matrix View { get; }

        Matrix Projection { get; }

        Matrix ViewProjection { get; }

        IRenderSurface OutputSurface { get; set; }

        IDepthSurface OutputDepthSurface { get; set; }

        float MinimumDrawDistance { get; set; }

        float MaximumDrawDistance { get; set; }
    }
}
