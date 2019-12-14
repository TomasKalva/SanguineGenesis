using SanguineGenesis.GameLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GUI.WinFormsComponents
{
    class Icons
    {
        /// <summary>
        /// Icons for objects in the game.
        /// </summary>
        private Dictionary<string, Bitmap> icons;
        /// <summary>
        /// Empty image returned when no icon exists for the string.
        /// </summary>
        private Bitmap empty;

        public Icons(GameStaticData data)
        {
            icons = new Dictionary<string, Bitmap>();

            foreach(var kvp in data.AnimalFactories.Factorys)
            {
                try
                {
                    string fileName = "Images/Icons/" + kvp.Key.ToLower() + ".png";
                    var bmp = new Bitmap("Images/Icons/" + kvp.Key.ToLower() + ".png");
                    icons.Add(kvp.Key, new Bitmap("Images/Icons/"+ kvp.Key.ToLower() + ".png"));
                }
                catch (Exception)
                {
                    // icon couldn't be loaded
                }
                empty = new Bitmap(1, 1);
            }
        }

        /// <summary>
        /// Returns icon for the name.
        /// </summary>
        public Bitmap GetIcon(string name)
        {
            if(icons.TryGetValue(name, out Bitmap icon))
            {
                return icon;
            }
            else
            {
                return empty;
            }
        }
    }
}
