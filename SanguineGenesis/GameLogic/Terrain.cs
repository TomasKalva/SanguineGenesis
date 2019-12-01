using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis
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
        BAD,
        LOW,
        MEDIUM,
        HIGH
    }

    public static class TerrainExtensions
    {
        /// <summary>
        /// Returns soil quality determined by the terrain, biome, nutrients combination.
        /// Throws argument exception if the combination is not valid.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid.</exception>
        public static SoilQuality Quality(this Terrain terrain, Biome biome, float nutrients)
        {
            switch (terrain)
            {
                case Terrain.LAND:
                    switch (biome)
                    {
                        case Biome.RAINFOREST:
                            if (nutrients < 4)
                                return SoilQuality.BAD;
                            else if (nutrients < 6)
                                return SoilQuality.LOW;
                            else if (nutrients < 8)
                                return SoilQuality.MEDIUM;
                            else
                                return SoilQuality.HIGH;
                        case Biome.SAVANNA:
                            if (nutrients < 2)
                                return SoilQuality.BAD;
                            else if (nutrients < 5)
                                return SoilQuality.LOW;
                            else
                                return SoilQuality.MEDIUM;
                        default:
                            return SoilQuality.BAD;
                    }
                case Terrain.SHALLOW_WATER:
                    switch (biome)
                    {
                        case Biome.DEFAULT:
                            return SoilQuality.BAD;
                        case Biome.SAVANNA:
                            return SoilQuality.LOW;
                        case Biome.RAINFOREST:
                            return SoilQuality.LOW;
                    }
                    break;
                case Terrain.DEEP_WATER:
                    switch (biome)
                    {
                        case Biome.DEFAULT:
                            return SoilQuality.BAD;
                        case Biome.SAVANNA:
                            return SoilQuality.LOW;
                        case Biome.RAINFOREST:
                            return SoilQuality.LOW;
                    }
                    break;

            }
            throw new ArgumentException("Combination " + terrain + ", " + biome + ", " + nutrients + " isn't valid");
        }

        /// <summary>
        /// Returns minimal amount of nutrients determined by the terrain, biome, soil quality combination.
        /// Throws argument exception if the combination is not valid.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid.</exception>
        public static float Nutrients(this Terrain terrain, Biome biome, SoilQuality soilQuality)
        {
            switch (terrain) {
                case Terrain.LAND:
                    switch (biome)
                    {
                        case Biome.RAINFOREST:
                            switch (soilQuality)
                            {
                                case SoilQuality.BAD:
                                    return 0;
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
                                case SoilQuality.BAD:
                                    return 0;
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
                    break;
                case Terrain.SHALLOW_WATER:
                    switch(biome)
                    {
                        case Biome.DEFAULT:
                            return 0;
                        case Biome.SAVANNA:
                            return 2;
                        case Biome.RAINFOREST:
                            return 2;
                    }
                    break;
                case Terrain.DEEP_WATER:
                    switch (biome)
                    {
                        case Biome.DEFAULT:
                            return 0;
                        case Biome.SAVANNA:
                            return 2;
                        case Biome.RAINFOREST:
                            return 2;
                    }
                    break;
            }
            throw new ArgumentException("Combination " + terrain + ", " + biome + ", " + soilQuality + " isn't valid");
        }
    }

    public static class SoilQualityExtensions
    {
        /// <summary>
        /// Returns amount of nutrients that can be produced by a Node with soilQuality.
        /// </summary>
        public static float NutrientsProduction(this SoilQuality soilQuality)
        {
            switch (soilQuality)
            {
                case SoilQuality.BAD:
                    return 0.3f;//0.05f;
                case SoilQuality.LOW:
                    return 0.5f;//0.01f;
                case SoilQuality.MEDIUM:
                    return 0.7f;//0.02f;
                case SoilQuality.HIGH:
                    return 1f;//0.03f;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Returns amount of nutrients that can be transfered by a Node with soilQuality.
        /// </summary>
        public static float TransferCapacity(this SoilQuality soilQuality)
        {
            switch (soilQuality)
            {
                case SoilQuality.BAD:
                    return 0f;
                case SoilQuality.LOW:
                    return 0.05f;
                case SoilQuality.MEDIUM:
                    return 0.005f;
                case SoilQuality.HIGH:
                    return 0f;
                default:
                    return 0f;
            }
        }
    }
}
