using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public interface IKeyboardDevice : IInputDevice<Key>
    {
        event KeyPressHandler OnCharacterKey;
    }
}
