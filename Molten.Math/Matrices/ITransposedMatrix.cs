using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public interface ITransposedMatrix<T> where T : unmanaged
    {
        void Transpose(out T result);
    }
}
