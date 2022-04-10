using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public class MSDFGeneratorConfig : GeneratorConfig
    {
        public ErrorCorrectionConfig ErrorCorrection;

        public MSDFGeneratorConfig() { }

        public MSDFGeneratorConfig(bool overlapSupport, ErrorCorrectionConfig errorCorrection) : 
            base(overlapSupport)
        {
            ErrorCorrection = errorCorrection;
        }
    }
}
