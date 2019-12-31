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

        public Icons()
        {
            icons = new Dictionary<string, Bitmap>();

            AddIcons("Images/Icons/Animals/");
            AddIcons("Images/Icons/Trees/");
            AddIcons("Images/Icons/Structures/");
            AddIcons("Images/Icons/Statuses/");
            AddIcons("Images/Icons/Abilities/");
        }

        /// <summary>
        /// Adds icons for the names in the directory to icons.
        /// </summary>
        private void AddIcons(string directoryName)
        {
            foreach(var name in Directory.GetFiles(directoryName)
                                        .Select(Path.GetFileNameWithoutExtension))
            //foreach (var name in names)
            {
                try
                {
                    string fileName = directoryName + name.ToLower() + ".png";
                    var bmp = new Bitmap(fileName);
                    icons.Add(name.ToUpper(), bmp);
                }
                catch (Exception)
                {
                    // icon couldn't be loaded - problem with opening file or duplicate key
                }
            }
            empty = new Bitmap(1, 1);
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
