using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Collections
{
    public enum ExpansionMode
    {
        /// <summary>Increment the collection capacity by a set amount each time the capacity is reached.</summary>
        Increment,

        /// <summary>Multiply the collection capacity by a certain amount each time the capacity is reached.</summary>
        Multiply,

        /// <summary>The collection does not expand itself. You must expand it manually if needed.</summary>
        None,
    }
}
