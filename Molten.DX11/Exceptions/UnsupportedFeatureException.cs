using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class UnsupportedFeatureException : Exception
    {
        public UnsupportedFeatureException(string featureName) : base($"{featureName} is not supported.") { }
    }
}
