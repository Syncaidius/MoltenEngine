using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public interface IInputAcceptor : ISceneObject
    {
        /// <summary>Called when a scene requires the object to handle input detection.</summary>
        /// <param name="inputPos">The input position.</param>
        /// <returns></returns>
        IInputAcceptor HandleInput(Vector2F inputPos);
    }
}
