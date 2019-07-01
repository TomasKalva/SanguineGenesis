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
    public class Map
    {
        public Node[,] Nodes { get; private set; }
        public int Width => Nodes.GetLength(0);
        public int Height => Nodes.GetLength(1);

        public WriteableBitmap MapImage { get; private set; }
        public int NodeSize { get; private set; }
        private Dictionary<Terrain, BitmapImage> tileImages;

        public Map(PixelColor[,] map, MainWindow resources)
        {
            LoadTileImages(resources);
            NodeSize = 64;
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
                        Nodes[i, j] = new Node(Terrain.GRASS,i,j);
                    }
                    //Console.WriteLine(i+";"+j+"(" +pc.Blue+","+pc.Green+")");
                }
            }
            InitializeMapImage();
        }

        private void LoadTileImages(MainWindow resources)
        {
            tileImages = new Dictionary<Terrain, BitmapImage>();
            tileImages.Add(Terrain.GRASS, (BitmapImage)resources.FindResource("grass"));
            tileImages.Add(Terrain.WATER, (BitmapImage)resources.FindResource("water"));
        }

        private void InitializeMapImage()
        {
            int width = Width*NodeSize;
            int height = Height*NodeSize;
            MapImage = new WriteableBitmap(width,height,96,96, PixelFormats.Bgra32, null);
            

            // Create an array of pixels to contain pixel color values
            uint[] pixels = new uint[width * height];


            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    BitmapImage tileIm = tileImages[Nodes[x, y].Terrain];
                    PixelColor[,] tilePixels = tileIm.GetPixels();
                    for (int i = 0; i < NodeSize; i++)
                    {
                        for (int j = 0; j < NodeSize; j++)
                        {
                            int arrInd = (y * NodeSize + j) * Width * NodeSize + x * NodeSize + i;
                            PixelColor pc = tilePixels[i, j];
                            pixels[arrInd] = (uint)((pc.Blue) + (pc.Green << 8) + (pc.Red << 16) + (pc.Alpha << 24));
                        }
                    }
                }
            }

            /*int red;
            int green;
            int blue;
            int alpha;

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int i = width * y + x;

                    red = 255;
                    green = 0;// * y / height;
                    blue = 0;// * (width - x) / width;
                    alpha = 255;
                    pixels[i] = (uint)((blue) + (green << 8) + (red << 16) + (alpha << 24));
                }
            }*/

            // apply pixels to bitmap
            MapImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
        }
    }
}
