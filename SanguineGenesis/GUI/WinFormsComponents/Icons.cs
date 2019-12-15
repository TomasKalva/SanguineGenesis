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

            AddIcons(data.AnimalFactories.Factorys.Select(kvp => kvp.Key), "Images/Icons/Animals/");
            AddIcons(data.TreeFactories.Factorys.Select(kvp => kvp.Key), "Images/Icons/Trees/");
            AddIcons(data.StructureFactories.Factorys.Select(kvp => kvp.Key), "Images/Icons/Structures/");
            AddIcons(data.Statuses.AllStatusFactories.Select(sf => sf.ToString()), "Images/Icons/Statuses/");
        }

        /// <summary>
        /// Adds icons for the names in the directory to icons.
        /// </summary>
        private void AddIcons(IEnumerable<string> names, string directoryName)
        {
            foreach (var name in names)
            {
                try
                {
                    string fileName = directoryName + name.ToLower() + ".png";
                    var bmp = new Bitmap(fileName);
                    icons.Add(name, bmp);
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
