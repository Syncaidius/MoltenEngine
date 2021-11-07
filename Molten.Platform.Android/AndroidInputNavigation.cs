using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Molten.Input;
using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten
{
    public class AndroidInputNavigation : IInputNavigation
    {
        public bool IsBackPressed => false;

        public bool IsContextButtonPressed => throw new NotImplementedException();

        public event MoltenEventHandler<IInputNavigation> OnBackPressed;
        public event MoltenEventHandler<IInputNavigation> OnContextButtonPressed;

        internal AndroidInputNavigation()
        {

        }

        internal void Update(Timing time)
        {

        }
    }
}