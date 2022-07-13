using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten.UI
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class UIThemeMemberAttribute : Attribute
    {

    }
}
