﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Contains extension methods for System.Drawing.Color.
    /// </summary>
    static class ColorExtensionMethods
    {
        /// <summary>
        /// Returns true if the colors a and b have the same rgb components.
        /// </summary>
        public static bool SameRGB(this Color a, Color b)=> a.R == b.R && a.G == b.G && a.B == b.B;
    }
}
