﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    [Flags]
    public enum UIInputRuleFlags
    {
        /// <summary>
        /// No input accepted.
        /// </summary>
        None = 0,

        /// <summary>
        /// Input on self is accepted.
        /// </summary>
        Self = 1,

        /// <summary>
        /// Input on child elements is accepted.
        /// </summary>
        Children = 2,

        /// <summary>
        /// Input on compound elements is accepted.
        /// </summary>
        Compound = 4,

        /// <summary>
        /// All types of input are accepted.
        /// </summary>
        All = Self | Children | Compound,
    }
}