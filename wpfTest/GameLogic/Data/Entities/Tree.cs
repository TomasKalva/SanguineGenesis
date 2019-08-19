using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class Tree:Building, IHerbivoreFood
    {
        public int Air { get; }

        bool IFood.FoodLeft => !IsDead;

        public Tree(Player player, string treeType, Node[,] nodes, Node[,] rootNodes, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool aggressive, float viewRange, int air, List<Ability> abilities)
            : base(player, treeType, nodes, rootNodes, maxHealth, maxEnergy, maxEnergyIntake, size, physical, biome, terrain, soilQuality, aggressive, viewRange, abilities)
        {
            Air = air;
            foreach(Node n in rootNodes)
                n.Roots.Add(this);
        }

        void IFood.EatFood(Animal eater)
        {
            decimal nutrientsToEat = Math.Min(eater.FoodEnergyRegen, Health);
            Health -= nutrientsToEat;
            eater.Energy += nutrientsToEat;
        }

        public override void Die()
        {
            base.Die();

            //after tree dies and has energy left, spawn a dead tree
            if(Energy > 0)
                Player.Entities.Add(
                    new Structure(Player, "DEAD_TREE", Nodes, EnergySources, MaxEnergy, 0, 0, Size,
                    Physical, Biome, Terrain, SoilQuality.BAD, false, 0, new List<Ability>()));
        }
    }
}
