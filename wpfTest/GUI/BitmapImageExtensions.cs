using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace wpfTest
{
    static class BitmapImageExtensions
    {
        public static PixelColor[,] GetPixels(this BitmapSource source)
        {
            int offset = 0;
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, width * 4, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            PixelColor[,] pixels = new PixelColor[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[x + x0, height - 1 - y + y0] = new PixelColor
                    {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
            return pixels;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;

        public int RGB => ((int)Red << 16) | ((int)Green << 8) | (int)Blue;

        public PixelColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = 0;
        }

    }
}
