using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    class Map
    {
        public Node[,] Nodes { get; private set; }
        public int Width => Nodes.GetLength(0);
        public int Height => Nodes.GetLength(1);

        public Map(PixelColor[,] map)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);
            Nodes = new Node[width, height];
            for(int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    PixelColor pc = map[i, j];
                    if (pc.Blue==255 && 
                        pc.Red ==0 &&
                        pc.Green == 0)
                    {
                        Nodes[i, j] = new Node(Terrain.WATER,i,j);
                    }
                    else
                    {
                        Nodes[i, j] = new Node(Terrain.WATER,i,j);
                    }
                    //Console.WriteLine(i+";"+j+"(" +pc.Blue+","+pc.Green+")");
                }
            }
        }
    }
}
