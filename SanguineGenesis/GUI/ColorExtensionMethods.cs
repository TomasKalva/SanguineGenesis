using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GUI
{
    static class ColorExtensionMethods
    {
        /// <summary>
        /// Returns true if the colors a and b have the same rgb components. Returns false if at
        /// least one of them is null.
        /// </summary>
        private static bool TheSameRGB(this Color a, Color b)
        {
            if (a == null || b == null)
                return false;
            else
                return a.R == b.R && a.G == b.G && a.B == b.B;
        }
    }
}
