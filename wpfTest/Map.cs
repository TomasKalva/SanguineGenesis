using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace wpfTest
{
    public class Map:IMap<Node>
    {
        private Node[,] nodes { get; set; }
        public Node this[int i, int j]
        {
            get => nodes[i, j];
            set => nodes[i, j] = value;
        }
        public int Width => nodes.GetLength(0);
        public int Height => nodes.GetLength(1);

        public Map(PixelColor[,] map)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);
            nodes = new Node[width, height];
            for(int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    PixelColor pc = map[i, j];
                    if (pc.Blue==255 && 
                        pc.Red ==0 &&
                        pc.Green == 0)
                    {
                        nodes[i, j] = new Node(i, j,Terrain.WATER);
                    }
                    else
                    {
                        nodes[i, j] = new Node(i, j, Terrain.GRASS);
                    }
                }
            }
        }
    }
}
