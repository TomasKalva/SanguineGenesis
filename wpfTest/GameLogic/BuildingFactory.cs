using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class BuildingFactory
    {
        public EntityType UnitType { get; }
        public int Size { get; }
        public float MaxHealth { get; }
        public float MaxEnergy { get; }
        public SoilQuality SoilQuality { get; }
        public float BuildingTime { get; }

        public Building NewInstance(Player player, Node[,] nodes)
        {
            return new Building(player, UnitType, maxHealth: MaxHealth, viewRange:10, maxEnergy: MaxEnergy, nodes: nodes, size: Size, soilQuality:SoilQuality, buildingTime:BuildingTime);
        }

        public BuildingFactory(EntityType unitType, int size, float maxHealth, float maxEnergy, SoilQuality soilQuality, float buildingTime)
        {
            Size = size;
            MaxHealth = maxHealth;
            MaxEnergy = maxEnergy;
            UnitType = unitType;
            SoilQuality = soilQuality;
            BuildingTime = buildingTime;
        }
    }
}
