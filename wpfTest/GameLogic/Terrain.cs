using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public enum Terrain
    {
        LAND,
        SHALLOW_WATER,
        DEEP_WATER
    }

    public enum Biome
    {
        DEFAULT,
        SAVANNA,
        RAINFOREST
    }

    public enum SoilQuality
    {
        LOW,
        MEDIUM,
        HIGH
    }

    public static class BiomeExtensions
    {
        public static SoilQuality Quality(this Biome biome, float nutrients)
        {
            switch (biome)
            {
                case Biome.RAINFOREST:
                    if (nutrients < 6)
                        return SoilQuality.LOW;
                    else if (nutrients < 8)
                        return SoilQuality.MEDIUM;
                    else
                        return SoilQuality.HIGH;
                case Biome.SAVANNA:
                    if (nutrients < 5)
                        return SoilQuality.LOW;
                    else
                        return SoilQuality.MEDIUM;
                default:
                    return SoilQuality.LOW;
            }
        }

        public static float Nutrients(this Biome biome, SoilQuality soilQuality)
        {
            switch (biome)
            {
                case Biome.RAINFOREST:
                    switch (soilQuality)
                    {
                        case SoilQuality.LOW:
                            return 4;
                        case SoilQuality.MEDIUM:
                            return 6;
                        case SoilQuality.HIGH:
                            return 8;
                    }
                    break;
                case Biome.SAVANNA:
                    switch (soilQuality)
                    {
                        case SoilQuality.LOW:
                            return 2;
                        case SoilQuality.MEDIUM:
                            return 5;
                        case SoilQuality.HIGH:
                            throw new ArgumentException("Savanna doesn't have high quality soil!");
                    }
                    break;
                default:
                    return 0;
            }
            throw new NotImplementedException("The case " + biome + " " + soilQuality + " should be implemented!");
        }
    }
}
